/**
 * Module written by scaroni <renato.scaroni@gmail.com>
 * Rewritten by Josi Perez <josiperez.neuromat@gmail.com>
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine.EventSystems;
using System.Security.Cryptography;     //170830 sha512Hash available


// Only valids for Windows (to call inpout32.dll)
#if UNITY_STANDALONE_WIN  || UNITY_EDITOR_WIN
using System.Runtime.InteropServices;
#endif

using System;
using System.IO.Ports;
using System.Net.Mime;


public class RandomEvent
{
	// Defense waited: 0, 1 or 2 (if choices=3), or 1,2 (if choices=2)
	public int resultInt;
	// Random play? Y, or n; in any other module it is "n" (AQ/AR is random...)
	public char ehRandom;
	// Defense choosed by player; numeric format
	public int optionChosenInt;
	// Defense waited in string format: dir, esq, cen
	public string result;
	// Defense choosed in string format: dir, esq, cen
	public string optionChosen;
	public bool correct;
	public string state;
	public float time;
	public float decisionTime;
	public float pauseTime;
	// Para analisar com os marcadores
	public float realTime;
}



//------------------------------------------------------------------------------------
public class UIManager : MonoBehaviour
{
	private string backupResults;                                 //170622 to save a copy of results
	private StringBuilder gamePlayed = new StringBuilder(4, 4);   //161212: _JG_ or AQ, AR, JM
	private string resultsFileName;                               //170622 to reread the saved file, if WEBGL, and save the name
	private string resultsFileContent;                            //170622 to reread the saved file, if WEBGL, and save the content
	public int failed;
	private bool uploading;
	#if UNITY_ANDROID || UNITY_IOS
		private StringBuilder LogGame = new StringBuilder(280, 320);
	#else
		private StringBuilder LogGame = new StringBuilder(180, 220);
	#endif
	private string tmp;                                           //170124 
	private int line = 0;                                         //170213 para inserir numero da linha no arquivo (uma sequencia)
	private bool casoEspecialInterruptedOnFirstScreen;            //170223 detectar dif entre interromper na firstScreen ou no jogo, no JM
	public WWW www = null;
	public GameObject cronosIn;
	public GameObject cronosOut;
	public GameObject tmpDecisao;
	public GameObject cruzLaranja;
	public GameObject cruzVerde;
	public GameObject cruzPreta;
		
	public Text eventsLog;
	public GKAnimController[] gkAnim;
	public int eventWindow = 10;
	public float successRate = 0;
	public ScoreMonitor scoreMonitor;
	public GameObject btnsAndQuestion;

	//visual and audio animations for player hit/error
	public Animator anim321;         //170108 changed to Animator by Thom
	public Animator pegoal;          //Defendeu
    //recognize all idioms and keep just one after player to select language interface
	public Animator pegoalEnUs;      //171031 Defended!
	public Animator pegoalPtBr;      //171031 defendeu!
	public Animator pegoalEsEs;      //171222 defendió!

	public Animator perdeu;          //170102 anim Thom; "perdeeu..." para fazer par com "defendeu!!"
	//recognize all idioms and keep just one after player to select language interface
	public Animator perdeuEnUs;      //171031 Lost...
	public Animator perdeuPtBr;      //171031 Perdeu... sendo "..." um único caracter
	public Animator perdeuEsEs;      //171031 Perdió...

	public AudioSource cheer;        //som para o defendeu
	public AudioSource cheershort;   //170315 som para o defendeu short
	public AudioSource lament;       //som para o perdeu
	public AudioSource lamentShort;  //som para o perdeu short

	public AudioSource sound321;     //170825 to synchronyze with 321animation
	public AudioSource sound321ptbr; //171031 voice talking pt-br
	public AudioSource sound321enus; //171031 voice talking en-us
	public AudioSource sound321eses; //171222 voice talking es-es
	private string locale;           //171031 save locale selected

	public Sprite neutralUISprite;
	public Sprite [] rightUISprite;
	public Sprite [] wrongUISprite;

	public List<GameObject> optBtns;

	public float decisionTimeA;      //170113 tempo que o user fica pensando o que fazer
	public float decisionTimeB;      //170113 tempo que o user fica pensando o que fazer
	public float movementTimeA;      //170309 tempo de movimento: desde que aparecem as setas de defesa até que player seleciona uma delas

	public int eventCount = 0;       //170106 para ser acessado no gameFlow.onAnimationEnded
	public bool BtwnLvls = false;

	private ProbCalculator probs;
	private GameFlowManager gameFlow;

	public int success = 0;
	public Text placar;              //muda do tipo string para StringBuilder (reserva espaco de antemao, sem garbage collection
	public Text placarFirstScreen;   //170103 Base Memoria //170125 basta o placar.text

	public GameObject setaEsq;      //mainScene/gameScene/GameUICanvas/bmIndicaChute/chutaEsq
	public GameObject setaDir;      //mainScene/gameScene/GameUICanvas/bmIndicaChute/chutaDir
	public GameObject setaCen;      //mainScene/gameScene/GameUICanvas/bmIndicaChute/chutaCen

	public int jogadasFirstScreen = 0;       //170104: MD numero de tentativas na firstScreen
	public int acertosFirstScreen = 0;       //170102: MD: necessario acertar 3x a sequ para avancar para MD (JG fase 3)
	public int teclaMDinput;        //170125 para avancar ou nao no idx da sequencia; se o goleiro errou não avanca ate acertar

	public GameObject mdFrameIndicaChute1;   //170102
	public GameObject mdFrameIndicaChute2;   //170102
	public GameObject mdFrameIndicaChute3;   //170102
	public GameObject mdFrameIndicaChute4;   //170102

	public List<GameObject>  mdSequChute1;   //170102
	public List<GameObject>  mdSequChute2;   //170102
	public List<GameObject>  mdSequChute3;   //170102
	public List<GameObject>  mdSequChute4;   //170102

	public GameObject mdMsg;                 //170124 Jogo da memoria: aperte uma tecla quando pronto
	public GameObject mostrarSequ;           //170124 Jogo da memoria: botao mostrar sequencia (estará escondida)
	public GameObject jogar;                 //170124 Jogo da memoria: botao jogar
	public GameObject menuJogos;             //170311 JM: botao Menu Jogos, para desistir do EXIT
	public GameObject btnExit;               //170313 JM: botao EXIT de todos os jogos; objeto para mostrar/nao mostrar o Exit

	public bool aguardandoTeclaBMcomTempo = false;  //161229
	public bool aguardandoTeclaMemoria = false;     //170124
	public bool aguardandoTeclaPosRelax = false;    //170222 descanso dos pacientes LPB

	public bool animCountDown = false;        //170111 para determinar continuacao ao fim das animacoes anim321, pegoal e perdeu
	public bool animResult = false;           //170111 para determinar continuacao ao fim das animacoes anim321, pegoal e perdeu

	private List<RandomEvent> _events = new List<RandomEvent> ();
	public  List<RandomEvent> _eventsFirstScreen = new List<RandomEvent> ();  //170108 salvar experimentos da fase MD testes de memoria

	public GameObject buttonPlay;             //170906 botões Play/Pause
	public GameObject buttonPause;            //170906
	public bool pausePressed;                 //170906
	public GameObject mdButtonPlay;           //170912 botões Play/Pause no Jogo da Memória
	public GameObject mdButtonPause;          //170912

	private LocalizationManager translate;    //171010 trazer script das rotinas de translation
	public SerialPort serialp = null;         //180104 define a serial port to send markers to EEG, if necessary
	public Byte[] data = { (Byte)0 };         //180104 to send data to the serial port; used also on gameFlow
	public int diagSerial;                    //180108 serial diagnostic
	public int timeForYellow=0;
	public int timeForZero=0;
	public float counttime=1.0f;
	public bool testa0 = false;
	public bool testa1 = true;
	public bool testa2 = false;
	public string pIn;
	public bool sentFile = false;
	public  int  timeBetweenMarkers = 100000000;  //tempo entre envios à paralela;
	public  int timeBetweenMarkersSerial = 100000;
	public bool userAbandonModule = false;
	public GameObject attentionPoint;
	public GameObject frame0EEG;  //middle of the screen to fix player attention (EEG experiments)
	public GameObject exibeFaixa;
	public GameObject frame1EEG;
	public GameObject frame2EEG;
	public GameObject frame3EEG;
	public GameObject frame4EEG;
	public float[] keyboardTimeMarkers;
	public GameObject showMsg;
	public bool failedRegisterUserEntry = false;
	public delegate void AnimationEnded();
	public static event AnimationEnded OnAnimationEnded;
	public delegate void AnimationStarted();
	public static event AnimationStarted OnAnimationStarted;


	public List<RandomEvent> events
	{
		get	{
			return _events;
		}
	}

	static private UIManager _instance;
	static public UIManager instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = GameObject.Find("UIManager").GetComponent<UIManager>();
			}

			return _instance;
		}
	}


	//Josi: ninguem chama esta function
	public float GetSccessRate()
	{
		if(_events.Count > eventWindow)
			return successRate;
		return 0;
	}



	//--------------------------------------------------------------------------------------------------------
	//Josi: arquiva os dados do experimento; verifica se acerto ou erro;
	//      esta função é tbem chamada no onClick do mainScene/.../Pergunta/<em cada uma das direcoes de chute>
	public void BtnActionGetEvent(string input)
	{
		// Inhibit typing by mouse. Only accept if main keys DownArrow, LeftArrow e RightArrow
		if (!(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) ||
			Input.GetKey(KeyCode.RightArrow))) {
			// Debug.Log("Mouse key is held down");
			return;
		}

		RandomEvent eLog = new RandomEvent ();
		eLog.time = Time.realtimeSinceStartup - movementTimeA - gameFlow.otherPausesTime ;
		float tmpTime = eLog.time + decisionTimeA;
	    tmpDecisao.GetComponent<Text>().text = eLog.time.ToString();
		cronosIn.GetComponent<Text>().text = tmpTime.ToString();

////		exibeFaixa.GetComponent<Text>().text = "Faixa 3";


		//170915 para impedir o click se está em modo pausa
		if (! pausePressed) {
			btnsAndQuestion.SetActive (false);  //170112 importante manter aqui e nao noUpdate, quando perderah a espera de teclas

			//170920 o PlayPause só vai valer entre o mostrar a seta e o user selecionar;
			//       interromper fora desse gap é só para arranjar problema com as sobras de animacao na tela
			buttonPlay.SetActive (false);
			buttonPause.SetActive (false);


			//170320 trocado para ca para tentar isolar a diferenca entre o tempo total de jogo e o tempo de movimento menos animacoes
//			RandomEvent eLog = new RandomEvent ();

			//170309 acertar tempo no JG descontando o tempo das animacoes e o tempo de relax se houver (senao valem zero)
			//eLog.time = Time.realtimeSinceStartup - movementTimeA -  (gameFlow.endRelaxTime - gameFlow.startRelaxTime);
			//170413
			//estava dando erro de tempo negativo no move logo após a tela de relax; nao entendi porque - mudei a estrategia
			//170919 descontar os possiveis tempos de pausa do Play/Pause
			//eLog.time = Time.realtimeSinceStartup - movementTimeA;
			eLog.time = Time.realtimeSinceStartup - movementTimeA - gameFlow.otherPausesTime ;
			eLog.pauseTime = gameFlow.otherPausesTime;
			eLog.realTime = Time.realtimeSinceStartup - gameFlow.startSessionTime;    //180418 to accomplish marker time by keyboard


			//170919
			gameFlow.otherPausesTotalTime = gameFlow.otherPausesTotalTime  +  gameFlow.otherPausesTime ;
			gameFlow.otherPausesTime = 0;

			gameFlow.endRelaxTime = 0.0f;
			gameFlow.startRelaxTime = 0.0f;
			//----


			//170112 estava aparecendo o frame vazio no BM/BMt apos defender para uma direcao
			if ((PlayerPrefs.GetInt ("gameSelected") == 1) || (PlayerPrefs.GetInt ("gameSelected") == 4)) {
				gameFlow.frameChute.SetActive (false);
			}


			//170130 para comparar o esperado com o input dado e saber se devemos andar com o ponteiro
			if (PlayerPrefs.GetInt ("gameSelected") == 5) {
				if (input == "esquerda") {
					teclaMDinput = 0;
				} else {
					if (input == "centro") {
						teclaMDinput = 1;
					} else {
						teclaMDinput = 2;  //"direita"
					}
				}
			}


			//170216
			int e = probs.GetEvent (teclaMDinput);  //170130 teclaMDinput param para nao precisar instanciar uiManager no probCalc

			string dirEsq = System.String.Empty;    //170110 Use System.String.Empty instead of "" when dealing with lots of strings;

			if (OnAnimationStarted != null)
				OnAnimationStarted ();
			btnsAndQuestion.SetActive (false);


			//170320 trocar estes tempos para cima para tentar isolar a diferenca entre o tempo total de jogo e o tempo de movimento menos animacoes
			if (PlayerPrefs.GetInt ("gameSelected") == 4) {      //BM com tempo
				//eLog.decisionTime = decisionTimeA + (Time.realtimeSinceStartup - decisionTimeB);  //170113: tempo do "aperte tecla" ate que user aperta
				eLog.decisionTime = decisionTimeA;   //170320
			} else {
				eLog.decisionTime = eLog.time;     //170113: tempo que o jogador está pensando; no BM equivale ao TMovimento
			}
			
			eLog.resultInt = e;
			if (e == 0) { //esquerda
				dirEsq = "esquerda";
			} else if (e == 1) {
				dirEsq = "centro";
			} else {
				dirEsq = "direita";
			}

			eLog.result = dirEsq;
			eLog.optionChosen = input;

			if (input.Equals (dirEsq)) {
				eLog.correct = true;
				success++;
			} else {
				eLog.correct = false;
			}

			
			//180410 if parametrized, show "attention point" in middle screen
			if (probs.attentionPointActive()) {
				
// troca a bola pela cruz - 20190702	attentionPointColor (eLog.correct == true?1:2);  //on Inspector: 0: start, 1:correct, 2:wrong
			
// Paulo Roberto pediu para tirar - 20190715			cruzLaranja.gameObject.SetActive(false); //troca a bola pela cruz - 20190702
// Paulo Roberto pediu para tirar - 20190715			cruzPreta.gameObject.SetActive(true);    //troca a bola pela cruz - 20190702
// Paulo Roberto pediu para tirar - 20190715			cruzVerde.gameObject.SetActive(false);   //troca a bola pela cruz - 20190702
			}

			//170921 zerar ou acumular minHitsInSequence
	        //180320 now, minHitsInSequence worth for all game modules
			if (eLog.correct) {
				++gameFlow.minHitsInSequence;
			} else {
				gameFlow.minHitsInSequence = 0;
			}

			//170215 gravar se a jogada, no JG, é randomizada ou não; nos demais é sempre n
			if (PlayerPrefs.GetInt ("gameSelected") != 2) {
				eLog.ehRandom = 'n';
			} else {
				if (probs.ehRandomKick) {
					eLog.ehRandom = 'Y';
				} else {
					eLog.ehRandom = 'n';
				}
			}


			int targetAnim = probs.GetCurrMachineIndex ();
			if ((targetAnim >= gkAnim.Length) || (gameFlow.jogarMDfase3 && ((PlayerPrefs.GetInt ("gameSelected") == 3) || (PlayerPrefs.GetInt ("gameSelected") == 5)))) {  //170125 MD ou Memoria usam a fase3 do JG
				targetAnim = gkAnim.Length - 1;
			}


			if (input == "esquerda") {
				eLog.optionChosenInt = 0;
				if (!gameFlow.firstScreen) {     //170102 nao eh MD primeira tela, onde nao existe o gk, apenas uma tela limpa
					if (eLog.correct) {
						gkAnim [targetAnim].Play ("esq", dirEsq.Substring (0, 3));
					} else {
						gkAnim [targetAnim].Play ("esq_goal", dirEsq.Substring (0, 3) + "_goal");
					}
				}
			} else if (input == "direita") {
				eLog.optionChosenInt = 2;
				if (!gameFlow.firstScreen) {     //170102 nao eh MD primeira tela, onde nao existe o gk, apenas uma tela limpa
					if (eLog.correct) {
						gkAnim [targetAnim].Play ("dir", dirEsq.Substring (0, 3));
					} else {
						gkAnim [targetAnim].Play ("dir_goal", dirEsq.Substring (0, 3) + "_goal");
					}
				}
			} else {
				eLog.optionChosenInt = 1;
				if (!gameFlow.firstScreen) {  //170102 nao eh MD primeira tela
					if (eLog.correct) {
						gkAnim [targetAnim].Play ("cen", dirEsq.Substring (0, 3));
					} else {
						gkAnim [targetAnim].Play ("cen_goal", dirEsq.Substring (0, 3) + "_goal");
					}
				}
			}


			_events.Add (eLog);

			if (gameFlow.firstScreen && (PlayerPrefs.GetInt ("gameSelected") == 3)) {  //Josi: apenas no memoDeclarat
	           _eventsFirstScreen.Add (eLog);  //170109
			}

			eventCount++;

			// TODO (GG-1/GG-2): DEBUG: remove after discover error
			if (Application.platform == RuntimePlatform.WebGLPlayer && eventCount % 10 == 0)
			{
				string content = "nickname: " + PlayerInfo.alias + "; plays: " + eventCount + "; date: "
				                 + DateTime.Now.ToString("yyMMdd_HHmmss") + "\n";
				StartCoroutine(ServerOperations.instance.logUserActivity("log_user_activity.php", content));
			}

			int successCountInWindow = 0;
			for (int i = 0; i < eventWindow; i++) {
				if (eventCount - 1 - i < 0) {
					break;
				}
				if (_events [eventCount - 1 - i].correct) {
					successCountInWindow++;
				}
			}
			
			successRate = ((float)successCountInWindow) / ((float)eventWindow);

			//#####################################################################################
			//170103 se primeira tela do MD, mostrar se user acertou ou errou
			if ((PlayerPrefs.GetInt ("gameSelected") == 3) && gameFlow.firstScreen) {  //MD
				//if (dirEsq == input) {  //direcao esperada = direcao usada pelo goleiro (defendeu)
				if (eventCount == 1) {
					mdFrameIndicaChute1.SetActive (false);
				} else {
					if (eventCount == 2) {
						mdFrameIndicaChute2.SetActive (false);
					} else {
						if (eventCount == 3) {
							mdFrameIndicaChute3.SetActive (false);
						} else {
							if (eventCount == 4) {
								mdFrameIndicaChute4.SetActive (false);
								jogadasFirstScreen++;                 //num de tentativas de cada ciclo de "decorar 4 simbolos"
								if (success == 4) {                   //ao fim das 4 defesas, soma-se um ciclo de certos; 3 ciclos encerram a tela inicial do MD
									acertosFirstScreen++;             //170103 se acertou
									if (acertosFirstScreen == 3) {    //jogou 3 vezes corretamente, ir para o MD do campo profissional
										gameFlow.firstScreen = false;
										gameFlow.jaPasseiPorFirstScreen = true;
									}
								} else {
									acertosFirstScreen = 0;          //vale se jogou 3x seguidas corretamente
								}
								gameFlow.NewGame (PlayerPrefs.GetInt ("gameSelected"));
							}
						}
					}
				}
				btnsAndQuestion.SetActive (true);
				movementTimeA = Time.realtimeSinceStartup; //170309 para nao precisar descontar tempo das animacoes (impreciso)

				//placarFirstScreen.text =
				//170216 novo param no PlayLimit caso JG: se configurado para ter phase0, os limites sao especificos desta fase, diferente das demais fases1,2,3
				placarFirstScreen.text = success.ToString ().PadLeft (3) + " / " + probs.GetCurrentPlayLimit (PlayerPrefs.GetInt ("gameSelected")).ToString ()
					         + " (" + acertosFirstScreen.ToString () + ")";
			}
			//#####################################################################################

			//#####################################################################################
			//@le 190514 : faz com que ao ser pressionado qq tecla, ele ja acione o script de gravacao
			// e nao mais espera pelo final da fase

			//Debug.Log("-------------------> INPUT PRESSIONADO = " +eLog.realTime);
			// Debug.Log("@BtnActionGetEvent:movementTimeA = "+ movementTimeA);
		}
	}

	/**
	 * Ao trocar de nível, envia os dados do experimento para arquivo local
	 * A corrotina se encarrega de enviar o arquivo para o servidor
	 */
	public void SendEventsToServer(int gameSelected)
	{
		if ( (_events != null && eventCount > 0) || (_eventsFirstScreen.Count > 0) )
		{
			if (PlayerInfo.agree)
			{
				float endSessionTime = Time.realtimeSinceStartup - gameFlow.startSessionTime;
				int jogadas = probs.GetCurrentPlayLimit (gameSelected);
				int acertos = success;

				if (gameSelected == 5) {
					//Fixo no restante do script.
					//Faltaria pensar um grid que permitisse aumentar os quadros iniciais.
					jogadas = 12;
				}

				//170217 melhor colocar o num jogadas original; no numLinhas do arquivo de resultados se verah que foi necessario gerar mais jogadas para atender minHits
				if ((gameSelected == 1) || (gameSelected == 4)) {
					jogadas = probs.saveOriginalBMnumPlays;
				}

				//170216 na phase0 do JG, o gameMode (ler da sequ ou da arvore) é readSequ
				bool gameMode = probs.getCurrentReadSequ (gameSelected);

				//170310 acrescentar a fase do jogo: no AQ, AR, JM há apenas uma fase; no JG pode haver de 0 a 8
				int phaseNumber = 0;
				if (gameSelected == 2) {
					phaseNumber = probs.GetCurrMachineIndex () + 1; //comeca de zero
				}

				string animationType;
				if (gameSelected == 2) {
					animationType = ProbCalculator.machines [probs.currentStateMachineIndex].animationTypeJG;
				} else {
					animationType = ProbCalculator.machines [probs.currentStateMachineIndex].animationTypeOthers;
				}

				string treeContextsAndProbabilities = probs.stringTree ();

				RegisterPlay (GameFlowManager.instance, locale, endSessionTime, probs.CurrentMachineID (),
					gameMode, phaseNumber, jogadas, acertos, successRate,
					probs.getMinHits (), ProbCalculator.machines [0].bmMaxPlays, ProbCalculator.machines [0].bmMinHitsInSequence,
					_events, userAbandonModule,
					_eventsFirstScreen, animationType,
					ProbCalculator.machines [probs.currentStateMachineIndex].playsToRelax,
					ProbCalculator.machines [probs.currentStateMachineIndex].showHistory,
					probs.getSendMarkersToEEG (),
					probs.getPortEEGserial(),
					ProbCalculator.machines [0].groupCode,
					ProbCalculator.machines [probs.currentStateMachineIndex].scoreboard,
					ProbCalculator.machines [probs.currentStateMachineIndex].finalScoreboard,
					treeContextsAndProbabilities,
					ProbCalculator.machines [0].choices,
					ProbCalculator.machines [0].showPlayPauseButton,
					ProbCalculator.machines [probs.currentStateMachineIndex].minHitsInSequence,
					ProbCalculator.machines [0].mdMinHitsInSequence,
					ProbCalculator.machines [0].mdMaxPlays,
					ProbCalculator.machines [0].institution,
					ProbCalculator.machines [0].attentionPoint,
					ProbCalculator.machines [0].attentionDiameter,
					ProbCalculator.machines [0].attentionColorStart,
					ProbCalculator.machines [0].attentionColorCorrect,
					ProbCalculator.machines [0].attentionColorWrong,
					ProbCalculator.machines [probs.currentStateMachineIndex].speedGKAnim,
					probs.getPortSendData(),
					ProbCalculator.machines [0].timeFaixa0,
					ProbCalculator.machines [0].timeFaixa1,
					ProbCalculator.machines [0].timeFaixa2,
					ProbCalculator.machines [0].timeFaixa3,
					ProbCalculator.machines [0].timeFaixa4,
					keyboardTimeMarkers
				);

				//170306 zerar a lista para não entrar aqui pelo GoToIntro e gerar dois arquivos de resultados para o mesmo JM (sendo um vazio)
				//170311 e voltar aos contadores
				if (gameSelected == 5) {
					_eventsFirstScreen.Clear ();
				}
			} //170830 só vai para gravar o arquivo se aprovada a participação na pesquisa...
		}
	}


	//--------------------------------------------------------------------------------------------------------
	//161214 change lawn/trave/ball for the new phase
	public void CorrectPhaseArt(int gameSelected)
	{
		int targetAnim = probs.GetCurrMachineIndex();
		//there is only 3 different football field; if more phases, for now use the last
		if ((targetAnim >= gkAnim.Length) || (gameFlow.jogarMDfase3 && ((gameSelected == 3) || (gameSelected == 5)))) {
			targetAnim = gkAnim.Length - 1;
		}

		//enable the correct animation and disable others
		for (int i = 0; i < gkAnim.Length; i++) {
			if (i != targetAnim) {
				gkAnim [i].gameObject.SetActive (false);
			} else {
				gkAnim [i].gameObject.SetActive (true);
			}
		}

		if (gameSelected == 4) {                     //Josi 161229 iniciar com esperoTecla se BMcomTempo, no AR
			aguardandoTeclaBMcomTempo = true;        //     nao deveria estar aqui, a melhorar @@

			//170914 se "aperte tecla", desativar Play/Pause
			buttonPause.SetActive(false);
			buttonPlay.SetActive(false);
		}
	}


	//--------------------------------------------------------------------------------------------------------
	//count total gkAnim phases; now are three: land field, semiprofessional, professional;
	//a designer could paint a champion field, with announces, public, etc
	public int GetTotalLevelArts() 	{
		return gkAnim.Length;
	}


	//--------------------------------------------------------------------------------------------------------
	//180411 set the color of attentionPoint: green if player turn; red if program turn
	public void attentionPointColor(int color)
	{
		switch (color) 
		{
			case 0:  // Laranja
				break; 
			case 1:  // Verde
				break; 
			case 2:  //Preta
				break; 
		}
	}

	//--------------------------------------------------------------------------------------------------------
	//Josi: inicializa listas, variáveis, histórico de jogadas (setas verdes e pretas), placar
	public void ResetEventList(int gameSelected)
	{
		_events = new List<RandomEvent> ();    //inicializar vetor com dados das fases
		                                       //nao inicia a _eventsFirstScreen do MD porque pode estar acumulando uma nova jogada
		eventCount = 0;
		success = 0;
		successRate = 0;
		if ((gameSelected == 2) || (((gameSelected == 3) || (gameSelected == 5)) && (!gameFlow.firstScreen)))  //161214: se JG ou MD ou JMemoria, resetar o painel do resultado das jogadas (setas em verde ou em preto)
		{
			scoreMonitor.Reset ();
		};

		//Josi: iniciar placar cf o jogo
		updateScore (gameSelected);
	}



	//--------------------------------------------------------------------------------------------------------
	//Josi: activate animations: perdeu/defendeu (visual) and lamento/alegria (sonoro)
	public void PostAnimThings ()
	{
		//Josi: nao executar se ultim jogo
		if (events.Count > 0) {
			btnsAndQuestion.SetActive(false);

			//170112 se eh ultima animacao defendeu/perdeu antes da tela de betweenLevels, nao fazer
			//170205 IMEjr FAZER animacao msmo na ultima antes do mudar de fase
			if (eventCount <= probs.GetCurrentPlayLimit (PlayerPrefs.GetInt ("gameSelected"))) {  //170216 limitPlays no JG (diferente se fase0 ou 1,2 ou 3)
				if (probs.getCurrentAnimationType() == "long") { //long anim, sound and visual

					if (events [events.Count - 1].correct) {     //if correct, animations cheer+defendeu
						cheer.gameObject.SetActive (true);
						pegoal.speed = 1.0f;                     //171031 needed to keep the normal speed
						pegoal.enabled = true;
						pegoal.SetTrigger ("pegoal");

						//170818 se Android, vibrar ao acertar
						//170828 ao compilar, reclamou do Handheld mesmo com using UnityEngine
						#if UNITY_ANDROID || UNITY_IOS
						//if (Application.platform == RuntimePlatform.Android) {
						Handheld.Vibrate();
						//}
						#endif

					} else {                                     //if wrong defense, animations lament+perdeu
						lament.gameObject.SetActive (true);
						perdeu.speed = 1.0f;                     //171031 needed to keep the normal speed
						perdeu.enabled = true;
						perdeu.SetTrigger ("goal");              //170204 anim Thom
					}
					//170111 como as animacoes tem o mesmo tempo pode vir para ca
					animResult = true;

				} else {                                         //170215 mas falta ter as animacoes
					if (probs.getCurrentAnimationType() == "short") {     //short anim, sound and visual
						if (events [events.Count - 1].correct) { //if correct, animations cheer+defendeu
//20190704							cheerShort.gameObject.SetActive (true);

							//171031 removed short animations: it is enough to change the speed
							pegoal.speed = 2.0f;
							pegoal.enabled = true;
							pegoal.SetTrigger ("pegoal");

							//170818 se Android, vibrar ao acertar
							//170828 ao compilar, reclamou do Handheld mesmo com using UnityEngine
							#if UNITY_ANDROID || UNITY_IOS
							//if (Application.platform == RuntimePlatform.Android) {
							Handheld.Vibrate();
							//}
							#endif

						} else {                                     //if wrong, animations lament+perdeu
//20190704							lamentShort.gameObject.SetActive (true);

							//171031 removed short animations: it is enough to change the speed
							perdeu.speed = 2.0f;                     //171031 needed to keep the normal speed
							perdeu.enabled = true;
							perdeu.SetTrigger ("goal");              //170204 anim Thom

						}
						//170111 como as animacoes tem o mesmo tempo pode vir para ca
						animResult = true;
					} else {
						if (probs.getCurrentAnimationType() == "none") {  //sem anim som e visual
							//170111 como as animacoes tem o mesmo tempo pode vir para ca
							animResult = true;
						}
					}
				}


				//btnsAndQuestion.SetActive(true);  //180416 try to increase speed for activate the btns before
				                                    //will this generate animations freezed?... an empty history block

				//170323 passar para ca para nao repetir em cada tipo de animacao
				//170418 o animationTime foi criado para esperar acabar uma animacao, que ja comecou, logo devolve tempos menores;
				//       aqui se acerta para garantir que nao havera sobreposicao com o proximo evento;
				//       no jogo AR, melhor esperar um pouco mais antes de colocar o "aperte uma tecla"
				float extraTime = 0.2f;
				if (PlayerPrefs.GetInt ("gameSelected") == 4) {
					extraTime = 0.5f;
				}
				/**
				 * TODO: 0.29f is a magic number to fix the problem of syncronization between the red arrows and the
				 * idle form of the player before new kick. The issue is partially fixed by diminishing the animation
				 * time of the animations related to goalkeeper not caughting the ball.
				 */
				StartCoroutine (WaitThenDoThings ( probs.animationTime() + extraTime - 0.29f ));  //170322 centralizado em uma rotina os tempos de animacao


				//Score here, else shows up before play
				updateScore ( PlayerPrefs.GetInt ("gameSelected") );
			}
		} //Josi: fim do if events.count
	}



	//--------------------------------------------------------------------------------------------------------
	//170126 inicializar e atualizar placar
	public void updateScore (int gameSelected)
	{
		placar.text = System.String.Empty;    //170216 Use System.String.Empty instead of "" when dealing with lots of strings;
		if (probs.getCurrentScoreboard ()) {
			if (eventCount > 0) {
				//180323 not reset the counter if error in sequence (Amparo request)
				placar.text = success.ToString ().PadLeft (3) + " / " + probs.GetCurrentPlayLimit (gameSelected).ToString ();  //170216

				//170928 AQ/AR na opcao minHitsInSequ
				//int howManyCorrects = success;     //trocar no placar.text, success por howManyCorrects
				//if ((gameSelected == 1 || gameSelected == 4) && (probs.getMinHitsInSequence () > 0)) {
				//	howManyCorrects = gameFlow.minHitsInSequence;
				//}
				//placar.text = howManyCorrects.ToString ().PadLeft (3) + " / " + probs.GetCurrentPlayLimit (gameSelected, phaseZeroJG).ToString ();  //170216
			} else {
				placar.text = "  0 / " + probs.GetCurrentPlayLimit (gameSelected).ToString ().PadLeft (3).Trim ();  //170216
				if (gameSelected == 3) {                   //170124 Base memória (input de teclado) tem placar; Jogo da memória nao tem placar
					//placarFirstScreen.text = placar.text + " (" + acertosFirstScreen.ToString () + ")";  //170102 comećam iguais gracas ao parametro gameSelected;
					placarFirstScreen.text = placar.text + " (" + acertosFirstScreen.ToString () + ")";  //170125 nao eh necessario um placar extra para firstScreen
					placar.text = System.String.Empty;     //170216 Use System.String.Empty instead of "" when dealing with lots of strings;;
					//neste se acrescenta o num de acertos da sequencia a decorar
				} else {
					if ((gameSelected == 5) && gameFlow.firstScreen) {       //170125 Jogo da memoria
						//placarFirstScreen.text = "";
						placar.text = System.String.Empty; //170216 Use System.String.Empty instead of "" when dealing with lots of strings;
					}
				}
			}
		}
	}

	//--------------------------------------------------------------------------------------------------------
	//Josi: function para acertar na tela o proximo chute a indicar
	public void showNextKick(string direcaoAindicar)
	{
		//acertar a seta da próxima jogada
		setaEsq.SetActive((direcaoAindicar == "0"));
		setaCen.SetActive((direcaoAindicar == "1"));
		setaDir.SetActive((direcaoAindicar == "2"));

		gameFlow.frameChute.SetActive (true);
		btnsAndQuestion.SetActive (true);

		//180410
		if (probs.attentionPointActive()) {   //180410 if parametrized, show "attention point" in middle screen
			attentionPointColor (0);          //on Inspector: 0: start, 1:correct, 2:wrong
		}

		movementTimeA = Time.realtimeSinceStartup; //170309 para nao precisar descontar tempo das animacoes (impreciso)
		decisionTimeB = Time.realtimeSinceStartup; //170113 apareceu "aperte tecla": inicia-se a contagem do tempo de decisão

//		Debug.Log("@CEL:Time.realtimeSinceStartup = "+ Time.realtimeSinceStartup);
//		Debug.Log("@CEL:--------------------gameFlow.startSessionTime = "+ gameFlow.startSessionTime);
		// Debug.Log("@CEL:++++ decisionTimeA = "+ movementTimeA);
		// Debug.Log("@CEL:++++ decisionTimeA = "+ decisionTimeA);
		
		//170915 se está nesta rotina, não está pausado, logo, garantir os botoes Play/Pause
		if (probs.getShowPlayPauseButton ()) {
			if (!pausePressed) {
				buttonPause.SetActive (true);
				buttonPlay.SetActive (false);
			}
		}

		//170311 remove "aperteTecla" after EXIT cancelado
		gameFlow.bmMsg.SetActive (false);                  //BM msg tutorial ou aperteTecla
		gameFlow.aperteTecla.SetActive (false);            //BM msg aperteTecla
	}



	//--------------------------------------------------------------------------------------------------------
	//170102 mostrar tela para catar sequencia até que user acerte 3x (obsoleto)
	//       mostraSequ4, "aperte uma tecla quando decorou"
	public void showFirstScreenMD(int gameSelected)
	{
		gameFlow.mdFirstScreen.SetActive (true);
		mdFrameIndicaChute1.SetActive (true);
		mdFrameIndicaChute2.SetActive (true);
		mdFrameIndicaChute3.SetActive (true);
		mdFrameIndicaChute4.SetActive (true);

		mostrarSequ.SetActive (false);      //botao Mostrar ao sumir sequ
		jogar.SetActive (false);            //botao Jogar ao sumir sequ
		menuJogos.SetActive(false);         //170311 botao MenuJogos, para desistir do EXIT
		btnExit.SetActive(false);           //170311 na tela dos simbolos nao vale o EXIT dos demais jogos

		//180410 memorization phase, not show attentionPoint
		if (probs.attentionPointActive()) {  //180410 if parametrized, show "attention point" in middle screen
			attentionPoint.SetActive(false); //on Inspector first image is green (0), second is red (1)
		}

		if (gameSelected == 3) {
			btnsAndQuestion.SetActive (true);
			mdMsg.SetActive (false);
			aguardandoTeclaMemoria = false;
			movementTimeA = Time.realtimeSinceStartup; //170309 para nao precisar descontar tempo das animacoes (impreciso)
		} else {
			btnsAndQuestion.SetActive (false);
			mdMsg.SetActive (true);
			aguardandoTeclaMemoria = true;
		}

		showMDsequence( probs.getMDsequ() );   //170102 cuidado para nao gerar novamente!
//		stopwatch = 0;                         //170309 trocado por movementTime;  170113 MD ao colocar a sequencia na tela, comeca a contagem

		decisionTimeA = Time.realtimeSinceStartup; //170213 log JM firstScreen: tempo desde que aparece a tela com os símbolos
	}



	//--------------------------------------------------------------------------------------------------------
	//170214 criar uma rotina intermediaria, ao invés de ir direto para showFirstScreenMD, para contar tempo
	//       entre "aperte uma tecla quando pronto" e "mostrar sequ";
	//       chamada no click do MostrarSequ, no Inspector
	public void showSequMDagain(int gameSelected)
	{
		RandomEvent eLog = new RandomEvent ();
		eLog.decisionTime = decisionTimeB - decisionTimeA;       //170214: tempo desde que aparece a tela até que
		eLog.time = Time.realtimeSinceStartup - decisionTimeB;   //170214: tempo desde que apertou "aperte uma tecla quando pronto" até selecionar um botao "Mostrar de novo" ou "Jogar"
		_eventsFirstScreen.Add(eLog);

		showFirstScreenMD(gameSelected);
	}


	//--------------------------------------------------------------------------------------------------------
	//170124 esconder "aperte tecla quando pronto" e trazer teclas de "mostrar sequ" ou "jogar"
	public void hideMDSequence()
	{
		mdFrameIndicaChute1.SetActive (false);
		mdFrameIndicaChute2.SetActive (false);
		mdFrameIndicaChute3.SetActive (false);
		mdFrameIndicaChute4.SetActive (false);

		mdMsg.SetActive (false);
		mostrarSequ.SetActive (true);
		jogar.SetActive (true);
		menuJogos.SetActive(true);                 //170311 botao MenuJogos, para desistir dado que nao ha o EXIT
		btnExit.SetActive(false);                  //170313 idem
//		btnExitFirstScreen.SetActive(false);       //170313 idem
		aguardandoTeclaMemoria = false;
	}



	//--------------------------------------------------------------------------------------------------------
	//170102 mostrar os chutes sorteados na tela
	//171110 2choices: naturalmente as sequ=1 ficarão falsas
	public void showMDsequence(string sequ)
	{
		mdSequChute1 [0].SetActive ( (sequ.Substring (0, 1) == "0") );
		mdSequChute1 [1].SetActive ( (sequ.Substring (0, 1) == "1") );
		mdSequChute1 [2].SetActive ( (sequ.Substring (0, 1) == "2") );

		mdSequChute2 [0].SetActive ( (sequ.Substring (1, 1) == "0") );
		mdSequChute2 [1].SetActive ( (sequ.Substring (1, 1) == "1") );
		mdSequChute2 [2].SetActive ( (sequ.Substring (1, 1) == "2") );

		mdSequChute3 [0].SetActive ( (sequ.Substring (2, 1) == "0") );
		mdSequChute3 [1].SetActive ( (sequ.Substring (2, 1) == "1") );
		mdSequChute3 [2].SetActive ( (sequ.Substring (2, 1) == "2") );

		mdSequChute4 [0].SetActive ( (sequ.Substring (3, 1) == "0") );
		mdSequChute4 [1].SetActive ( (sequ.Substring (3, 1) == "1") );
		mdSequChute4 [2].SetActive ( (sequ.Substring (3, 1) == "2") );
	}




	//--------------------------------------------------------------------------------------------------------
	//170327 acrescentar param para indicar se o Quit veio da BetweenLevels (1) ou pelo botao de Exit do canto superior direito
	public void QuitGame(int whatScreen)
	{
		if (!Screen.fullScreen)
		{
			Application.OpenURL("https://game.numec.prp.usp.br");
			return; //TODO: remove if it isn't necessary
		}

		if (whatScreen == 2) {
			//170417 estava demorando muito tempo se o user apenas quisesse olhar a primeira tela e Exitar
			//170418 se Exit no anim321 deve-se aguardar terminar a animacao
			float stopTime;
			if (animCountDown) {
				//170824 calcular o tempo que falta para acabar a animação;
				//       normalizedTime = % de tempo que já rodou (módulo 1.0f para remover a primeira parte: #vezes que rodou)
				//       tempo que já rodou = tempo total da animação * % de tempo que já rodou
				//       tempo que falta para acabar = tempo total da animação - tempo que já rodou
				float timeToEnd = 3.1f - (3.1f * (this.anim321.GetCurrentAnimatorStateInfo (0).normalizedTime % 1.0f));
				stopTime = timeToEnd;

			} else {
				if (eventCount == 0) {
					stopTime = 0.0f;
				} else {
					stopTime = probs.animationTime ();
				}
			}

			//StartCoroutine (gameFlow.waitTime(PlayerPrefs.GetInt ("gameSelected"), probs.animationTime (), whatScreen));
			StartCoroutine (gameFlow.waitTime(PlayerPrefs.GetInt ("gameSelected"), stopTime, whatScreen));
		}
	}



	//--------------------------------------------------------------------------------------------------------
	public void Sair ()
	{
		//170322 unity3d tem erro ao usar application.Quit
		//       workaround: http://unity3dtrenches.blogspot.com.br/2015/10/unity-3d-compiled-game-freezes-or.html
		//Application.Quit ();
		if (!Application.isEditor) {  //if in the editor, this command would kill unity...
			if (Application.platform == RuntimePlatform.WebGLPlayer) {
				Application.OpenURL ("https://game.numec.prp.usp.br");
			} else {
				//171121 not working kill()
				if ((Application.platform == RuntimePlatform.IPhonePlayer) ||
					(SystemInfo.deviceModel.Contains("iPad"))) {           //try #IF UNITY_IOS
					Application.Quit ();
				} else {
					System.Diagnostics.Process.GetCurrentProcess ().Kill ();
				}
			}
		}
	}


	//--------------------------------------------------------------------------------------------------------
	public void OnEnable()	{
		OnAnimationEnded += PostAnimThings;
	}


	//--------------------------------------------------------------------------------------------------------
	public void OnDisable()	{
		OnAnimationEnded -= PostAnimThings;
	}


	//--------------------------------------------------------------------------------------------------------
	int centerStateHash;
	int currentState;
	//	IEnumerator Start ()
	void Start ()
	{
		probs = ProbCalculator.instance;
		gameFlow = GameFlowManager.instance;  // para fechar objetos
	
		cronosIn.GetComponent<Text>().text = decisionTimeA.ToString();

		//171005 declarar a instance para permitir chamar rotinas do outro script
		translate = LocalizationManager.instance;

		//171006 textos a alterar na interface
		setaCen.GetComponentInChildren<Text>().text = translate.getLocalizedValue ("cen");
		setaEsq.GetComponentInChildren<Text>().text = translate.getLocalizedValue ("esq");
		setaDir.GetComponentInChildren<Text>().text = translate.getLocalizedValue ("dir");

		//171010 botoes MD (Jogo da Memoria)
		mostrarSequ.GetComponentInChildren<Text>().text = translate.getLocalizedValue ("mdBack");
		jogar.GetComponentInChildren<Text>().text = translate.getLocalizedValue ("mdPlay");
		menuJogos.GetComponentInChildren<Text>().text = translate.getLocalizedValue ("mdMenu").Replace("\\n","\n");


		//171031 to decide what sound/animation to choose
		locale = translate.getLocalizedValue ("locale");

		//171031 based on locale, select the correct animation/sound and remove unused
		//171222 created Spanish/Spain locale
		if (locale == "pt_br") {
			pegoal = pegoalPtBr;
			perdeu = perdeuPtBr;
			sound321 = sound321ptbr;

			Destroy (pegoalEnUs); Destroy (pegoalEsEs);
			Destroy (perdeuEnUs); Destroy (perdeuEsEs);
		} else {
			if (locale == "en_us") {
				pegoal = pegoalEnUs;
				perdeu = perdeuEnUs;
				sound321 = sound321enus;

				Destroy (pegoalPtBr); Destroy (pegoalEsEs);
				Destroy (perdeuPtBr); Destroy (perdeuEsEs);
			} else {
				if (locale == "es_es") {
					pegoal = pegoalEsEs;
					perdeu = perdeuEsEs;
					sound321 = sound321eses;

					Destroy (pegoalPtBr); Destroy (pegoalEnUs);
					Destroy (perdeuPtBr); Destroy (perdeuEnUs);
				}
			}
		}


		int targetAnim = probs.GetCurrMachineIndex ();
		if ((targetAnim >= gkAnim.Length) || (gameFlow.jogarMDfase3 && ((PlayerPrefs.GetInt ("gameSelected") == 3) || (PlayerPrefs.GetInt ("gameSelected") == 5))))   //170125
		{
			targetAnim = gkAnim.Length - 1;
		}
		centerStateHash = gkAnim[targetAnim].gk.GetCurrentAnimatorStateInfo(0).shortNameHash;
		currentState = centerStateHash;

		//180413 shift arrows (AQ/AR) if "attentionPoint":"true"
		if (probs.attentionPointActive ()) {
			var frame = setaEsq.transform.parent.GetComponent<Transform> ();

			float posX = attentionPoint.transform.position.x - 200f;
			float posY = setaEsq.transform.parent.GetComponent<Transform> ().position.y;

			frame.position = new Vector2 (posX, posY);
			setaEsq.transform.position = new Vector2 (posX, posY);
			setaCen.transform.position = new Vector2 (posX, posY);
			setaDir.transform.position = new Vector2 (posX, posY);
		}

		//180418 to resize the array and initialize
		keyboardTimeMarkers = new float[10];
		initKeyboardTimeMarkers ();

		// 170822 ==================================================================
		//        definir texto da mensagem dependendo de ambiente, no Jogo da Memoria (md);
		// 171122 iOS (iPad/iPhone)
		if ((Application.platform == RuntimePlatform.Android)  ||
			(Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad"))) {
			//171010
			//mdMsg.GetComponentInChildren<Text>().text = "Toque na tela\nquando estiver pronto!";
			mdMsg.GetComponentInChildren<Text>().text = translate.getLocalizedValue ("toqueMD").Replace("\\n","\n");
		} else {
			//171010
			//mdMsg.GetComponentInChildren<Text>().text = "Aperte uma tecla\nquando estiver pronto!";
			mdMsg.GetComponentInChildren<Text>().text = translate.getLocalizedValue ("aperteMD").Replace("\\n","\n");
		}

		//180411 ==================================================================
		//keep the attention point with the size and colors parametrized
		if (probs.attentionPointActive ()) {
			attentionPoint.transform.localScale += new Vector3 (probs.attentionDiameter(), probs.attentionDiameter(), 0f);

		}

	}

	//--------------------------------------------------------------------------------------------------------
	void Update ()
	{
		if (failedRegisterUserEntry) return;
		
		int currAnim = probs.GetCurrMachineIndex ();
		bool estouNoPegaQualquerTecla = false;
		int number;

		/*
		 * Nunca entra aqui mas é obrigatorio para acertar o gkAnim correto nos hashes,
		 * mas é no caso de pular direto para campo profissional.
		 */
		if ((currAnim >= gkAnim.Length) || (gameFlow.jogarMDfase3 
		                                    && ((PlayerPrefs.GetInt ("gameSelected") == 3) 
		                                        || (PlayerPrefs.GetInt ("gameSelected") == 5)))) {
			currAnim = gkAnim.Length - 1;
		}

		//180418 teclas numéricas de 1 a 0 para servirem de marcador para o experimentador
		if (Input.GetKeyDown ("1") || Input.GetKeyDown ("2") || Input.GetKeyDown ("3") || Input.GetKeyDown ("4") ||
			Input.GetKeyDown ("5") || Input.GetKeyDown ("6") || Input.GetKeyDown ("7") || Input.GetKeyDown ("8") ||
			Input.GetKeyDown ("9") || Input.GetKeyDown ("0")) {
			int.TryParse(Input.inputString, out number);
			keyboardTimeMarkers [number] = Time.realtimeSinceStartup - gameFlow.startSessionTime;
		}


		//170915 encebolar o pegaInput para valer se nao está pausado
		if (!pausePressed) {
			//============================================================================
			//180402 accept pausePlay key (on/off), but only when permitted
			if (Input.GetKeyDown (probs.playPauseKey())) {
				if (probs.getShowPlayPauseButton() && !gameFlow.firstScreen && buttonPause.activeSelf) {
					clickPausePlay ();
				}
			}


			//============================================================================
			//170124 catch key "press any key when ready"
			if (aguardandoTeclaMemoria && (PlayerPrefs.GetInt ("gameSelected") == 5)) {

				if (Input.anyKey) {        //para aceitar qualquer tecla!
					//170310 //para aceitar qualquer tecla... menos o click no botao de EXIT
					//170310 em https://docs.unity3d.com/ScriptReference/Input.GetMouseButtonDown.html
					if (!(Input.GetMouseButtonDown (0) || Input.GetMouseButtonDown (1) || Input.GetMouseButtonDown (2))) {     //170322 left/center/right
						decisionTimeB = Time.realtimeSinceStartup;

						aguardandoTeclaMemoria = false;
						estouNoPegaQualquerTecla = true;  //170110 para aceitar qualquer tecla, inclusive as do jogo

						hideMDSequence ();
					}
				}
			}

			// ============================================================================
			if (aguardandoTeclaBMcomTempo && (PlayerPrefs.GetInt ("gameSelected") == 4)) {
				if (Input.anyKey) {
					//170310 //to accept any key... except click on EXIT button
					//170310 in https://docs.unity3d.com/ScriptReference/Input.GetMouseButtonDown.html
					if (!(Input.GetMouseButtonDown (0) || Input.GetMouseButtonDown (1) || Input.GetMouseButtonDown (2))) {  //170322 só vale click no EXIT...
						decisionTimeA = Time.realtimeSinceStartup - decisionTimeA;  //170113 desde a msg "qualquer tecla", ate que user apertou anyKey

						btnExit.SetActive (true);  //170322 sai o "aperte tecla" passa a valer o EXIT
						gameFlow.bmMsg.SetActive (false);
						gameFlow.aperteTecla.SetActive (false); //BM msg aperteTecla
						gameFlow.frameChute.SetActive (false);
						btnsAndQuestion.SetActive (false);

						aguardandoTeclaBMcomTempo = false;
						estouNoPegaQualquerTecla = true;  //170110 to accept any key including playing game

						//171031 select pt-br or en-us sound
						if (locale == "pt_br") {
							sound321.gameObject.SetActive (true);  //170825 para sincronizar som/imagem no 321
							sound321.enabled = true;               //170825 para sincronizar som/imagem no 321...
						} else {
							if (locale == "en_us") {
								sound321enus.gameObject.SetActive (true);  //171031 to synchronize soundEnUs/imagen 321
								sound321enus.enabled = true;               //171031 to synchronize soundEnUs/imagen 321
							} else {
								if (locale == "es_es") {
									sound321eses.gameObject.SetActive (true);  //171222 to synchronize soundEsEs/imagen 321
									sound321eses.enabled = true;               //171222 to synchronize soundEnUs/imagen 321
								}
							}
						}

						anim321.enabled = true;
						anim321.SetTrigger ("anim321");
						animCountDown = true;
						StartCoroutine (WaitThenDoThings (3.2f)); //170417 era 3.4 mas havia uam latência extra; a animacao dura exatos 3s
					}
				}
			}

			//============================================================================
			AnimatorStateInfo currentBaseState = gkAnim [currAnim].gk.GetCurrentAnimatorStateInfo (0);

			if (BtwnLvls)
				return;
			if (currentState != currentBaseState.shortNameHash) {
				if (currentBaseState.shortNameHash == centerStateHash) {
					if (OnAnimationEnded != null)
						OnAnimationEnded ();
				}
			}

			if (Application.platform == RuntimePlatform.WebGLPlayer && !Screen.fullScreen && Input.anyKey)
			{
				showMsg.GetComponent<Text>().text = translate.getLocalizedValue("txtAbort").Replace("\\n", "\n");
				return;
			}

			// ============================================================================
			//180402 playing: to avoid capture keys when gameOver/gameLover active
			//170223 if msg "relax time" only spaces could be accepted
			if ((btnsAndQuestion.activeSelf) && !estouNoPegaQualquerTecla && !aguardandoTeclaPosRelax && gameFlow.playing) {

				//180410 if parametrized, show "attention point" in middle screen
				if (probs.attentionPointActive()) {
					// 20190702 - Mantem a bola Laranja por cerca de 1 segundo e troca para a Verde 
//						attentionPointColor (0); //on Inspector: 0: start, 1:correct, 2:wrong
				}

				if (Input.GetKeyDown (KeyCode.DownArrow) || Input.GetKeyDown (KeyCode.UpArrow) ||
				    Input.GetKeyDown (probs.acceptedKey (1))) {           //180328 user defined input key for center defense
					//171109 nao aceitar centro se 2choices
					if (probs.getChoices () == 3) {
						BtnActionGetEvent ("centro");
					}
				} else {                                                 //170112 alterado para if/else pq soh entrarah em um caso
					if (Input.GetKeyDown (KeyCode.LeftArrow) ||
					    Input.GetKeyDown (probs.acceptedKey (0))) {      //180328 user defined input key for left defense
						BtnActionGetEvent ("esquerda");
						//btnsAndQuestion.SetActive (false);
					} else {
						if (Input.GetKeyDown (KeyCode.RightArrow) ||
						    Input.GetKeyDown (probs.acceptedKey (2))) {    //180328 user defined input key for right defense
							BtnActionGetEvent ("direita");
						}
					}
				}
			}

			currentState = currentBaseState.shortNameHash;
		} else {
			//============================================================================
			//180402 accept pausePlay key (on/off), but only if permitted
			if (Input.GetKeyDown (probs.playPauseKey ())) {
				if (probs.getShowPlayPauseButton() && !gameFlow.firstScreen && buttonPlay.activeSelf) {
					clickPausePlay ();
				}
			}
		}
	}


	//--------------------------------------------------------------------------------------
	//170111 coroutine para aguardar tempo enquanto a animacao nao termina
	public IEnumerator WaitThenDoThings(float time)   //170203 publica, para ser acessada no gameFlow.
	{
		yield return new WaitForSeconds(time);

		//acabou de aparecer a imagem, faca isto
		if (animCountDown) {
			//print("acabou 321");
			//se houver um Exit pendente, aparecerah o simbolo e logo a seguir a tela (abandonar?)
			showNextKick (probs.GetNextKick ());
			animCountDown = false;

			//170915
			if (probs.getShowPlayPauseButton ()) {
				buttonPause.SetActive (true);
				buttonPlay.SetActive (false);
			}

			//171031 select pt-br or en-us sound
			if (locale == "pt_br") {
			   sound321.enabled = false; //170825 para resetar o som (aparentemente
			} else {
				if (locale == "en_us") {
					sound321enus.enabled = false; //171031 to reset the sound
				} else {
					if (locale == "es_es") {
						sound321eses.enabled = false; //171222 to reset the sound
					}
				}
			}
		}

		if (animResult) {
			animResult = false;

			//print("acabou defendeu ou perdeu");
			//170112 se estah para ir para a tela de betweenlevels nao fazer os acertos de objetos
			if (!BtwnLvls) {
				if (PlayerPrefs.GetInt ("gameSelected") == 1) {   //BM
					showNextKick (probs.GetNextKick ());
				} else {
					if (PlayerPrefs.GetInt ("gameSelected") == 4) {
					    aguardandoTeclaBMcomTempo = true;         //AR = AQ com tempo (antigo Base Motora)
						gameFlow.bmMsg.SetActive (true);          //BM frame msg tutorial ou aperteTecla
						gameFlow.aperteTecla.SetActive (true);    //BM msg aperteTecla
						gameFlow.frameChute.SetActive (false);
						btnsAndQuestion.SetActive (true);         //fica apenas a msg "aperte uma tecla"
					    btnExit.SetActive(false);                 //170418 enquanto "aperte tecla" nao vale o EXIT

					    //170914 se está aqui já nao é a 1a jogada, entao, no "aperte tecla" nao valem os botoes Play/Pause
					    buttonPause.SetActive(false);
					    buttonPlay.SetActive(false);

						decisionTimeA = Time.realtimeSinceStartup; //170113 apareceu "aperte tecla": inicia-se a contagem do tempo de decisão
					    movementTimeA = Time.realtimeSinceStartup; //170309 para nao precisar descontar tempo das animacoes (impreciso)

					} else { //if ((PlayerPrefs.GetInt ("gameSelected") == 2) || (PlayerPrefs.GetInt ("gameSelected") == 3) || (PlayerPrefs.GetInt ("gameSelected") == 5)) {   //JG ou MD ou JMemo
						btnsAndQuestion.SetActive (true);

					    //170920 voltando de uma animação; se for o caso, ativar Play/Pause
					    if (probs.getShowPlayPauseButton ()) {
							buttonPause.SetActive (true);
							buttonPlay.SetActive (false);
						}

						//170307 reiniciar contagem do tempo: desde que aparecem as teclas de defesa
					    movementTimeA = Time.realtimeSinceStartup; //170309 para nao precisar descontar tempo das animacoes (impreciso)
						decisionTimeA = Time.realtimeSinceStartup;  //170307 apareceram as setas de defesa: inicia-se a contagem do tempo de movimento
					}
				}
				RandomEvent eLog = new RandomEvent ();
				eLog.time = Time.realtimeSinceStartup - movementTimeA - gameFlow.otherPausesTime ;
			}
		}
	}

	//170623 rotinas que acessam a DLL de acesso à paralela (EEG)
	//       com base nos testes em C:\Users\HP\Documents\1.Neuromat\acessoParalela\Assets\Scripts

	//---------------------------------------------------------------------------------------
	//170906 botão Play/Pause clicado (no canto superior direito, ao lado do Exit)
	public void clickPausePlay ()
	{
		if (pausePressed) {
			// ------------------------------------------------------------------------------- Play pressed
			//Debug.Log ("PLAY: estava pausado, mostrando bPlay; agora deve virar bPause e iniciar o jogo");

			//170918 param showPlayPauseButton false, então:
			//       1) se "Jogar", não mostrar o Play/Pause (abrazo: tela limpa)
			//       2) se "Jogar com pausa", entrar com Pause apenas na primeira jogada (amparo: explicacao do jogo)
			if (!probs.getShowPlayPauseButton ()) {
				if (PlayerPrefs.GetInt ("gameSelected") == 5 && gameFlow.firstScreen) {
					gameFlow.changeAlpha (5, 1.0f);
					mdButtonPause.SetActive (false);
					mdButtonPlay.SetActive (false);
				} else {
					if (PlayerPrefs.GetInt ("gameSelected") == 4) {
						gameFlow.changeAlpha (4, 1.0f);
					}
					buttonPause.SetActive (false);
					buttonPlay.SetActive (false);
				}

			} else {
				if (PlayerPrefs.GetInt ("gameSelected") == 4) {
					gameFlow.changeAlpha (4, 1.0f);
					if (aguardandoTeclaBMcomTempo) {
						buttonPause.SetActive (false);
						buttonPlay.SetActive (false);
					} else {
						buttonPause.SetActive (true);
						buttonPlay.SetActive (false);
					}

				} else {
					if (PlayerPrefs.GetInt ("gameSelected") == 5 && gameFlow.firstScreen) {
						gameFlow.changeAlpha (5, 1.0f);
						mdButtonPause.SetActive (false);
						mdButtonPlay.SetActive (false);
					} else {
						buttonPause.SetActive (!buttonPause.activeSelf);
						buttonPlay.SetActive (!buttonPlay.activeSelf);
					}
				}
			}

			//170912 se parada inicial (startpaused + eventCount=0) para explicação, acertar os tempos;
			if (PlayerPrefs.GetInt("startPaused") == 1  &&  eventCount == 0) {
				gameFlow.initialPauseTime = Time.realtimeSinceStartup - gameFlow.startSessionTime;
			}
			//170919 mas depois da explicação ainda pode haver paradas;
			//       somar o tempo corrente em pausa, aas pausas anteriores neste mesmo jogo
			if (gameFlow.startOtherPausesTime > 0) {  //se foi iniciado num inicio de pausa
				gameFlow.otherPausesTime = gameFlow.otherPausesTime + (Time.realtimeSinceStartup - gameFlow.startOtherPausesTime);
			}

			pausePressed = false;

		} else {
			// ------------------------------------------------------------------------------- Pause pressed
			//Debug.Log ("PAUSE: estava rodando, mostrando bPause; agora deve virar bPlay e parar o jogo");
			if (PlayerPrefs.GetInt ("gameSelected") == 4) {
				//gameFlow.changeAlpha (4, 0.5f);
				buttonPause.SetActive (false);
				buttonPlay.SetActive (true);
			} else {
				if (PlayerPrefs.GetInt ("gameSelected") == 5  && gameFlow.firstScreen) {
					//gameFlow.changeAlpha (5, 0.5f);
					mdButtonPause.SetActive (false);
					mdButtonPlay.SetActive (true);
				} else {
					buttonPause.SetActive (!buttonPause.activeSelf);
					buttonPlay.SetActive (!buttonPlay.activeSelf);
				}
			}

			//170919 se estava rodando não é parada inicial para explicação, acertar os tempos
			gameFlow.numOtherPauses = gameFlow.numOtherPauses + 1;
			gameFlow.startOtherPausesTime = Time.realtimeSinceStartup;

			pausePressed = true;
		}
	}


	//---------------------------------------------------------------------------------------
	//180418 reset array
	public void initKeyboardTimeMarkers ()
	{
		for (int i = 0; i <= 9; i++) {
			keyboardTimeMarkers [i] = 0.0f;
		}
	}
		
	//---------------------------------------------------------------------------------------
	//180510 apply correct phase speedGKAnim; there is only 3 field scenarios -
	//       when there is more than 3 phases, the scenario is always the last: professional (until do more);
	//       the speed is the same for all: player, ball and goalkeeper
	public void initSpeedGKAnim ()
	{
		int aux;
		aux = (probs.GetCurrMachineIndex () >= gkAnim.Length) ? gkAnim.Length - 1 : probs.GetCurrMachineIndex ();
		gkAnim [aux].player.speed = probs.speedGKAnim(probs.GetCurrMachineIndex ());
		gkAnim [aux].ball.speed = gkAnim [aux].player.speed;
		gkAnim [aux].gk.speed = gkAnim [aux].player.speed;
	}

	public void RegisterPlay (MonoBehaviour mb, string locale, float endSessionTime, string stageID, bool gameMode, int phaseNumber, 
		int totalPlays, int totalCorrect, float successRate, int bmMinHits, int bmMaxPlays, int bmMinHitsInSequence,
		List<RandomEvent> log, bool interrupted, List<RandomEvent> firstScreenMD, string animationType, int playsToRelax,
		bool showHistory, string sendMarkersToEEG, string portEEGserial, string groupCode, bool scoreboard,
		string finalScoreboard, string treeContextsAndProbabilities, int choices, bool showPlayPauseButton,
		int jgMinHitsInSequence, int mdMinHitsInSequence, int mdMaxPlays, string institution, bool attentionPoint,
		string attentionDiameter, string attentionColorStart, string attentionColorCorrect, string attentionColorWrong,
		string speedGKAnim, string portSendData, string timeFaixa0, string timeFaixa1, string timeFaixa2,
		string timeFaixa3, string timeFaixa4, float[] keyboardTimeMarkers)

	{
		if ((Application.platform != RuntimePlatform.WebGLPlayer) && (Application.platform != RuntimePlatform.Android) &&
			(Application.platform != RuntimePlatform.IPhonePlayer) && (!SystemInfo.deviceModel.Contains("iPad"))) {
			backupResults = Application.dataPath + "/ResultsBk";

			try {
				if (!Directory.Exists (backupResults)) {
					Directory.CreateDirectory (backupResults);
				}
			} catch (IOException ex) {
				Debug.Log ("Error creating Results Backup directory (ResultsBk): " + ex.Message);
			}
		}

		//Josi: using StringBuilder; based https://forum.unity3d/threads/how-to-write-a-file.8864
		//      caminhoLocal/Plays_grupo1-v1_HP-HP_YYMMDD_HHMMSS_fff.csv
		//      string x stringBuilder: em https://msdn.microsoft.com/en-us/library/system.text.stringbuilder(v=vs.110).aspx
		int gameSelected = PlayerPrefs.GetInt ("gameSelected");

		gamePlayed.Length = 0;
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

		tmp = (1000 + UnityEngine.Random.Range (0, 1000)).ToString().Substring(1,3);
		LogGame.Length = 0;

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

			if (gameMode) {
				tmp = "readSequence";
			} else {
				tmp = "readTree";
			}

			sr.WriteLine ("currentLanguage,{0}", Application.systemLanguage.ToString ());
			// operatingSystem could have commas like in "iPad4,2" or "iMac12,1" destroying the CSV format
			sr.WriteLine ("operatingSystem,{0} [{1}]", SystemInfo.deviceModel.Replace (",", "."), SystemInfo.operatingSystem.Replace (",", "."));
			sr.WriteLine ("ipAddress,{0}", PlayerPrefs.GetString ("IP"));
			sr.WriteLine ("ipCountry,{0}", PlayerPrefs.GetString ("Country"));
			sr.WriteLine ("gameVersion,{0}", PlayerPrefs.GetString ("version"));
			sr.WriteLine ("gameLanguage,{0}", locale);
			sr.WriteLine ("institution,{0}", institution);
			sr.WriteLine ("soccerTeam,{0}", PlayerPrefs.GetString ("teamSelected"));
			sr.WriteLine ("game,{0}", gamePlayed.ToString ().Substring (1, 2));
			sr.WriteLine ("playID,{0}", stageID.Trim ());
			sr.WriteLine ("phase,{0}", phaseNumber.ToString ());                        
			sr.WriteLine ("choices,{0}", choices.ToString ());
			sr.WriteLine ("showPlayPauseButton,{0}", showPlayPauseButton.ToString ());
			sr.WriteLine ("pausePlayInputKey,{0}", ProbCalculator.machines [0].pausePlayInputKey); 
			sr.WriteLine ("sessionTime,{0}", (endSessionTime).ToString ("f6").Replace (",", "."));
			sr.WriteLine ("relaxTime,{0}", mb.GetComponent<GameFlowManager> ().totalRelaxTime.ToString ("f6").Replace (",", "."));
			sr.WriteLine ("initialPauseTime,{0}", mb.GetComponent<GameFlowManager> ().initialPauseTime.ToString ("f6").Replace (",", "."));
			sr.WriteLine ("numOtherPauses,{0}", mb.GetComponent<GameFlowManager> ().numOtherPauses.ToString ());
			sr.WriteLine ("otherPausesTime,{0}", mb.GetComponent<GameFlowManager> ().otherPausesTotalTime.ToString ("f6").Replace (",", "."));
			sr.WriteLine ("attentionPoint,{0}", attentionPoint.ToString ());
			sr.WriteLine ("attentionDiameter,{0}", attentionDiameter);
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


			// Send a warning if game was interrupted by the user
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

			// Changed the style one line has all data (criated to facilitate IMEjr analysis), for "variable, content";
			sr.WriteLine ("status,{0}", tmp);
			sr.WriteLine ("playsToRelax,{0}", playsToRelax.ToString ());
			sr.WriteLine ("scoreboard,{0}", scoreboard.ToString ());
			sr.WriteLine ("finalScoreboard,{0}", finalScoreboard);
			sr.WriteLine ("animationType,{0}", animationType);
			sr.WriteLine ("showHistory,{0}", showHistory.ToString ());
			sr.WriteLine ("sendMarkersToEEG,{0}", sendMarkersToEEG);
			sr.WriteLine ("portEEGserial,{0}", portEEGserial);
			sr.WriteLine ("groupCode,{0}", groupCode); 
			sr.WriteLine ("leftInputKey,{0}", ProbCalculator.machines [0].leftInputKey);
			sr.WriteLine ("centerInputKey,{0}", ProbCalculator.machines [0].centerInputKey);
			sr.WriteLine ("rightInputKey,{0}", ProbCalculator.machines [0].rightInputKey);
			sr.WriteLine ("speedGKAnim,{0}", speedGKAnim);
			sr.WriteLine ("portSendData,{0}", portSendData);
			sr.WriteLine ("timeFaixa0,{0}", timeFaixa0);
			sr.WriteLine ("timeFaixa1,{0}", timeFaixa1);
			sr.WriteLine ("timeFaixa2,{0}", timeFaixa2);
			sr.WriteLine ("timeFaixa3,{0}", timeFaixa3);
			sr.WriteLine ("timeFaixa4,{0}", timeFaixa4);

			// Following keyboard number order: 1,2,...,8,9,0
			for (int i = 1; i <= 9; i++) {
				sr.WriteLine("keyboardMarker" + i.ToString() + ",{0}", keyboardTimeMarkers[i].ToString ("f6").Replace("," , "." ) );
			}
			sr.WriteLine("keyboardMarker0,{0}", keyboardTimeMarkers[0].ToString ("f6").Replace("," , "." ) );

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



			// Sequencia executada (NES) pelo computador
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
					sequExecutada.Insert (line, l.resultInt.ToString ());
					line++;
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

			line = 0;   
			foreach (RandomEvent l in log) {
				//170713 some machines generate decimal with commas (locale?)
				//170217 capitalize FALSE    //(l.correct ? "TRUE" : "false")
				//170919 pauseTime of the play
				tmp = l.resultInt.ToString () + "," + l.ehRandom + "," + l.optionChosenInt.ToString () + "," + (l.correct ? "TRUE" : "false")
				+ "," + l.time.ToString ("f6").Replace (",", ".") + "," + l.pauseTime.ToString ("f6").Replace (",", ".")
				+ "," + l.realTime.ToString ("f6").Replace (",", ".");

				if (gameSelected != 4) {
					sr.WriteLine ("{0},{1}", ++line, tmp);   
				} else {
					sr.WriteLine ("{0},{1},{2}", ++line, tmp, l.decisionTime.ToString("f6").Replace("," , "." ) );            
				}                                                
			}

			//TODO: remove this. It's only for test.
			for (int i = 0; i < 998; i++)
			{
				sr.WriteLine ("{0},{1},{2}", ++line, tmp, "decision time" );
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

			if ((Application.platform == RuntimePlatform.WebGLPlayer) || (Application.platform == RuntimePlatform.Android) ||
				(Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad")))
			{
				buildPost(resultsFileName, resultsFileContent);
			}
		}
	}
	
	private void buildPost(string fileName, string contentFile)
	{
		byte[] fileData = Encoding.UTF8.GetBytes (contentFile);

		string hash = GetHash(contentFile)	;
		hash = GetHash(hash);

		WWWForm formData = new WWWForm ();
		formData.AddField("action", "level upload");
		formData.AddField("ident", hash);
		formData.AddField("file","file");
		formData.AddBinaryData ( "file", fileData, fileName, "text/plain");

		GKGConfigContainer gkgConfig = GKGConfigContainer.Load();
		// TODO: GKGConfigContainer returns a list. First element's list is the webSite corresponding
		// attribute with its elements. Improve access by referencing by name, not by index!
		string loginURL = gkgConfig.configItems[0].URL + "/unityUpload.php";
		
		StartCoroutine(ServerOperations.instance.uploadFile(loginURL, formData, UIManager.instance));
	}
	
	static string GetHash(string input)
	{	
		var sha512Hash = new SHA512CryptoServiceProvider();

		byte[] data = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

		StringBuilder sBuilder = new StringBuilder();
		// Format as a hexadecimal string.
		for (int i = 0; i < data.Length; i++) {
			sBuilder.Append(data[i].ToString("x2"));
		}

		return sBuilder.ToString();
	}
		
}

