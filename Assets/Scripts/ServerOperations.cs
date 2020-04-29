/**************************************************************************************/
//  Module written by scaroni <renato.scaroni@gmail.com>
//  Rewrited by Josi Perez <josiperez.neuromat@gmail.com>
//
//	Responsible for making all http requests in all environments (standalone, android and web)
//  to save results
//  Time.deltaTime is a float representing the difference (or the delta) in time (seconds)
//                 since the last update (or frame) occurred. 
/**************************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;				
using JsonFx.Json;				        //iOS JsonFx: http://answers.unity3d.com/questions/502124/importing-jsonfx.html
using System.Text;                      //StringBuilder
using System.Security.Cryptography;     //170830 sha512Hash available

//using UnityEngine.Networking;         //180126 get ipAddress, but return Intranet IP if is the case; 
                                        //       strategy changed for read myip.com app site in LocalizationManager 
//using UnityEditor;	    	        //170123: FileUtil: copy to backupResults, move to append .csv, etc
//                                                DO NOT USE! NO WORK IN BUILD! SAME CASE AS BEEP
//using System.Runtime.InteropServices  //[DLLimport  170609
//                                      //not necessary anymore; changed by Application.ExternalEval("_JS_FileSystem_Sync();");
//using System.Globalization;           //170713 to use CultureInfo as locale


          
//------------------------------------------------------------------------------------
public class ServerOperations 
{
	//170612 where to save results if Android, iOS or WebGL
	public static readonly string gameServerLocation = "game.numec.prp.usp.br/game/";     //170928 from Carlos Ribas
	public static readonly string webProtocol = "https://";
//	string fileContent;    //contains all data results, before to send to web


	//170901 playerMachine was 64 chars long, but not enough for mobiles
	//171124 SHA512(MD5(identifierID)) has size 128
	#if UNITY_ANDROID || UNITY_IOS
	//      starts with length 280 but can increase until 320
	private StringBuilder LogGame = new StringBuilder(280, 320);  //120 path + 20 stageID + 128 machine + 20 date/hour + 4 .csv
	#else
	//      starts with length 180 but can increase until 220
	private StringBuilder LogGame = new StringBuilder(180, 220);  
	#endif


	private StringBuilder gamePlayed = new StringBuilder(4, 4);   //161212: _JG_ or AQ, AR, JM
	private string backupResults;                                 //170622 to save a copy of results
	private string resultsFileName;                               //170622 to reread the saved file, if WEBGL, and save the name
	private string resultsFileContent;                            //170622 to reread the saved file, if WEBGL, and save the content

	private string tmp;                                           //170124 
	private int line = 0;                                         //170213 para inserir numero da linha no arquivo (uma sequencia)
//	private string gameCommonData;                                //170213 para inserir em cada linha do arquivo de resultado
//	private string fileHeader;                                    //170217 para manter apenas linhas de resutados e acrescentar dados em colunas
	private bool casoEspecialInterruptedOnFirstScreen;            //170223 detectar dif entre interromper na firstScreen ou no jogo, no JM


	//--------------------------------------------------------------------------------------
	static private ServerOperations _instance;
	static public ServerOperations instance
	{
		get {
			if(_instance == null) {
				_instance = new ServerOperations();
			}
			return _instance;
		}
	}


	// -------------------------------------------------------------------------------------
	//Josi: 161205: acrescido parametro sobre o modo de operacao do jogo
	//170126 added param bmMinHits 
	//170310 added phaseNumber
	//170316 added endSessionTime
	//170622 added showHistory
	//170629 researchGroup (now groupCode)
	//171025 choices and showPlayPauseButton
	//180117 locale
	//180326 new parameters: minHitsInSequenceForJG, ForJM, mdMaxPlays
	//180417 send speedAnim
	//180419 write keyboardTimeMarkers
	public void RegisterPlay (MonoBehaviour mb, string locale, float endSessionTime, string stageID, bool gameMode, int phaseNumber, int totalPlays, int totalCorrect, float successRate,
		int bmMinHits, int bmMaxPlays, int bmMinHitsInSequence, List<RandomEvent> log, bool interrupted, List<RandomEvent> firstScreenMD, string animationType,
		int playsToRelax,
		bool showHistory,
		string sendMarkersToEEG,
		string portEEGserial,
		string groupCode,
		bool scoreboard,
		string finalScoreboard,
		string treeContextsAndProbabilities,
		int choices,
		bool showPlayPauseButton,
		int jgMinHitsInSequence,
		int mdMinHitsInSequence,
		int mdMaxPlays,
		string institution,
		bool attentionPoint,
		string attentionDiameter,
		string attentionColorStart,
		string attentionColorCorrect,
		string attentionColorWrong,
		string speedGKAnim,

		string portSendData,
		string timeFaixa0,
		string timeFaixa1,
		string timeFaixa2,
		string timeFaixa3,
		string timeFaixa4,

		float[] keyboardTimeMarkers
	)

	{
		//170123 Garantir que existe o diretório de backup dos resultados
		//       http://answers.unity3d.com/questions/528641/how-do-you-create-a-folder-in-c.html
		//       backupResults = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/backupResults";
		//170608 if WEB do not create directory
		//170622 mais seguro perguntar desta maneira, sem diretivas de compilacao
		//170818 if Android do not save in a local directory
		//171123 if iOS do not save in a local directory
		if ((Application.platform != RuntimePlatform.WebGLPlayer) && (Application.platform != RuntimePlatform.Android) &&
			(Application.platform != RuntimePlatform.IPhonePlayer) && (!SystemInfo.deviceModel.Contains("iPad"))) {
			//170622 concentrate resultsBk here
			backupResults = Application.dataPath + "/ResultsBk";

			try {
				if (!Directory.Exists (backupResults)) {
					Directory.CreateDirectory (backupResults);
				}
			} catch (IOException ex) {
				//171011 to see in output.txt
				Debug.Log ("Error creating Results Backup directory (ResultsBk): " + ex.Message);
			}
		}



		//Josi: using StringBuilder; based https://forum.unity3d/threads/how-to-write-a-file.8864
		//      caminhoLocal/Plays_grupo1-v1_HP-HP_YYMMDD_HHMMSS_fff.csv
		//      string x stringBuilder: em https://msdn.microsoft.com/en-us/library/system.text.stringbuilder(v=vs.110).aspx
		int gameSelected = PlayerPrefs.GetInt ("gameSelected");


		gamePlayed.Length = 0;                        //mantem a capacity, LogGame="" e apontador comeca do zero
		switch (gameSelected) {                       //Josi: 161212: indicar no server, arquivo (nome e conteudo), o jogo jogado
		case 1:
			gamePlayed.Append ("_AQ_");   //Base Motora: Aquecimento
			break;
		case 2:
			gamePlayed.Append ("_JG_");   //Jogo do Goleiro
			break;
		case 3:
			gamePlayed.Append ("_MD_");   //Base memória (memória declarativa): reconhece sequ por teclado
			break;
		case 4:
			gamePlayed.Append ("_AR_");   //Base motora com Tempo: Aquecimento com relogio
			break;
		case 5:
			gamePlayed.Append ("_JM_");   //Jogo da Memória (MD sem input por teclado; jogador fala para o experimentador)
			break;
		}


		//170607 playerMachine not valid for build webGL; using directives
		//       https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
		string playerMachine;
		if (Application.platform == RuntimePlatform.WebGLPlayer) {
			playerMachine = "WEBGL";
		} else { 
			//170818 do not have device name; ​​SystemInfo.deviceUniqueIdentifier? Android is androidID
			//171123 iOS
			if ((Application.platform == RuntimePlatform.Android) ||
				(Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad"))) { 
				playerMachine = SystemInfo.deviceUniqueIdentifier; 

				//170830 identifierID comes as MD5, easy to open, then, 
				//       let's encebol with a hash512, fast and unbreakable until now... just large... 128 bytes...
				string hash = GetHash(playerMachine);
				playerMachine = hash;

			} else {
				playerMachine = System.Net.Dns.GetHostName ().Trim ();
			}
		}

		tmp = (1000 + UnityEngine.Random.Range (0, 1000)).ToString().Substring(1,3);  //170126: a random between 000 e 999
		LogGame.Length = 0;                                           //keeps capacity, LogGame="" and pointer starts at zero

		//170608 if webGL needs to save in an free area
		//170622 without using directives
		//170818 where to save in Android
		//171122 iOS (iPad/iPhone)
		if ((Application.platform == RuntimePlatform.WebGLPlayer) || (Application.platform == RuntimePlatform.Android) ||
			(Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad")) ) {
			LogGame.Append(Application.persistentDataPath);   	      //web IndexedDB or where the local browser permits write
		} else {
			LogGame.Append(Application.dataPath);   			      //local path
		}
		LogGame.Append("/Plays");                                     //start file name, Plays_ - CAI O UNDERSCORE!
		LogGame.Append(gamePlayed);                                   //161212game played
		LogGame.Append(stageID.Trim());                               //game phase
		LogGame.Append("_");                                          //separator

		//170607 nomeDaMaquina is environment dependent
		//LogGame.Append(System.Net.Dns.GetHostName().Trim());        //nome do host: not valid in all environments
		LogGame.Append(playerMachine);                                //nome do host

		LogGame.Append("_");                                          //sep
		LogGame.Append(DateTime.Now.ToString("yyMMdd_HHmmss_"));      //170116: data (6)_hour(6)_
		LogGame.Append(tmp);                                          //170126: random between 000 e 999			

		//Josi: open/write/close file
		if (!File.Exists (LogGame.ToString () + ".csv")) {            //Josi: no more StringBuilder... 
			line = 0;                                                 //inicialize line counter at each new result file
			casoEspecialInterruptedOnFirstScreen = false;             //170223 inicializar a cada gravacao

			var sr = File.CreateText (LogGame.ToString ());

			//Josi: 161205: including operation mode on results 
			if (gameMode) {
				tmp = "readSequence";
			} else {
				tmp = "readTree";
			}

			//170712 saving in a simple way: common data in a style "variable,content";
			//171114 language the user's operating system is running in + user operatingSystem
			//171123 including deviceModel
			//180117 add locale selected by player
			//180226 operatingSystem could have commas like in "iPad4,2" or "iMac12,1" destroying the CSV format
			sr.WriteLine ("currentLanguage,{0}", Application.systemLanguage.ToString ());
			sr.WriteLine ("operatingSystem,{0} [{1}]", SystemInfo.deviceModel.Replace (",", "."), SystemInfo.operatingSystem.Replace (",", "."));

			//180126 IPAddress Prof Gubi idea  (can known the machine, not the player! privacy respected!)
			//var ipaddress = Network.player.externalIP; //return Intranet IP if is the case... 
			//variables loaded on localizationManager
			sr.WriteLine ("ipAddress,{0}", PlayerPrefs.GetString ("IP"));
			sr.WriteLine ("ipCountry,{0}", PlayerPrefs.GetString ("Country"));

			//180402 save the program version used
			sr.WriteLine ("gameVersion,{0}", PlayerPrefs.GetString ("version"));
			sr.WriteLine ("gameLanguage,{0}", locale);

			//----------------------------------------------------------------------
			sr.WriteLine ("institution,{0}", institution);                               //180403 integration NES (new)
			sr.WriteLine ("soccerTeam,{0}", PlayerPrefs.GetString ("teamSelected"));    //180403 old experimentalGroup
			sr.WriteLine ("game,{0}", gamePlayed.ToString ().Substring (1, 2));
			sr.WriteLine ("playID,{0}", stageID.Trim ());
			sr.WriteLine ("phase,{0}", phaseNumber.ToString ());                        
			sr.WriteLine ("choices,{0}", choices.ToString ());  //171025 can be 2 or 3
			sr.WriteLine ("showPlayPauseButton,{0}", showPlayPauseButton.ToString ());  //171025 true/false
			sr.WriteLine ("pausePlayInputKey,{0}", ProbCalculator.machines [0].pausePlayInputKey);   //180403
			sr.WriteLine ("sessionTime,{0}", (endSessionTime).ToString ("f6").Replace (",", "."));

			//170913 using param mb from gameFlowmanager
			sr.WriteLine ("relaxTime,{0}", mb.GetComponent<GameFlowManager> ().totalRelaxTime.ToString ("f6").Replace (",", "."));

			//170913 Play/Pause times
			sr.WriteLine ("initialPauseTime,{0}", mb.GetComponent<GameFlowManager> ().initialPauseTime.ToString ("f6").Replace (",", "."));
			sr.WriteLine ("numOtherPauses,{0}", mb.GetComponent<GameFlowManager> ().numOtherPauses.ToString ());
			sr.WriteLine ("otherPausesTime,{0}", mb.GetComponent<GameFlowManager> ().otherPausesTotalTime.ToString ("f6").Replace (",", "."));


			//180410 attention strategy: enabled, size, start color, correct color, wrong color
			sr.WriteLine ("attentionPoint,{0}", attentionPoint.ToString ());  //true/false
			sr.WriteLine ("attentionDiameter,{0}", attentionDiameter);        //x.x
			sr.WriteLine ("attentionColorStart,{0}", attentionColorStart);
			sr.WriteLine ("attentionColorCorrect,{0}", attentionColorCorrect);
			sr.WriteLine ("attentionColorWrong,{0}", attentionColorWrong);

			sr.WriteLine ("playerMachine,{0}", playerMachine);
			sr.WriteLine ("gameDate,{0}", LogGame.ToString ().Substring (LogGame.Length - 17, 6));
			sr.WriteLine ("gameTime,{0}", LogGame.ToString ().Substring (LogGame.Length - 10, 6));
			sr.WriteLine ("gameRandom,{0}", LogGame.ToString ().Substring (LogGame.Length - 3, 3));
			sr.WriteLine ("playerAlias,{0}", PlayerInfo.alias);
			sr.WriteLine ("limitPlays,{0}", totalPlays.ToString ());
			sr.WriteLine ("totalCorrect,{0}", totalCorrect.ToString ());
			sr.WriteLine ("successRate,{0}", successRate.ToString ("f1").Replace (",", "."));
			sr.WriteLine ("gameMode,{0}", tmp);

			//CultureInfo works strange... or I'am stupid...
			//tmp = (relaxTime).ToString ("f6", CultureInfo.CreateSpecificCulture("en-US")).Replace (",", ".");


			//------------------------------------------------------------------------------
			//Josi: 161207: send a warning if game was interrupted by the user
			//      161214: gravar em formato dados e para todas as situacoes
			if (!interrupted) {
				tmp = "OK";
			} else {
				tmp = "INTERRUPTED BY USER";
				if (gameSelected == 5) {         //170223 JM: INTERRUPT comes on firstScreen or during the game part?

					//170713 until now, line by line, we know where the INTERRUPT occurs: phase 0 (memorization) or phase 1 (game);
					//       now, just one information line; then, append in the interruption text
					if (log.Count > 0) {         //170223 if there is game log, the, INTERRUPT comes from game;
						tmp = tmp + " (ph1)";    //170713 during the game phase          
					} else {
						tmp = tmp + " (ph0)";    //170223 else, INTERRUPT comes from firstScreen (memorization), not even the game started
					}
					if (log.Count == 0) {
						casoEspecialInterruptedOnFirstScreen = true;   //170223 INTERRUPT from firstScreen (memorization), not even the game started
					}
				}
			}

			//170413 playsToRelax,scoreboard,finalScoreboard,animationType
			//170622 showHistory (true/false)
			//170626 sendMarkersToEEG
			//170629 researchGroup (now groupCode)
			//170712 changed the style one line has all data (criated to facilitate IMEjr analysis), for "variable, content";
			sr.WriteLine ("status,{0}", tmp);
			sr.WriteLine ("playsToRelax,{0}", playsToRelax.ToString ());
			sr.WriteLine ("scoreboard,{0}", scoreboard.ToString ());
			sr.WriteLine ("finalScoreboard,{0}", finalScoreboard);
			sr.WriteLine ("animationType,{0}", animationType);
			sr.WriteLine ("showHistory,{0}", showHistory.ToString ());
			sr.WriteLine ("sendMarkersToEEG,{0}", sendMarkersToEEG);
			sr.WriteLine ("portEEGserial,{0}", portEEGserial);
			sr.WriteLine ("groupCode,{0}", groupCode); 

			//180329 keyCodes used for a user defined keys for defense direction
			//180402 keyCode alternative to playPause button (Amparo)
			//180417 speedAnim (batter/ball/goalkeeper)
			sr.WriteLine ("leftInputKey,{0}", ProbCalculator.machines [0].leftInputKey);
			sr.WriteLine ("centerInputKey,{0}", ProbCalculator.machines [0].centerInputKey);
			sr.WriteLine ("rightInputKey,{0}", ProbCalculator.machines [0].rightInputKey);
			sr.WriteLine ("speedGKAnim,{0}", speedGKAnim);  //x.x

			sr.WriteLine ("portSendData,{0}", portSendData);
			sr.WriteLine ("timeFaixa0,{0}", timeFaixa0);
			sr.WriteLine ("timeFaixa1,{0}", timeFaixa1);
			sr.WriteLine ("timeFaixa2,{0}", timeFaixa2);
			sr.WriteLine ("timeFaixa3,{0}", timeFaixa3);
			sr.WriteLine ("timeFaixa4,{0}", timeFaixa4);

			//180418 keyboard markers, if exist; no, print always to the user know that has/no has values; 
			//following keyboard number order: 1,2,...,8,9,0
			for (int i = 1; i <= 9; i++) {
				sr.WriteLine("keyboardMarker" + i.ToString() + ",{0}", keyboardTimeMarkers[i].ToString ("f6").Replace("," , "." ) );
			}
			sr.WriteLine("keyboardMarker0,{0}", keyboardTimeMarkers[0].ToString ("f6").Replace("," , "." ) );



			//170126 bmMinHits
			if ((gameSelected == 1) || (gameSelected == 4)) {
				sr.WriteLine ("minHits,{0}", bmMinHits.ToString ());
				sr.WriteLine ("minHitsInSequence,{0}", bmMinHitsInSequence.ToString ());  //170919
				sr.WriteLine ("maxPlays,{0}", bmMaxPlays.ToString ()); //170919
			} else {
				//170417 executed tree, format tree="context;prob0;prob1 | context;prob0;prob1 | ...
				if (gameSelected == 2) {
					sr.WriteLine ("minHitsInSequence,{0}", jgMinHitsInSequence.ToString ());  //180324
					sr.WriteLine ("tree, {0}", treeContextsAndProbabilities);
				}
			}



			//170406 entregar a sequencia executada (NES) pelo computador
			//------------------------------------------------------- 
			line = 0;
			StringBuilder sequExecutada = new StringBuilder (log.Count);
			sequExecutada.Length = 0;

			if (gameSelected != 5) {	
				foreach (RandomEvent l in log) {
					sequExecutada.Insert (line, l.resultInt.ToString ());
					line++;                                               
				}
			} else {
				//No JG, se o jogador erra, insiste-se ateh que acerte a jogada
				//180418 save all plays, hit or error, until max plays...
				foreach (RandomEvent l in log) {
//					if (l.correct == true) {
					sequExecutada.Insert (line, l.resultInt.ToString ());
					line++;                                               
//					}
				}
				//180418 player can interrupt the game with 3 or less plays, then, we can know the sequence to memorize
				sr.WriteLine ("sequJMGiven,{0}", mb.GetComponent<GameFlowManager> ().sequJMGiven);
			}
			sr.WriteLine ("sequExecuted,{0}", sequExecutada);  //170717 estava dois pontos...
			//-------------------------------------------------------


			//170217 firstScreen do JM: memorization (part 1)
			if (gameSelected == 5) { 
				sr.WriteLine ("minHitsInSequence,{0}", mdMinHitsInSequence.ToString () );  //180324
				sr.WriteLine ("maxPlays,{0}", mdMaxPlays.ToString () );  //180324
				sr.WriteLine ("try,timeUntilAnyKey,timeUntilShowAgain"); 

				line = 0;
				foreach (RandomEvent l in firstScreenMD) {
					//170713 fixed format: 6 decimal places and dot as decimal separator
				    sr.WriteLine ("{0},{1},{2}", ++line, l.decisionTime.ToString("f6").Replace("," , "." ), l.time.ToString("f6").Replace("," , "." ) ); 
				}
			}


			if (gameSelected == 4) {   //170215 aquecto com tempo: unico jogo com dois tempos: movimento e decisao
				//sr.WriteLine ("{0},{1},waitedResult,ehRandom,optionChosen,correct,movementTime,decisionTime", gameCommonData , ++line);   //170213
				//170311 trocado line por move, mais apropriado (pensado tbem shot...)
				sr.WriteLine ("move,waitedResult,ehRandom,optionChosen,correct,movementTime,pauseTime,timeRunning,decisionTime");
			} else {
				if (!casoEspecialInterruptedOnFirstScreen) {   //170223 if INTERRUPT on firstScreen do not generate header for game lines
					//170712
					sr.WriteLine ("move,waitedResult,ehRandom,optionChosen,correct,movementTime,pauseTime,timeRunning"); 
				}
			}


			// -----
			line = 0;   
			foreach (RandomEvent l in log) {
				//170713 some machines generate decimal with commas (locale?)
				//170217 capitalize FALSE    //(l.correct ? "TRUE" : "false")
				//170919 pauseTime of the play
				tmp = l.resultInt.ToString () + "," + l.ehRandom + "," + l.optionChosenInt.ToString () + "," + (l.correct ? "TRUE" : "false")
				+ "," + l.time.ToString ("f6").Replace (",", ".") + "," + l.pauseTime.ToString ("f6").Replace (",", ".")
				+ "," + l.realTime.ToString ("f6").Replace (",", ".");

				if (gameSelected != 4) {
					//170712 agora comeca o registro das jogadas (moves) no JM; este continua num bloco; parte 2: jogadas
					//sr.WriteLine ("{0},{1},{2}", gameCommonData, ++line, tmp);      //170217 header+commonData; tempo de movimento - jogador entra com a direcao
					sr.WriteLine ("{0},{1}", ++line, tmp);   
				} else {
					//170712 agora comeca o registro das jogadas (moves) no JM; este continua num bloco; parte 2: jogadas
					//sr.WriteLine ("{0},{1},{2},{3}", gameCommonData, ++line, tmp,   // tempo de movimento - jogador entra com a direcao
					//	l.decisionTime.ToString());               //170217 header+commonData; 170113 tempo de decisao - enquanto jogador fica pensando
						sr.WriteLine ("{0},{1},{2}", ++line, tmp, l.decisionTime.ToString("f6").Replace("," , "." ) );            
				}                                                
			}
			sr.Close ();


			//171122 iOS (iPad/iPhone)
			if ((Application.platform == RuntimePlatform.WebGLPlayer) || (Application.platform == RuntimePlatform.Android) ||
				(Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad")) ) {
				//SyncFiles();                                        //170622 fast refresh
				Application.ExternalEval("_JS_FileSystem_Sync();");

				//170620 ter o nome do arquivo de resultados e abrir todo o arquivo para coletar o conteúdo, para enviar por formWeb
				//170622 reler o arquivo só eh necessario se eh WEBGL
				int i = LogGame.ToString().IndexOf("/Plays");
				resultsFileName = LogGame.ToString ().Substring (i, LogGame.Length - i);

				resultsFileContent = System.IO.File.ReadAllText(LogGame.ToString());
			}

			//170612 se webGL estes comandos dao erro win32 IO already exists...
			//170622 sem usar diretiva de compilacao
			//171122 iOS (iPad/iPhone)
			if ((Application.platform != RuntimePlatform.WebGLPlayer) && (Application.platform != RuntimePlatform.Android) &&
				(Application.platform != RuntimePlatform.IPhonePlayer) && (! SystemInfo.deviceModel.Contains("iPad")))  {
				//170123 copiar arquivo para o backup e renomear com a extensao .csv
				tmp = LogGame.ToString();
				tmp = tmp.Substring(tmp.IndexOf("Plays_")-1) + ".csv";
				//  Using System.IO
				File.Copy(LogGame.ToString(), backupResults + tmp);       // copiar sem ext para backupResults com ext
				File.Move(LogGame.ToString(), LogGame.ToString()+".csv"); // mover sem ext para com ext, em assets
				File.Delete(Application.dataPath + tmp + ".meta");        // deletar os .meta criados pelo unity3d
			}



			//170818 trying to save result files for mobile devices in Android
			//171122 iOS (iPad/iPhone)
			// ==============================================================================
			if ((Application.platform == RuntimePlatform.WebGLPlayer) || (Application.platform == RuntimePlatform.Android) ||
				(Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad")))
			{
				//estudar isto... nao permite chamar um IEnumerator declarado aqui... não é MonoBehavior...
				mb.StartCoroutine( uploadFile(resultsFileName, resultsFileContent) );
			}
			// ==============================================================================

		}   //if (!File.Exists
	}       //public void RegisterPlay


	// --------------------------------------------------------------------------------------
	//170612
	IEnumerator uploadFile(string fileName, string contentFile)
	{
		//converting text to bytes to be ready for upload (really necessary?)
		byte[] fileData = Encoding.UTF8.GetBytes (contentFile);

		//180226 hash; inside routine convert to byte
		string hash = GetHash(contentFile)	;
		hash = GetHash(hash);

		//criar o form que receberá o conteudo do arquivo
		WWWForm formData = new WWWForm ();

		formData.AddField("action", "level upload");
		formData.AddField("ident", hash);          //180226
		formData.AddField("file","file");
		formData.AddBinaryData ( "file", fileData, fileName, "text/plain");


		//url do servidor com o script php que receberá o arquivo
		string loginURL = webProtocol + gameServerLocation + "/unityUpload_test.php";

		//iniciar o envio dor form (https://docs.unity3d.com/ScriptReference/WWW-ctor.html)
		WWW w = new WWW (loginURL, formData);
		yield return w;

		//se w.error envia erro, logar nome do arquivo para tentar descobrir o problema
		if (w.error != null) {
			Debug.Log ("file " + fileName + " w.error = " + w.error);
		} 
	}


	// --------------------------------------------------------------------------------------
	//170830 para hashear novamente o identifierID (que vem como hash5 mas foi facilmente decriptado)
	//       em: https://stackoverflow.com/questions/43042428/sha256-is-returning-invalid-characters-in-the-hash
	//           
	static string GetHash(string input)
	{	//SHA512 sha512Hash = SHA512.Create();   NullReferenceException: Object reference not set to an instance of an object
		//Em https://stackoverflow.com/questions/30055358/md5-gethash-work-only-in-unity-editor :
		//MD5.Create() doesn't return an object on Unity Android when the Stripping Level is set to Micro mscorlib, but 'new MD5CryptoServiceProvider()' does.
		var sha512Hash = new SHA512CryptoServiceProvider();

		// Convert the input string to a byte array and compute the hash.//
		byte[] data = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

		// Create a new Stringbuilder to collect the bytes and create a string.
		StringBuilder sBuilder = new StringBuilder();

		// Loop through each byte of the hashed data and format each one as a hexadecimal string.
		for (int i = 0; i < data.Length; i++) {
			sBuilder.Append(data[i].ToString("x2"));
		}

		// Return the hexadecimal string.
		return sBuilder.ToString();
	}
		
}


