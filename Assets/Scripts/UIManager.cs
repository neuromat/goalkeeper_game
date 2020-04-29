/**************************************************************************************/
//  Module written by scaroni <renato.scaroni@gmail.com>
//  Rewrited by Josi Perez <josiperez.neuromat@gmail.com>, keeping the original code in comment
//
/**************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;                       //to use StringBuilder
using UnityEngine.EventSystems;          //170308 to know which was last button clicked - did not work ...


//170623 only valids for Windows (to call inpout32.dll)
#if UNITY_STANDALONE_WIN  || UNITY_EDITOR_WIN
using System.Runtime.InteropServices;    //[DLLimport para envio ao EEG pela porta paralela
#endif
using System;                            //180201 para declara Byte[0] e buildar para linux
using System.IO.Ports;
using System.Net.Mime;

//180104 to use serialPort; works in linux and mac?



//------------------------------------------------------------------------------------
public class RandomEvent            //Josi: result matrix to save experiment results
{
	public int resultInt;           //defense waited: 0, 1 or 2 (if choices=3), or 1,2 (if choices=2)
	public char ehRandom;           //170215 JG: random play? Y, or n; in any other module it is "n" (AQ/AR is random...)
	public int optionChosenInt;     //defense choosed by player; numeric format
	public string result;           //defense waited in string format: dir, esq, cen
	public string optionChosen;     //defense choosed  in string format: dir, esq, cen
	public bool correct;            //correct (true) or no (false)
	public string state;
	public float time;
	public float decisionTime;       //170113 tempo de decisao
	public float pauseTime;          //170919 tempo em pausa (do Play/Pause) nesta jogada
	public float realTime;           //180418 tempo corrido (para analisar com os marcadores)
}



//------------------------------------------------------------------------------------
public class UIManager : MonoBehaviour
{
	private float StartTime;
	//public GameObject cronos;
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


	//170626
	public  int  timeBetweenMarkers = 100000000;        //QG para dar um tempico entre envios à paralela;
	//public  int  timeBetweenMarkersSerial = 10000000; //180129 10^7 time between sendMarkersToSerial on BrainProductsEEG connected to TriggerBox
	                                                    //       can see markers on vmrk and sobreposition on recorder screen at the moment
	//public  int  timeBetweenMarkersSerial = 100000;   //180131 10^5, com samplrate 5000, samplInterval 200; ok!
	//public  int  timeBetweenMarkersSerial = 10000;    //180131 10^4, com samplrate 5000, samplInterval 200; ok!
	//public  int  timeBetweenMarkersSerial = 100;      //180131 10^2, com samplrate 5000, samplInterval 200; perdeu 1 em 48mkr...
	public  int timeBetweenMarkersSerial = 100000;      //180131

	public bool userAbandonModule = false;              //180326 not more possible to decide considering the numPlays (if gamer hits before, it goes out)

	public GameObject attentionPoint;
	//public GameObject frameEEG;
	public GameObject frame0EEG;                //180410 in the middle screen to fix player attention (EEG experiments)
	public GameObject exibeFaixa;
	public GameObject frame1EEG;
	public GameObject frame2EEG;
	public GameObject frame3EEG;
	public GameObject frame4EEG;
	public float[] keyboardTimeMarkers;                 //180418 markers from experimenter (keyboard F1 until F9)


	//170623 DLLs inpout32.dll from http://highrez.co.uk/
	//171017 DLls inpoutx64.dll
/*	#if UNITY_STANDALONE_WIN  || UNITY_EDITOR_WIN
	[DllImport("inpout32")]
	private static extern UInt32 IsInpOutDriverOpen();

	[DllImport("inpout32")]
	private static extern void Out32(ushort PortAddress, ushort Data);

	[DllImport("inpoutx64", EntryPoint = "IsInpOutDriverOpen")]
	private static extern UInt32 IsInpOutDriverOpen_x64();

	[DllImport("inpoutx64", EntryPoint = "Out32")]
	private static extern void Out32_x64(ushort PortAddress, ushort Data);
	#endif
*/
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
		float tempoJogo;
		tempoJogo = Time.realtimeSinceStartup - gameFlow.startSessionTime;

	//	Debug.Log("@BtnActionGetEvent:Time.realtimeSinceStartup = "+ Time.realtimeSinceStartup);
	//	//Debug.Log("movementTimeA = "+ movementTimeA);
	//	//Debug.Log("decisionTimeA = "+ decisionTimeA);
	//	Debug.Log("@BtnActionGetEvent:--------------------gameFlow.startSessionTime = "+ gameFlow.startSessionTime);
	//	Debug.Log("@BtnActionGetEvent:++++ tempoJogo = "+ tempoJogo);

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
			Debug.Log("eLog.time = "+ eLog.time);
			eLog.pauseTime = gameFlow.otherPausesTime;
			eLog.realTime = Time.realtimeSinceStartup - gameFlow.startSessionTime;    //180418 to accomplish marker time by keyboard
			//Debug.Log("eLog.realTime = "+ eLog.realTime);
			//Debug.Log("************************************");


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
//				Debug.Log("timeForYellow_Preta= "+ timeForYellow);
//				exibeFaixa.GetComponent<Text>().text = "Faixa 4";
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
/*
			//170630
			//============================================================
			//180104 if parallel connection, valid for Windows environment only
			//       if serial, anyone
			//if (PlayerPrefs.GetInt ("gameSelected") == 2) {      //180122 valid to all game modules
			if (probs.getSendMarkersToEEG () != "none") {
				if (probs.getSendMarkersToEEG () == "parallel") {
					//-------------------------------------------------------------
					#if UNITY_STANDALONE_WIN  || UNITY_EDITOR_WIN
					//170626 enviar os marcadores EEG se necessario (apenas se JG);
					//       com base na tabela sugerida por Magá (INDC/RJ)
					int j;

					//marcador de direcao de defesa esperada
					if (e == 0) {
						Write (0x02);        //marcador 2: DEFESA ESPERADA aa esquerda
					} else {
						if (e == 1) {
							Write (0x04);    //marcador 4: DEFESA ESPERADA ao centro
						} else {
							Write (0x08);    //marcador 8: DEFESA ESPERADA aa direita
						}
					}
					for (j = 1; j < timeBetweenMarkers; j++) {
						j = j + 1;
					}
					;  //170626 para dar um tempico entre envios à paralela
					Write (0x00);             //170626 envio do marcador zero


					// marcador indicativo de jogada random ou não random
					if (eLog.ehRandom == 'Y') {
						Write (0x10);        //marcador 16: JOGADA RANDOM
					} else {
						Write (0x20);        //marcador 32: JOGADA NAO RANDOM
					}
					for (j = 1; j < timeBetweenMarkers; j++) {
						j = j + 1;
					}
					;  //170626 para dar um tempico entre envios à paralela
					Write (0x00);             //170626 envio do marcador zero


					// marcador de direcao de defesa selecionada
					if (eLog.optionChosenInt == 0) {
						Write (0x02);        //marcador 2: DEFESA DADA aa esquerda
					} else {
						if (eLog.optionChosenInt == 1) {
							Write (0x04);    //marcador 4: DEFESA DADA ao centro
						} else {
							Write (0x08);    //marcador 4: DEFESA DADA aa direita
						}
					}
					for (j = 1; j < timeBetweenMarkers; j++) {
						j = j + 1;
					}
					;  //170626 para dar um tempico entre envios à paralela
					Write (0x00);             //170626 envio do marcador zero
					#endif
					//-------------------------------------------------------------
				} else {
					//180104 only for standalone desktops... not very sure...
					if (probs.getSendMarkersToEEG () == "serial") {
						//-------------------------------------------------------------
						#if UNITY_STANDALONE || UNITY_EDITOR
						//180201 changed to convert 3 markers in 1; need 18 markers changing response by stimuli on EEG BrainProducts;
						//       to avoid marker lost and to avoid a delayer (loop)
						if (e == 0) {
							if (eLog.ehRandom == 'Y') {
								if (eLog.optionChosenInt == 0) {
									data[0] = 0x0a;                                         //0y0
								} else {
									data[0] = (eLog.optionChosenInt == 1) ? Convert.ToByte(0x0b) : Convert.ToByte(0x0c);    //0y1 e 0y2
								}
							} else {
								if (eLog.optionChosenInt == 0) {
									data[0] = 0x0d;                                         //0n0
								} else {
									data[0] = (eLog.optionChosenInt == 1) ? Convert.ToByte(0x0e) : Convert.ToByte(0x0f);    //0n1 e 0n2
								}
							}
						} else {
							if (e == 1) {
								if (eLog.ehRandom == 'Y') {
									if (eLog.optionChosenInt == 0) {
										data[0] = 0x10;                                         //1y0
									} else {
										data[0] = (eLog.optionChosenInt == 1) ? Convert.ToByte(0x11) : Convert.ToByte(0x12);    //1y1 e 1y2
									}
								} else {
									if (eLog.optionChosenInt == 0) {
										data[0] = 0x13;                                         //1n0
									} else {
										data[0] = (eLog.optionChosenInt == 1) ? Convert.ToByte(0x14) : Convert.ToByte(0x15);    //1n1 e 1n2
									}
								}
							} else {  //(e == 2)
								if (eLog.ehRandom == 'Y') {
									if (eLog.optionChosenInt == 0) {
										data[0] = 0x16;                                         //2y0
									} else {
										data[0] = (eLog.optionChosenInt == 1) ? Convert.ToByte(0x17) : Convert.ToByte(0x18);    //2y1 e 2y2
									}
								} else {
									if (eLog.optionChosenInt == 0) {
										data[0] = 0x19;                                         //2n0
									} else {
										data[0] = (eLog.optionChosenInt == 1) ? Convert.ToByte(0x1A) : Convert.ToByte(0x1B);    //2n1 e 2n2
									}
								}
							}
						}
						sendDataToSerial(data);
						#endif
						//-------------------------------------------------------------
					}
				}
			}  //if (probs.getSendMarkersToEEG () != "none")
*/
			//============================================================

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
			Debug.Log("@BtnActionGetEvent:movementTimeA = "+ movementTimeA);
		}
	}



	//--------------------------------------------------------------------------------------------------------
	//Josi: ao trocar de nivel, envia os dados do experimento para arquivo local (a thread se encarrega de enviar o arquivo para o server)
	//170109 nasce jogoJogado como parametro
	public void SendEventsToServer(int gameSelected)
	{
		if ( (_events != null && eventCount > 0) || (_eventsFirstScreen.Count > 0) )
		{   //170108 pode estar na mdFirstScreen que acumula _events dos testes de memoria (eventCount)  ou estar no JM 5 na firstScreen
			//Josi: era assim
			//ServerOperations.instance.RegisterPlay (GameFlowManager.instance, probs.CurrentMachineID(), success, successRate, _events);
			//Josi: 161205: inclui o parametro do modo de operacao do jogo: por sequOtima ou por arvore
			//              inclui saber se o nivel foi interrompido ou nao

			if (PlayerInfo.agree)
			{   //170830 if player agree to give his results, prepare to write file results
				//       else... lost the data (even without identification... that occurs in NES)

				//170316 tempo da sessao (para comparar com os tempos de decisao/movto);
				//       se houve tempo de relax, descontar
				//float relaxTime = gameFlow.endRelaxTime - gameFlow.startRelaxTime;   pode haver mais do que uma parada...
				float endSessionTime = Time.realtimeSinceStartup - gameFlow.startSessionTime;

				//Josi 161229 nao precisa mais do primeiro parametro
				//     170108 resultados da firstScreen do MD
				//     170109 total de jogadas (se interrompido ia o total de vezes jogado)
				int jogadas = probs.GetCurrentPlayLimit (gameSelected);
				int acertos = success;

				//170310 curto e grosso: isto está fixo no restante do script... faltaria pensar um grid que permitisse aumentar os quadros iniciais
				if (gameSelected == 5) {
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


				//170417 montar string para apresentar no arquivo de resultados;
				//       tree="context;prob0;prob1 | context;prob0;prob1 | ...
				string treeContextsAndProbabilities = probs.stringTree ();


				//170126 getCurrentReadSequ(do jogo selecionado) + param numHits (num minimo de jogadas corretas)
				//170216 gameMode lido do arq config, excepto na phase0 do JG onde o jogo lê a sequ dada, espelhada entre grupo1-v1 e grupo2-v1
				//ServerOperations.instance.RegisterPlay (GameFlowManager.instance, probs.CurrentMachineID (), probs.getCurrentReadSequ (gameSelected), jogadas, acertos, successRate, probs.getMinHits(), _events, interrupted, _eventsFirstScreen);
				//170310 enviar phaseNumber
				//170413 enviar machines[currentState] para gravar animationType, scoreboard, finalScoreboard, playsToRelax
				//170417 enviar tree no formato (["context"; "prob0"; "prob1"] ... ["context"; "prob0"; "prob1"])
				//170622 enviar showHistory
				//171025 enviar choices (até agora 3 mas poderá ser 2) e showPlaypauseButton
				//180105 send new parameter getPortEEGserial()
				//180117 send locale selected by player
				//180326 new parameters: minHitsInSequenceForJG, ForJM, mdMaxPlays
				//180417 send speedAnim
				//180418 keyboard markers
				ServerOperations.instance.RegisterPlay (GameFlowManager.instance, locale, endSessionTime, probs.CurrentMachineID (),
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
		//on Inspector: 0: start, 1:correct, 2:wrong
		//troca a bola pela cruz - 20190702 attentionPoint.GetComponentsInChildren<Image>()[0].enabled = (color == 0) ? true : false;
		//troca a bola pela cruz - 20190702 attentionPoint.GetComponentsInChildren<Image>()[1].enabled = (color == 1) ? true : false;
		//troca a bola pela cruz - 20190702 attentionPoint.GetComponentsInChildren<Image>()[2].enabled = (color == 2) ? true : false;
		//troca a bola pela cruz - 20190702 attentionPoint.SetActive (true);

		
		switch (color) //troca a bola pela cruz - 20190702 
		{
			case 0:  // Laranja
// Paulo Roberto pediu para tirar - 20190715				cruzLaranja.gameObject.SetActive(true);
// Paulo Roberto pediu para tirar - 20190715				cruzVerde.gameObject.SetActive(false);
// Paulo Roberto pediu para tirar - 20190715				cruzPreta.gameObject.SetActive(false);
/*
				exibeFaixa.GetComponent<Text>().text = "Faixa 1";
				frame1EEG.SetActive(true);
				frame2EEG.SetActive(false);
				frame3EEG.SetActive(false);
				frame4EEG.SetActive(false);
*/
				break; 

			case 1:  // Verde
// Paulo Roberto pediu para tirar - 20190715				cruzLaranja.gameObject.SetActive(false);
// Paulo Roberto pediu para tirar - 20190715				cruzVerde.gameObject.SetActive(true);
// Paulo Roberto pediu para tirar - 20190715				cruzPreta.gameObject.SetActive(false);
/*
				exibeFaixa.GetComponent<Text>().text = "Faixa 2";
				frame1EEG.SetActive(false);
				frame2EEG.SetActive(true);
				frame3EEG.SetActive(false);
				frame4EEG.SetActive(false);
*/
				break; 

			case 2:  //Preta
// Paulo Roberto pediu para tirar - 20190715				cruzLaranja.gameObject.SetActive(false);
// Paulo Roberto pediu para tirar - 20190715				cruzVerde.gameObject.SetActive(false);
// Paulo Roberto pediu para tirar - 20190715				cruzPreta.gameObject.SetActive(true);
/*
				exibeFaixa.GetComponent<Text>().text = "Faixa 3/4";
				frame1EEG.SetActive(false);
				frame2EEG.SetActive(false);
				frame3EEG.SetActive(false);
				frame4EEG.SetActive(false);
*/
				break; 
		} //troca a bola pela cruz - 20190702
		
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
					//170322 StartCoroutine (WaitThenDoThings (2.4f));

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
						//170322 StartCoroutine (WaitThenDoThings (1.4f));
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
				StartCoroutine (WaitThenDoThings ( probs.animationTime() + extraTime ));  //170322 centralizado em uma rotina os tempos de animacao


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
		Debug.Log("@CEL:++++ decisionTimeA = "+ movementTimeA);
		Debug.Log("@CEL:++++ decisionTimeA = "+ decisionTimeA);
		
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
				Application.OpenURL (PlayerPrefs.GetString ("gameURL"));
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
		gameFlow = GameFlowManager.instance;             //161230 para fechar objetos
		float tempoJogo;

		StartTime = Time.time;
	
		tempoJogo = Time.realtimeSinceStartup - gameFlow.startSessionTime;

		Debug.Log("@START:Time.realtimeSinceStartup = "+ Time.realtimeSinceStartup);
		//Debug.Log("movementTimeA = "+ movementTimeA);
		//Debug.Log("decisionTimeA = "+ decisionTimeA);
		Debug.Log("@START:--------------------gameFlow.startSessionTime = "+ gameFlow.startSessionTime);
		Debug.Log("@START:++++ tempoJogo = "+ tempoJogo);
		Debug.Log("@START:++++ decisionTimeA = "+ decisionTimeA);
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

/*
		//170630
		//============================================================
		//180104 if parallel connection, valid for Windows environment only
		//       if serial, anyone... it is necessary test if IO.Ports is valid in Linux or mac

		if (probs.getSendMarkersToEEG () != "none") {
			if (probs.getSendMarkersToEEG () == "parallel") {
				//170623 trecho para incluir a DLL para envio para o EEG, em Windows 32/64 bits
				//
				// antes saber se é Windows e se é 32 bits;
				// mas aqu não sabemos ainda se o JG serah escolhido...
				// se 64bits vem na descricao cf https://docs.unity3d.com/ScriptReference/SystemInfo-operatingSystem.html
				//
				//171017 agora vale para 32 e 64bits - 64bits nao testado
				//bool isWindows32bits = (Application.platform == RuntimePlatform.WindowsPlayer) && (! SystemInfo.operatingSystem.Contains ("64bit"));
				//if (isWindows32bits) {
				//
				//180104
				#if UNITY_STANDALONE_WIN  || UNITY_EDITOR_WIN
//				Debug.Log("Porta="+ probs.getPortSendData());
				definePortAccess (0xbd00);
//				definePortAccess (0x378);
//				pIn = probs.getPortSendData();
//				ushort pOut = Convert.ToUInt16(pIn);
//				definePortAccess (pOut);

				for (int j = 1; j < timeBetweenMarkers; j++) {j = j + 1;};  //170626 para dar um tempico entre envios à paralela
				Write (0x00);                         //170626 envio do marcador zero: INICIO DO JOGO
				#endif
			} else {
				if (probs.getSendMarkersToEEG() == "serial") {
					//180104 only for standalone desktops... not very sure...
					#if UNITY_STANDALONE || UNITY_EDITOR
					openSerialPort(probs.getPortEEGserial());  //collect port name from configurationFile
					if (diagSerial == 1) {
						//if (openSerialPort(probs.getPortEEGserial())) {  //collect port name from configurationFile
						data[0] = 0x00; sendDataToSerial(data);      //BrainProducts: set the port to an initial state
					}
					#endif
				}
			}
		}

*/
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

			//attentionPoint.GetComponentsInChildren<Image>()[0].color = probs.attentionColors (0);
			//attentionPoint.GetComponentsInChildren<Image>()[1].color = probs.attentionColors (1);
			//attentionPoint.GetComponentsInChildren<Image>()[2].color = probs.attentionColors (2);
		}
// Paulo Roberto pediu para tirar - 20190715		cruzLaranja.gameObject.SetActive(true); //troca a bola pela cruz - 20190702
// Paulo Roberto pediu para tirar - 20190715		cruzVerde.gameObject.SetActive(false);  //troca a bola pela cruz - 20190702
// Paulo Roberto pediu para tirar - 20190715		cruzPreta.gameObject.SetActive(false);  //troca a bola pela cruz - 20190702

	}

	//--------------------------------------------------------------------------------------------------------
	void Update ()
	{
		int currAnim = probs.GetCurrMachineIndex ();
		bool estouNoPegaQualquerTecla = false;  //170223170110 para aceitar qualquer tecla, inclusive as do jogo
		int number; //180419 to facilitate the routine
		float tempoJogo;

		float TimerControl = Time.time - StartTime;
		string mins = ((int)TimerControl/60).ToString("00");
		string segs = (TimerControl % 60).ToString("00");
		string milisegs = ((TimerControl * 100)%100).ToString ("00");
         
		string TimerString = string.Format ("{00}:{01}:{02}", mins, segs, milisegs);
         
		//cronos.GetComponent<Text>().text = TimerString.ToString ();
		
		tempoJogo = Time.realtimeSinceStartup - gameFlow.startSessionTime;

//		Debug.Log("@ALE:Time.realtimeSinceStartup = "+ Time.realtimeSinceStartup);
		//Debug.Log("movementTimeA = "+ movementTimeA);
		//Debug.Log("decisionTimeA = "+ decisionTimeA);
//		Debug.Log("@ALE:--------------------gameFlow.startSessionTime = "+ gameFlow.startSessionTime);
//		Debug.Log("@ALE:++++ tempoJogo = "+ tempoJogo);
//		Debug.Log("@ALE:++++ decisionTimeA = "+ decisionTimeA);


		//161226 nunca entra aqui por ser >= gkAnim...
		//170130 mas eh obrigatorio para acertar o gkAnim correto nos hashes, no caso de pular direto paa campo profissional
		if ((currAnim >= gkAnim.Length) || (gameFlow.jogarMDfase3 && ((PlayerPrefs.GetInt ("gameSelected") == 3) || (PlayerPrefs.GetInt ("gameSelected") == 5)))) {
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
//						StartCoroutine (WaitThenDoThings (3.2f)); //170417 era 3.4 mas havia uam latência extra; a animacao dura exatos 3s
						StartCoroutine (WaitThenDoThings (2.7f)); //170417 era 3.4 mas havia uam latência extra; a animacao dura exatos 3s
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
		float tempoJogo;

		yield return new WaitForSeconds(time);

		tempoJogo = Time.realtimeSinceStartup - gameFlow.startSessionTime;

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
				Debug.Log("@FINISHED:Time.realtimeSinceStartup = "+ Time.realtimeSinceStartup);
				//Debug.Log("movementTimeA = "+ movementTimeA);
				//Debug.Log("decisionTimeA = "+ decisionTimeA);
				Debug.Log("@FINISHED:--------------------gameFlow.startSessionTime = "+ gameFlow.startSessionTime);
				Debug.Log("@FINISHED:++++ tempoJogo = "+ tempoJogo);
				Debug.Log("@FINISHED:++++ decisionTimeA = "+ decisionTimeA);
				eLog.time = Time.realtimeSinceStartup - movementTimeA - gameFlow.otherPausesTime ;
				float tmpTime = eLog.time + decisionTimeA;
//				cronosOut.GetComponent<Text>().text = decisionTimeA.ToString();
				cronosOut.GetComponent<Text>().text = tmpTime.ToString();



/*				//180123 valid for all game modules
				if (probs.getSendMarkersToEEG () != "none") {
					//180123 change to routine to call in all other game modules
					sendStartMoveToSerial ();
				}
*/
			}
		}
	}

	//---------------------------------------------------------------------------------------
	//170623 rotinas que acessam a DLL de acesso à paralela (EEG)
	//       com base nos testes em C:\Users\HP\Documents\1.Neuromat\acessoParalela\Assets\Scripts
/*	#if UNITY_STANDALONE_WIN  || UNITY_EDITOR_WIN
	private ushort _PortAddress;
	public void definePortAccess(ushort PortAddress)
	{   //171017 considerar 32 e 64bits
		_PortAddress = PortAddress;
		uint nResult = 0;
		if (! SystemInfo.operatingSystem.Contains ("64bit")) {
			nResult = IsInpOutDriverOpen();
		} else {
			nResult = IsInpOutDriverOpen_x64();
		}

		if (nResult == 0) {
			throw new ArgumentException ("Unable to open inpOut32 or inpOutx64 DLL");
		} else {
			//dll.text = "Open inpOut32.dll";
			//Debug.Log("Aberta porta paralela com inpOut32! nresult = "+ nResult);  //apagar
		}
	}

	//---------------------------------------------------------------------------------------
	//170623 gravar dado na paralela
	public void Write(ushort Data)
	{   //171017 considerar 32 e 64bits
		if (!SystemInfo.operatingSystem.Contains ("64bit")) {
			Out32 (_PortAddress, Data);    //versao INDC/RJ EEG  w32
		} else {
			Out32_x64(_PortAddress, Data);
		}
	}
    #endif
*/

	//---------------------------------------------------------------------------------------
	//170906 botão Play/Pause clicado (no canto superior direito, ao lado do Exit)
	//170918
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

/*
	//---------------------------------------------------------------------------------------
	//180104 Open serialPort
	public void openSerialPort(string port)
	{
		if (serialp == null)
			serialp = new SerialPort(port);

		//em Programming Examples: http://www.brainproducts.com/downloads.php?kid=40
		//serial properties
		serialp.BaudRate = 9600;
		serialp.DataBits = 8;
		serialp.StopBits = StopBits.One;
		serialp.Parity = Parity.None;
		serialp.Handshake = Handshake.None;

		diagSerial = 1;
		try {
			serialp.Open ();
		}
		catch (Exception e)	{  //better more generic, than "IOException e"
			Debug.Log("Error opening serial port: " + e.Message);
			diagSerial = 2;
			//if (e.GetType().IsSubclassOf(typeof(Exception)))
			//	throw;
		}
		//return (diagSerial == 1) ? true : false ;
	}


	//---------------------------------------------------------------------------------------
	//180104 Close serialPort
	public void closeSerialPort()
	{
		if (serialp != null && diagSerial == 1) {     //if (serialp != null && serialp.IsOpen) : not ok if notOpen
			data[0] = 0xff; sendDataToSerial(data);   //BrainProducts: reset the port to its default state
			serialp.Close();
			serialp.Dispose();
		}
		serialp = null;
	}


	//---------------------------------------------------------------------------------------
	//180104 SendDataToSerial
	public void sendDataToSerial(byte[] packet)
	{
		//180129 brainProducts EEG connected to the triggerBox using USB
		//       needs time between sended followed markers: not more! converted 3 markers in 1, using 18 different markers, changing results by stimulus!

		//data[0] = 0x00; serialp.Write(data, 0, packet.Length);;   //marker 0: Set the port to an initial state; Bazán: "not necessary"
		serialp.Write(packet, 0, packet.Length);
	}


	//---------------------------------------------------------------------------------------
	//180123 start a move
	public void sendStartMoveToSerial()
	{
		if (probs.getSendMarkersToEEG() == "parallel") {																																							
			#if UNITY_STANDALONE_WIN  || UNITY_EDITOR_WIN
			//170626 se eh para enviar marcador para o EEG
			Write(0x01);        //marcador um: INICIO DE JOGADA/MOVIMENTO (apos mostrar setas de direcao para selecionar)
			for (int j = 1; j < timeBetweenMarkers; j++) { j = j + 1; };  //170626 para dar um tempico entre envios à paralela
			Write(0x00);        //170626 envio do marcador zero após INICIO De JOGADA
			#endif
		} else {
			//180104 only for standalone desktops... not very sure...
			if (probs.getSendMarkersToEEG() == "serial") {
				#if UNITY_STANDALONE || UNITY_EDITOR
				data[0] = 0x01; sendDataToSerial(data);   //marker 1: START A MOVE
				//data[0] = 0x00; sendDataToSerial(data);   //marker 0: Set the port to an initial state
				#endif
			}
		}
	}
*/		
}
