/**************************************************************************************/
//  Module written by scaroni <renato.scaroni@gmail.com>
//  Rewrited by Josi Perez <josiperez.neuromat@gmail.com>, keeping the original code in comment
//
/**************************************************************************************/
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;  //170102 List
using JsonFx.Json;


//---------------------------------------------------------------------------------------
public class GameFlowManager : MonoBehaviour
{
    private float StartTime;
//    public GameObject cronosIn;
//    public GameObject cronosOut;
    
    public GameObject game;
    public CanvasGroup gameCanvas;
    public GameObject betweenLevels;
    public GameObject intro;
    public GameObject introMenu;
    public GameObject gameModeMenu;
    public ScoreMonitor scoreMonitor;

    public GameObject logBox;         //Josi: JG: box com as 8 jogadas mainScene/gameScene/gameUICanvas/LogBox
    public GameObject bmMsg;          //Josi: BM: tutorial ou "aperte tecla" mainScene/gameScene/gameUICanvas/bmMsg
    public GameObject aperteTecla;    //Josi: 161229: reuniao: sai tutorial, mas no BMcomTempo entra aviso de AperteTecla para 3-2-1

    //	public GameObject mdTutorial;     //Josi: MD: tutorial do memoDecl - mainScene/gameScene/gameUICanvas/mdTutorial: Reuniao pede para eliminar
    //	public GameObject progressionBar; //Josi: 161227: reuniao pede para eliminar a menos do Jogo do Goleiro
    public GameObject frameChute;     //Josi: 161229: reuniao: contorno que recebe a indicacao da seta de direcao mainScene/gameScene/gameUICanvas/bmIndicaChute

    public GameObject mdFirstScreen;  //170102: reuniao: primeira tela do MD (ou Base Memoria)
                                      //	public GameObject ExitFirstScreenJM;     //170309 mainScene/gameScene/gameUIcanvas/mdFirstScreen/ExitFirstScreenJM botao de exit
    public GameObject mdAperteTecla;  //170912: para poder alterar a transparência enquanto está em modo Pause (não encontrei sintaxe sem declarar)

    public GameObject errorMessages;         //170311 em configuration/canvas/errorMessages; QG para apontar se o param ID estah repetido...
    public Text txtMessage;                  //170623 txt do erro
    private bool waitingKeyToExit = false;   //170311 para sair apos encontrar erro nos confFiles

    public float startSessionTime;           //170316 inicio da sessao: selecionar jogo (para comparativo entre os tempos de decsao/movimento)
    public float startRelaxTime;             //170316 para descontar o tempo na tela de relax
    public float endRelaxTime;               //170316 para descontar o tempo na tela de relax
    public float totalRelaxTime;             //170317 pode haver mais do que uma parada


    //no gameFlow para facilitar o ServerOperations que tem o gameFlow na mãogameLover
    public float initialPauseTime;           //170912 tempo de pausa no início do jogo
    public int numOtherPauses;               //170912 contar o num de paradas (de Play para Pause) excepto a inicial
    public float otherPausesTime;            //170912 totalizar as pausas em uma jogada
    public float startOtherPausesTime;       //170912 aux
    public float otherPausesTotalTime;       //170919 totalizar todas as pausas de um jogo

    public GameObject bmGameOver;            //170925 mainScene/GameUICanvas/bmgameOver
    public bool waitingKeyGameOver = false;  //170925 para receber a tecla para voltar ao menu
    public GameObject bmGameLover;            //180321 mainScene/GameUICanvas/bmgameLover
    public bool waitingKeyGameLover = false;  //180321 para receber a tecla para voltar ao menu

    public int minHitsInSequence;            //170921 AQ/AR opcao 2: garantir um num de acertos seguidos em no max bmLimitPlays
                                             //             opcao 1: existente: min de MinHits certos
                                             //180320 JG/JM: player needs to hit asymptote plays to end the phase module
    public bool useTimer = false;            //Josi era assim: public bool useTimer;
    public bool firstScreen = false;         //170102 saber que ainda nao chegou na fase profissional (unica) do MD
    public bool jaPasseiPorFirstScreen = false;  //170103 MD tem duas fases: tela inicial de memorizacao e JG no nivel 3
    public bool jogarMDfase3 = false;        //170104 passou por firstScreen, agora jogar MD fase 3 direto, com a mesma sequ repetida 3x
    public int playsToRelax = 0;             //170223 num jogadas para dar um descanso


    public GameObject userInfoForm;
    public GameObject quitGameMenu;         //MainScene/gameScene/giveUpMenu
    public Text txtAbandon;                 //MainScene... GiveUpMenu
    public int playLimit = 0;

    public BetweenLevelsController btLevelsController; //Josi 161214 erro em betweenLevels.GetComponent<BetweenLevelsController>().PostEnd/Middle/EndGame

    private ProbCalculator probCalculator;
    private UIManager uiManager;
    //	private bool barCalculated = false;     //170106 sem barra de progresso

    private bool onVersusMode = false;

    public Text placarFinal;
    public Button nextLevel;  //MainScene/GameScene/BetweenLevelsCanvas/Panel/NextLevel 
    public Button thisLevel;  //MainScene/GameScene/BetweenLevelsCanvas/Panel/ThisLevel = exit
    public Button endLevel;   //MainScene/GameScene/BetweenLevelsCanvas/Panel/EndLevel = menu de jogos (goToIntro)
    public Button notAbandon; //MainScene/GameScene/GiveUpMenu/Nao
    public Button yesAbandon; //MainScene/GameScene/GiveUpMenu/Sim

    public Button menuPrizes;   //180605 
    public Button menuTutorial; //180605 old: 4 field with a little text
    public Button menuCredits;  //180605
    public Button menuAbout;    //180605
    public GameObject bkgPrizes;//180706

    //170322 para saber se o Exit foi clicado (para resolver as telas onde há o "aperte alguma tecla" e o click no Exit)
    public Button exitIcon;   //MainScene/GameScene/Exit; botao para, ao clicar, enviar para uiManager.QuitGame

    //170303 menu dinamico: descrito no JSon e montado aqui no Start()
    public GridLayoutGroup menuJogos;     //170302 mainScene/IntroScene(1)/Canvas/LogBox/MenuInicio/menuJogos - grid 1 coluna para receber os botoes dinamicos
    public GameObject btnPrefab;          //170302 para o menu dinâmico; estrategia copiada do menu de pacotes dinamicos em loadStages
                                          //       em Project/Prefabs/gameMenuBtn, com texto tamanho 30 (o dos pacotes está 20) - nao acertei a sintaxe para alterar por pograma
    public GridLayoutGroup menuIcons;     //180521 mainScene/IntroScene(1)/Canvas/LogBox/MenuInicio/menuIcons - grid to receive the associated icon
    public GameObject[] menuIconList;


    public GameObject relaxTime;          //170818 estava no UIManager; 170222 aviso para dar um tempo de descanso ao jogador                                         

    public Text jogoSelecionado;          //170303 na tela de pegaDados, mostrar o jogo selecionado e acrescentado o botão menu (nao tinha saida antes)
    public Text obrigaAlias;              //170303 se user seleciona voltar ao Menu, msg fica com "obrigatorio preencher apelido"

    public GameObject scrTutorial;        //180626 temporarily screen gameTutorial; will be changed for an Magara art
                                          //180626 TMP are called as gameObjects
    private LocalizationManager translate;    //171006 trazer script das rotinas de translation
    public GameObject txtTut1;                //171006 elementos para traduzir na tela de Menu
    public GameObject txtTut2;
    public GameObject txtTut3;
    public GameObject txtTut4;
	//public GameObject txtTut5;
    public Text txtTut5;
    public Text txtJogo;
    public Text txtMenu;
    public Text txtSair;
    public Text txtHeader;                   //171009 errMsgs
    public Text txtExit;
    public Text txtTeam;                     //180614 back to Team Selection
    public Text txtStartG;                   //180629 start game
    public Text txtComP;                     //180629 com pausa
    public Text txtSemP;                     //180629 sem pausa

    int errorNumber;                         //180105 to create a function
    public string sequJMGiven;               //180418 to save the sequence given to the player in JM


    //-------------------------------------------------------------------------------------
    static private GameFlowManager _instance;
    static public GameFlowManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("GameFlowManager").GetComponent<GameFlowManager>();
            }
            return _instance;
        }
    }

    //-------------------------------------------------------------------------------------
    void OnEnable()
    {
        UIManager.OnAnimationEnded += OnAnimationEnded;
        UIManager.OnAnimationStarted += OnAnimationStarted;
    }

    //-------------------------------------------------------------------------------------
    void OnDisable()
    {
        UIManager.OnAnimationEnded -= OnAnimationEnded;
        UIManager.OnAnimationStarted -= OnAnimationStarted;
    }


    //-------------------------------------------------------------------------------------
    void OnAnimationStarted()
    {
        //-------------------------------------------------------
        //float TimerControl = Time.time - StartTime;
        //string mins = ((int)TimerControl/60).ToString("00");
        //string segs = (TimerControl % 60).ToString("00");
        //string milisegs = ((TimerControl * 100)%100).ToString ("00");
         
        //string TimerString = string.Format ("{00}:{01}:{02}", mins, segs, milisegs);
         
        //cronosIn.GetComponent<Text>().text = TimerString.ToString ();
        //Debug.Log("Iniciou a animação"+TimerString.ToString ());

        //-------------------------------------------------------
        //Josi: 161212: independente do jogo, deixar so animations
        uiManager.btnsAndQuestion.SetActive(false);
        frameChute.SetActive(false);             //170112 sobra no JG
        uiManager.setaEsq.SetActive(false);
        uiManager.setaCen.SetActive(false);
        uiManager.setaDir.SetActive(false);
    }


    //-------------------------------------------------------------------------------------
    void OnAnimationEnded()
    {
        if (!onVersusMode)
        {
            scoreMonitor.UpdateMenu();

            //161212: verificar os limites de jogadas em cada jogo
            //170216: JG pode estar configurado para conter uma phase0 (experimental, sem historico) - o novo param informa este estado
            int numPlays = probCalculator.GetCurrentPlayLimit(PlayerPrefs.GetInt("gameSelected"));  //BM ou JG


            //170222 pausa para descanso
            if (playsToRelax > 0)
            {                         //maioria do povo nao vai ativar este recurso, entao, sai fora de cara
                                      //se atingiu o num de jogadas para descanso e nao eh o fim (N jogadas e N playsToRelax) e nao eh o Aquecto com tempo, ativar descanso
                if ((uiManager.eventCount >= playsToRelax) && (playsToRelax != numPlays) && (PlayerPrefs.GetInt("gameSelected") != 4))
                {
                    startRelaxTime = Time.realtimeSinceStartup;     //170316 inicia tempo de relax

                    relaxTime.SetActive(true);
                    uiManager.aguardandoTeclaPosRelax = true;

                    //170412 
                    //supor 20 numPlays e playsToRelax 5: precisa parar em 5, 10 e 15 jogadas - não em 20...
                    playsToRelax = playsToRelax + probCalculator.getPlaysToStartRelax();
                }
            }

            //----------------------------------------------------------------------------------
            if (numPlays > 0)
            {
                //170306 IMEjr nao gostou da estrategia de soh aumentar ao final do totalDeJogadas; como eram 5 x 1 mudei...
                //       assim pode nao ficar claro que o aumento ocorreu porque ao fim das jogadas planejadas o min nao foi atingido

                //180320 Profa MElisa acrescenta o conceito de "assíntota" no JG: o número de jogadas que o player deve acertar
                //       em sequ para concluir que "adivinhou" o padrão; se "asymptotePlays" = 0, implica árvore probabilística
                //       onde não é possível determinar uma sequência, ou que o JG deve continuar independentemente de acertar/errar
                if (PlayerPrefs.GetInt("gameSelected") == 2)
                {
                    //180628 gameOver comes only if numHitsInSequence is defined (assíntota)
                    if ((uiManager.eventCount >= numPlays) && (probCalculator.getJGminHitsInSequence() > 0))  //180622
                    {    //180402 extremes (>, not >=)
                        gameOver(2);
                    }
                    else
                    {
                        if (probCalculator.getJGminHitsInSequence() > 0)
                        { //if assymptote zero, the game advance
                            if (probCalculator.getJGminHitsInSequence() == minHitsInSequence)
                            {  //got the assymptote
                                gameLover(2);
                            }
                        }
                    }
                }


                //170125 nos Base Motora, se nao atingido o num minimo de jogadas, aumentar as jogadas
                //170126                  e posicionar proximo chute + atualizar placar
                //170921 ver opcao de jogo: ou por numMinAcertos na jogada 
                //                          ou obrigar minHitsInSequence dentro de um maxPlays
                if ((PlayerPrefs.GetInt("gameSelected") == 1) || (PlayerPrefs.GetInt("gameSelected") == 4))
                {
                    //---------------------------------------------------
                    //haverah diferencas entre as duas ops, tipo "pára em bmLimitPlays tambem se op2 ou aqui deixa sair por hits"?
                    //melhor deixar dois blocos repetidos...
                    if (probCalculator.getMinHitsInSequence() > 0)
                    {    //op1: numMin de acertos em sequencia
                         //180323 if hits minHitsInSequence exit, win the game!
                        if (probCalculator.getMinHitsInSequence() == minHitsInSequence)
                        {
                            gameLover(PlayerPrefs.GetInt("gameSelected"));
                        }
                        else
                        {
                            int i = probCalculator.getMinHitsInSequence() - minHitsInSequence;   //qtos hits faltam
                            int j = numPlays - uiManager.eventCount;                              //qtas jogadas faltam
                            if (i > j)
                            {
                                //se faltam mais jogadas para completar os hits necessarios...
                                if ((probCalculator.getBmMaxPlays() - uiManager.eventCount + minHitsInSequence) >= probCalculator.getMinHitsInSequence())
                                { //e isso nao ultrapasse o limite...
                                    probCalculator.sumNumPlays(1);            //adicionar uma jogada
                                    probCalculator.defineBMSequ(false, 1);    //e gera um novo simbolo de cada vez

                                    probCalculator.currentBMSequIndex = probCalculator.GetCurrentPlayLimit(PlayerPrefs.GetInt("gameSelected")) - i;
                                    uiManager.updateScore(PlayerPrefs.GetInt("gameSelected"));

                                    numPlays = probCalculator.GetCurrentPlayLimit(PlayerPrefs.GetInt("gameSelected"));  //BM ou JG

                                    //170203 quando entra na validacao do minHits (trecho acima), nao vai para a WaitThenDoThings
                                    //       mas vai quando entra na rotina e eventCount < numPlays...
                                    uiManager.animCountDown = false;
                                    uiManager.animResult = true;
                                    StartCoroutine(uiManager.WaitThenDoThings(probCalculator.animationTime())); //170307 param3: apos esperar vai para betweenLevels
                                }
                                else
                                {
                                    gameOver(PlayerPrefs.GetInt("gameSelected"));
                                }
                            }
                        }

                    }
                    else
                    {//op2: obrigar numMinAcertos
                     //---------------------------------------------------
                        if (probCalculator.getMinHits() == uiManager.success)
                        {
                            gameLover(PlayerPrefs.GetInt("gameSelected"));
                        }
                        else
                        {
                            int i = probCalculator.getMinHits() - uiManager.success;  //jogadas que falta acertar
                            int j = numPlays - uiManager.eventCount;                   //jogadas que faltam para encerrar a fase
                            if (i > j)
                            {
                                //se faltam mais jogadas para completar os hits necessarios...
                                if ((probCalculator.getBmMaxPlays() - uiManager.eventCount + uiManager.success) >= probCalculator.getMinHits())
                                {  //180309
                                    probCalculator.sumNumPlays(1);            //170306 agora soma de um em um
                                    probCalculator.defineBMSequ(false, 1);    //       e gera um novo simbolo de cada vez

                                    //170216 o 2 param vale apenas se gameSelected =2
                                    probCalculator.currentBMSequIndex = probCalculator.GetCurrentPlayLimit(PlayerPrefs.GetInt("gameSelected")) - i;
                                    uiManager.updateScore(PlayerPrefs.GetInt("gameSelected"));

                                    //170309 falta atualizar numPlays, alterado nas linhas acima
                                    numPlays = probCalculator.GetCurrentPlayLimit(PlayerPrefs.GetInt("gameSelected"));  //BM ou JG

                                    //170203 quando entra na validacao do minHits (trecho acima), nao vai para a WaitThenDoThings
                                    //       mas vai quando entra na rotina e eventCount < numPlays...
                                    uiManager.animCountDown = false;
                                    uiManager.animResult = true;
                                    StartCoroutine(uiManager.WaitThenDoThings(probCalculator.animationTime())); //170307 param3: apos esperar vai para betweenLevels
                                }
                                else
                                {
                                    gameOver(PlayerPrefs.GetInt("gameSelected"));
                                }
                            }
                        }
                    }
                    //---------------------------------------------------
                }
                else
                {
                    //180312 limit maxPlays in JM by the played plays
                    if (PlayerPrefs.GetInt("gameSelected") == 5 && jogarMDfase3 && playing)
                    {
                        if (uiManager.eventCount >= probCalculator.getMDMaxPlays())
                        {   //180402 extremes (>, not >=)

                            if (probCalculator.getMDminHitsInSequence() > 0)
                            {
                                if (probCalculator.getMDminHitsInSequence() == minHitsInSequence)
                                {
                                    gameLover(5);
                                }
                                else
                                {
                                    gameOver(5);
                                }
                            }
                            else
                            {
                                //180327 when minHitsInSequ=0, old strategy: 
                                //       if the player hits 12 plays, but not necessarily in sequence, it is ok
                                //12 is hit 3x the sequence of 4 symbols
                                if (uiManager.success >= 12)
                                {
                                    gameLover(5);
                                }
                                else
                                {
                                    gameOver(5);
                                }
                            }
                        }
                        else
                        {
                            if (probCalculator.getMDminHitsInSequence() > 0)
                            { //if assymptote zero, the game advance
                                if (probCalculator.getMDminHitsInSequence() == minHitsInSequence)
                                {  //got the assymptote
                                    gameLover(5);
                                }
                            }
                        }
                    }
                }
                //-------------------------------------------------------
                //float TimerControl = Time.time - StartTime;
                //string mins = ((int)TimerControl/60).ToString("00");
                //string segs = (TimerControl % 60).ToString("00");
                //string milisegs = ((TimerControl * 100)%100).ToString ("00");
         
                //string TimerString = string.Format ("{00}:{01}:{02}", mins, segs, milisegs);
                //cronosOut.GetComponent<Text>().text = TimerString.ToString ();
                //Debug.Log("Terminou a animação"+TimerString.ToString ());
                //-------------------------------------------------------

                //if(playing && uiManager.events.Count >= probCalculator.GetCurrentPlayLimit())    //Josi: era assim  
                //				if (playing && uiManager.events.Count >= numPlays) {   //170106 events contem o log,que no caso do MD acumula os testes iniciais
                //if (playing && (uiManager.eventCount >= numPlays)) {   //       eventCount contem o numero de jogadas de uma fase
                if (playing && (uiManager.eventCount >= numPlays) && (PlayerPrefs.GetInt("gameSelected") != 5))
                { //
                    uiManager.BtwnLvls = true;

                    //170320 wait dependendo do tipo de animacao
                    //170322 criada uma rotina para devolver o tempo das animacoes - pelo menos se concentra em um lugar
                    StartCoroutine(waitTime(PlayerPrefs.GetInt("gameSelected"), probCalculator.animationTime(), 1)); //170307 param3: apos esperar vai para betweenLevels
                }
            }
        }
    }


    //----------------------------------------------------------------------------------------------------
    //180312 to avoid repeat this code
    public void gameOver(int game)
    {
        //wait defended/ended animation to end before to shows up the gameOver screen
        if (probCalculator.getCurrentAnimationType() == "short")
        { //short visual and sound anim
            StartCoroutine(waitTime(game, 1.4f, 3));              //short
        }
        else
        {
            if (probCalculator.getCurrentAnimationType() == "long")
            { //long visual and sound anim
                StartCoroutine(waitTime(game, 2.5f, 3));             //long
            }
            else
            {
                StartCoroutine(waitTime(game, 0.3f, 3));             //none: without anim
            }
        }
        waitingKeyGameOver = true;
        playing = false;

        //180410 if parametrized, show "attention point" in middle screen
        if (probCalculator.attentionPointActive())
        {
            uiManager.attentionPoint.SetActive(false);         //on Inspector first image is green (0), second is red (1)       
        }
    }



    //----------------------------------------------------------------------------------------------------
    //180321 to avoid repeat this code
    public void gameLover(int game)
    {
        //wait defended/ended animation to end before to shows up the gameOver screen
        if (probCalculator.getCurrentAnimationType() == "short")
        { //short visual and sound anim
            StartCoroutine(waitTime(game, 1.4f, 4));              //short
        }
        else
        {
            if (probCalculator.getCurrentAnimationType() == "long")
            { //long visual and sound anim
                StartCoroutine(waitTime(game, 2.5f, 4));             //long
            }
            else
            {
                StartCoroutine(waitTime(game, 0.3f, 4));             //none: without anim
            }
        }
        waitingKeyGameLover = true;
        playing = false;

        //180410 if parametrized, show "attention point" in middle screen
        if (probCalculator.attentionPointActive())
        {
            uiManager.attentionPoint.SetActive(false);         //on Inspector first image is green (0), second is red (1)       
        }
    }


    //------------------------------------------------------------------------------------------------------
    //170205 esperar terminar a animacao da ultima jogada para aparecer a tela de betweenLevels (1)
    //170307 ou a de giveUP (2) - virou public para chamar no UImanager.QuitGame
    public IEnumerator waitTime(int gameSelected, float time, int whatScreen)
    {
        yield return new WaitForSeconds(time);
        
        if (whatScreen == 1)
        {
            ShowInBetween(gameSelected);
        }
        else
        {
            //170327 ao clicar no Exit após waitTime de fim de animacao, deve ir para a tela de abandonar s/n
            if (whatScreen == 2)
            {
                quitGameMenu.SetActive(true);

            }
            else
            {
                if (whatScreen == 3)
                {
                    //170928 diferença de texto para Android
                    //       lembrar que replace nao substitui a string inplace
                    //171006 translation
                    //171122 iOS (iPad/iPhone)
                    if ((Application.platform == RuntimePlatform.Android) ||
                        (Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad")))
                    {
                        bmGameOver.GetComponentInChildren<Text>().text = translate.getLocalizedValue("toqueBmGameOver").Replace("\\n", "\n");
                    }
                    else
                    {
                        bmGameOver.GetComponentInChildren<Text>().text = translate.getLocalizedValue("aperteBmGameOver").Replace("\\n", "\n");
                    }
                    bmGameOver.SetActive(true);
                }
                else
                {  //180321 gameLover: congratulations: reach the assymptote before the total plays ------
                    if (whatScreen == 4)
                    {
                        if ((Application.platform == RuntimePlatform.Android) ||
                            (Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad")))
                        {
                            bmGameLover.GetComponentInChildren<Text>().text = translate.getLocalizedValue("toqueBmGameLover").Replace("\\n", "\n");
                        }
                        else
                        {
                            bmGameLover.GetComponentInChildren<Text>().text = translate.getLocalizedValue("aperteBmGameLover").Replace("\\n", "\n");
                        }
                        bmGameLover.SetActive(true);
                    } //-----------------------------------------------------------------------------------------
                }
            }
        }
    }


    void Start()
    {
        probCalculator = ProbCalculator.instance;
        uiManager = UIManager.instance;

        intro.SetActive(true);
        introMenu.SetActive(true);

        gameModeMenu.SetActive(false);
        betweenLevels.SetActive(false);
        gameCanvas.interactable = false;
        game.SetActive(false);
        quitGameMenu.SetActive(false);
        bmGameOver.SetActive(false);    //170925 start without gameOver
        bmGameLover.SetActive(false);    //180321 start without gameLover


        //171006 declarar a instance para permitir chamar rotinas do outro script
        translate = LocalizationManager.instance;

        //171006 trocar os textos
        //180626 manter a tela de tutorial com as 4 imagens/texto ate que venha uma sugestão do designer
        //txtTut1.text = translate.getLocalizedValue("tut1").Replace("\\n", "\n");  //@@ SE APROVADO APAGAR
        //txtTut2.text = translate.getLocalizedValue("tut2").Replace("\\n", "\n");  //@@ SE APROVADO APAGAR
        //txtTut3.text = translate.getLocalizedValue("tut3").Replace("\\n", "\n");  //@@ SE APROVADO APAGAR
        //txtTut4.text = translate.getLocalizedValue("tut4").Replace("\\n", "\n");  //@@ SE APROVADO APAGAR
        txtTut5.text = translate.getLocalizedValue("tut5").Replace("\\n", "\n");  //@@ SE APROVADO APAGAR
        //180627 from UiText to TMPro
        txtTut1.GetComponentInChildren<TMPro.TMP_Text>().text = translate.getLocalizedValue("tut1").Replace("\\n", "\n");
        txtTut2.GetComponentInChildren<TMPro.TMP_Text>().text = translate.getLocalizedValue("tut2").Replace("\\n", "\n");
        txtTut3.GetComponentInChildren<TMPro.TMP_Text>().text = translate.getLocalizedValue("tut3").Replace("\\n", "\n");
        txtTut4.GetComponentInChildren<TMPro.TMP_Text>().text = translate.getLocalizedValue("tut4").Replace("\\n", "\n");
		//txtTut5.GetComponentInChildren<TMPro.TMP_Text>().text = translate.getLocalizedValue("tut5").Replace("\\n", "\n");

        txtJogo.text = translate.getLocalizedValue("jogo");
        txtMenu.text = PlayerPrefs.GetString("teamSelected") + " : " + translate.getLocalizedValue("menu");
        txtSair.text = translate.getLocalizedValue("sair1");
        txtTeam.text = translate.getLocalizedValue("bckTeams");

        txtStartG.text = translate.getLocalizedValue("iniciaJ").Replace("\\n", "\n");  //180629 start game
        txtComP.text = translate.getLocalizedValue("comP");                            //180629 com pausa
        txtSemP.text = translate.getLocalizedValue("semP");                            //180629 sem pausa

    //180612 new buttons
    menuAbout.GetComponentInChildren<Text>().text = translate.getLocalizedValue("sobre");       //.Replace("\\n", "\n");
    //@@menuCredits.GetComponentInChildren<Text>().text = translate.getLocalizedValue("creditos");  //.Replace("\\n", "\n");
    menuPrizes.GetComponentInChildren<Text>().text = translate.getLocalizedValue("premios");    //.Replace("\\n", "\n");
    menuTutorial.GetComponentInChildren<Text>().text = translate.getLocalizedValue("tutor");    //.Replace("\\n", "\n");

        //170311 validar arq conf ======================================
        errorNumber = probCalculator.configValidation();
        // if (errorNumber != 0 || uiManager.diagSerial == 2)
        // { //180105 besides configvalidation, test if serial open in a defined port
        //
        //     //171009 translate frases de erro
        //     //171122 iOS (iPad/iPhone) + change order to avoid negatives
        //     txtHeader.text = translate.getLocalizedValue("errHeader");
        //     if ((Application.platform == RuntimePlatform.Android) ||
        //         (Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad")))
        //     {
        //         txtExit.text = translate.getLocalizedValue("toqueErrExit");
        //     }
        //     else
        //     {
        //         txtExit.text = translate.getLocalizedValue("aperteErrExit");
        //     }
        //
        //     errorMessages.SetActive(true);
        //     txtMessage.text = string.Empty;
        //     //---
        //     //180105
        //     if (errorNumber - 64 >= 0)
        //     {
        //         //txtMessage.text = "O parâmetro 'sendMarkersToEEG' aceita apenas os valores serial, parallel ou none)";
        //         showErrorMessage("err05", 64);
        //     }
        //     //---
        //     //180105
        //     if (errorNumber - 32 >= 0 || uiManager.diagSerial == 2)
        //     {
        //         //txtMessage.text = "'sendMarkersToEEG' indica envio pela serial, mas falta indicar a porta em 'portEEGserial'";
        //         showErrorMessage("err06", 32);
        //     }
        //     //---
        //     if (errorNumber - 16 >= 0)
        //     {
        //         //txtMessage.text = "O parâmetro 'menus' está inexistente ou inválido (falta associar o primeiro item de menu ou este aparece mais de uma vez)";
        //         showErrorMessage("err04", 16);
        //     }
        //     //---
        //     if (errorNumber - 8 >= 0)
        //     {
        //         //txtMessage.text = "- Nos arquivos de configuração, o parâmetro ID está com o mesmo nome em fases diferentes - o ID deve ser único em cada um deles.";
        //         showErrorMessage("err01", 8);
        //     }
        //     //---
        //     if (errorNumber - 4 >= 0)
        //     {
        //         //txtMessage.text = "- Faltam parâmetros de configuração: executável do Jogo incompatível com a definição dos times."; 
        //         showErrorMessage("err02", 4);
        //     }
        //     //---
        //    // if (errorNumber - 2 >= 0)
        //    // {
        //    //     //txtMessage.text = "- O envio de marcadores ao EEG através da porta paralela só está válido para ambientes Windows 32bits (parâmetro sendMarkersToEEG)."; 
        //    //     showErrorMessage("err03", 2);
        //    // }
        //     waitingKeyToExit = true;  //aparece o quadro de erros e aguarda tecla para sair;
        // }

        //=============================================================


        //Josi; onClick nao funciona no betweenLevels; ideia em https://docs.unity3d.com/ScriptReference/UI.Button-onClick.html
        Button btnNextLevel = nextLevel.GetComponent<Button>();
        btnNextLevel.onClick.AddListener(NextLevel);
        //--
        //180628 changed by Exit Icon
        //Button btnThisLevel = thisLevel.GetComponent<Button>();
        //btnThisLevel.onClick.AddListener(Sair);
        //--
        Button btnEndLevel = endLevel.GetComponent<Button>();      //Josi: 161212: ao haver mais jogos, 
        btnEndLevel.onClick.AddListener(GoToIntro);                //              terminar os niveis deve levar ao menu principal

        //Josi; onClick nao funciona no betweenLevels; ideia em https://docs.unity3d.com/ScriptReference/UI.Button-onClick.html
        Button btnNotAbandon = notAbandon.GetComponent<Button>();
        btnNotAbandon.onClick.AddListener(keepOnGame);
        Button btnYesAbandon = yesAbandon.GetComponent<Button>();

        //170307 necessaria uma funcao para esperar terminar as animacoes defendeu/perdeu
        //btnYesAbandon.onClick.AddListener(GoToIntro);
        btnYesAbandon.onClick.AddListener(abandonGame);

        //170322 cuidar no script do Exit (para resolver as telas onde há "aperte tecla" e botao Exit)
        //180628 changed button Sair by Exit Icon at upper right
        Button btnExit = exitIcon.GetComponent<Button>();
        //btnExit.onClick.AddListener(uiManager.QuitGame(2));  nao funciona esta sintaxe se há parâmetros
        btnExit.onClick.AddListener(() => uiManager.QuitGame(2));

        //180605 new buttons on old tutorial screen (together menu)
        Button btnTutorial = menuTutorial.GetComponent<Button>();
        btnTutorial.onClick.AddListener(showTutorial);
        Button btnCredits = menuCredits.GetComponent<Button>();
        btnCredits.onClick.AddListener(showCredits);
        Button btnAbout = menuAbout.GetComponent<Button>();
        btnAbout.onClick.AddListener(showAbout);

        //================
        //170302 definir MENU DE JOGOS com base no primeiro arquivo de configuracao
        //170303 fixar machines[0] para o menu: somente estah arquivado aqui (depois de implementar a leitura do param no 1o arq conf)
        //170303 *static members* don't belong to a specific instance, the variable only exists once and is shared between all instances of that class

        //171006 reconhecer o locale para decidir sobre os titulos dos jogos no menu
        string locale = translate.getLocalizedValue("locale");
        string gameN;

        //171130 refactoring the cellSizes to adapt to iPad (strategy found on Internet)
        menuJogos.GetComponent<RectTransform>().localScale = Vector3.one;              //let the parent with initial size (1,1,1); the "jump of the cat"
        float xCellWidth = menuJogos.GetComponent<RectTransform>().rect.width / 1.1f;  //not occupy all cell
        float xCellHeight = menuJogos.GetComponent<RectTransform>().rect.height;
        xCellHeight = xCellHeight / ProbCalculator.machines[0].menuList.Count;  //divide the space between the itens in menu
        xCellHeight -= menuJogos.spacing.x;
        menuJogos.cellSize = new Vector2(xCellWidth, xCellHeight);

        //180525 to keep same sizes between both grids
        menuIcons.cellSize = new Vector2(menuIcons.cellSize.x, xCellHeight);


        for (int j = 1; j < ProbCalculator.machines[0].menuList.Count; j++)
        {
            for (int i = 0; i < ProbCalculator.machines[0].menuList.Count; i++)
            {
                if (ProbCalculator.machines[0].menuList[i].sequMenu == j)
                {
                    GameObject go = Instantiate(btnPrefab);
                    go.transform.SetParent(menuJogos.transform);

                    //171006 se idioma pt_br fica o title do param menu, dado que o user pode alterar,
                    //       senao, pegar o texto do arquivo de locale
                    //171113 title sempre coletado do arquivo de locale
                    gameN = "game" + ProbCalculator.machines[0].menuList[i].game;
                    go.GetComponentInChildren<Text>().text = translate.getLocalizedValue(gameN).Replace("\\n", "\n");
                    go.name = translate.getLocalizedValue(gameN);
                    go.GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 220); //white without alpha
                                                                                               //go.GetComponentInChildren<Text> ().fontSize = 10;
                    go.GetComponentInChildren<Text>().resizeTextForBestFit = true;

                    //180523 Insert menu icon into the gameMenu: AQ 1;JG 2;AR 4;JM 5 (3 ficou sem uso)
                    GameObject menuIcon = Instantiate(menuIconList[ProbCalculator.machines[0].menuList[i].game - 1]);
                    menuIcon.transform.SetParent(menuIcons.transform);
                    menuIcon.transform.localScale = Vector3.one;


                    //171130
                    go.GetComponent<RectTransform>().localScale = Vector3.one;

                    Button b = go.GetComponent<Button>();

                    //170203 b.onClick.AddListener(delegate{gameFlow.ShowGameModeMenu( machines [currentStateMachineIndex].menuList [i].game );});
                    int temp = ProbCalculator.machines[0].menuList[i].game;
                    b.onClick.AddListener(() => ShowGameModeMenu(temp));

                    //180529 accept onClick also on menuIcons
                    Button iconMenuButton = menuIcon.GetComponent<Button>();
                    iconMenuButton.onClick.AddListener(() => ShowGameModeMenu(temp));

                    i = ProbCalculator.machines[0].menuList.Count;
                }
            }
        }


        //================

        //170818 definir texto da mensagem dependendo de ambiente;
        //       para o AR (aquecto com tempo) e para a tela de relax  
        //171122 iOS (iPad/iPhone)
        if ((Application.platform == RuntimePlatform.Android) ||
            (Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad")))
        {
            //171006 trocar pelos translate
            //aperteTecla.GetComponent<Text>().text = "Toque na tela\npara iniciar!";
            aperteTecla.GetComponent<Text>().text = translate.getLocalizedValue("toqueIniciar").Replace("\\n", "\n"); ;
            //relaxTime.GetComponentInChildren<Text>().text = "Intervalo no Jogo do Goleiro!\nPausa para descanso!\n\n\nToque na tela\npara continuar!";
            relaxTime.GetComponentInChildren<Text>().text = translate.getLocalizedValue("toqueRelax").Replace("\\n", "\n"); ;
        }
        else
        {
            //171006
            //aperteTecla.GetComponent<Text>().text = "Aperte uma tecla\npara iniciar!";
            aperteTecla.GetComponent<Text>().text = translate.getLocalizedValue("aperteIniciar").Replace("\\n", "\n");
            //relaxTime.GetComponentInChildren<Text>().text = "Intervalo no Jogo do Goleiro!\nPausa para descanso!\n\n\nAperte a barra de espaços\npara continuar!";
            relaxTime.GetComponentInChildren<Text>().text = translate.getLocalizedValue("aperteRelax").Replace("\\n", "\n"); ;
        }

        //===============
        //171020 translate dos textos da tela de giveUP (quer abandonar?)
        txtAbandon.text = translate.getLocalizedValue("txtAbandonar");
        foreach (Button b in quitGameMenu.GetComponentsInChildren<Button>())
        {
            if (b.name == "Nao")
            {
                b.GetComponentInChildren<Text>().text = translate.getLocalizedValue("txtNo");
            }
            else
            {
                if (b.name == "Sim")
                {
                    b.GetComponentInChildren<Text>().text = translate.getLocalizedValue("txtYes");
                }
            }
        }
    }

    //---------------------------------------------------------------------------------------
    //170307 antes ia direto para GoToIntro, mas isto fazia com que as animacoes Defendeu/Perdeu ficassem penduradas na tela sem jeito de sair
    //180620 same for gameOver/gameLover
    public void abandonGame()
    {
        //playerSelecionouAbandonarGame = true;
        //170311 evitar abandonar JG e ficarem as variaveis de um possivel ja chamado JM
        uiManager.jogadasFirstScreen = 0;
        jaPasseiPorFirstScreen = false;
        bmGameLover.SetActive(false);      //Amparo, when gameLover, goes out using "yes,abandon", and these screens stayed fixed; corrected!
        bmGameOver.SetActive(false); 
        uiManager.userAbandonModule = true; //to guarantee to save results
        GoToIntro();
    }


    //---------------------------------------------------------------------------------------
    //161227 Tela de menu; vem para cá ao terminar os níveis ou no "sim, quero abandonar este jogo"
    public void GoToIntro()
    {
        if (uiManager.userAbandonModule)    //180618 was: if(game.activeInHierarchy), but now, many options come to GoToIntro...
        {
            uiManager.SendEventsToServer(PlayerPrefs.GetInt("gameSelected"));  //161207: o user pode querer nao avancar e ai perde a gravacao do nivel mesmo que interrupted		                                                                     //      passou para o GameFlowManager.ShowInBetween e GoToIntro (ao abandonar o nivel do jogo)
        }

        //170307 antes estava dentro do if acima, mas ao voltar para o menu, todos deveriam executar esta inicializacao
        uiManager.ResetEventList(PlayerPrefs.GetInt("gameSelected"));

        playing = false;
        intro.SetActive(true);
        betweenLevels.SetActive(false);
        gameCanvas.interactable = false;
        game.SetActive(false);
        introMenu.SetActive(true);
        gameModeMenu.SetActive(false);
        probCalculator.ResetToInitialMachine();
        waitingKeyGameOver = false;
        waitingKeyGameLover = false;
        uiManager.userAbandonModule = false; //180326 reset when start new game
    }



    //---------------------------------------------------------------------------------------
    public void ShowGameModeMenu(int gameSelected) //Josi: 161209 agora a funcao tem um parametro para definir as telas do jogo
    {
        PlayerPrefs.SetInt("gameSelected", gameSelected);

        //170108 garantir os conteudos originais
        if (gameSelected != 2)
            probCalculator.resetOriginalData(); //de sequ e de numPlays


        //170108 inicializacao especial para o MD firstScreen - nao podem ocorrer nas rotinas de inicializacao
        if ((gameSelected == 3) || (gameSelected == 5))
        {       //170124 mantido o base memoria por teclado
            uiManager.jogadasFirstScreen = 0;
            uiManager.acertosFirstScreen = 0;
            uiManager._eventsFirstScreen = new List<RandomEvent>();   //170105 nao pode er inicializado no entre testes de memoria
            jaPasseiPorFirstScreen = false;
            jogarMDfase3 = false;
            firstScreen = false;
        }

        //161212: se o apelido nao foi preenchido, chamar o cata Dados, se não, fica valendo o já digitado alguma hora e inicia-se jogo
        if (PlayerInfo.alias.Trim().Length == 0)
        {
            //170303 mostrar titulo do jogo selecionado(acrescentado também o botao Menu de Jogos para permitir sair da tela) e inicializar critica
            //171009 translate
            //jogoSelecionado.text = ProbCalculator.machines [0].menuList [gameSelected - 1].title;
            jogoSelecionado.text = translate.getLocalizedValue("game" + gameSelected.ToString());

            obrigaAlias.text = System.String.Empty;

            introMenu.SetActive(false);        //Josi: menuInicio em IntroScene(1): botões dos jogos
            gameModeMenu.SetActive(true);      //Josi: userData em IntroScene(1): cata dados: msg, msg se vazio, botao Continuar
            userInfoForm.SetActive(true);      //Josi: sendData em IntroScene(1): cata dados: apelido

        }
        else
        {
            NewGame(gameSelected);    //Josi: 161214: esta chamada estava no LogUser, mas, como não precisou ser ativado, novo Jogo parte daqui
        }
    }


    //---------------------------------------------------------------------------------------
    //Josi: ninguém chama esta function (falta fazer o configurador)
    public void ToConfigurations()
    {
        //PlayerPrefs.DeleteAll();
        //Application.LoadLevel("Configurations");
        SceneManager.LoadScene("Configurations");
    }



    //---------------------------------------------------------------------------------------
    public void NewGame(int gameSelected) //Josi: 161214 agora a funcao tem um parametro para definir as telas do jogo
    {
        useTimer = false;

        // 171109 esconder botão centro se 2 choices; [0]esq, [1]dir, [2]cen
        if (probCalculator.getChoices() == 2)
        {
            uiManager.optBtns[2].SetActive(false);
        }


        if ((gameSelected == 1) || (gameSelected == 4))
        {              //BM ou BMcomTempo
            probCalculator.defineBMSequ(true, 0);                     //sequ pronta ou gerada; 170126 parametros                    
        }
        else
        {
            if ((gameSelected == 3) || (gameSelected == 5))
            {          //MD e Jogo da memoria
                if (!jaPasseiPorFirstScreen)
                {                         // se ainda nao feita, fazer tela inicial
                    if (uiManager.jogadasFirstScreen == 0)
                    {           //mas garantir que nao entrou nenhuma vez
                        sequJMGiven = probCalculator.defineMDSequ();  //esta sequ servirah para as duas fases: firstScreen e JG nivel 3
                        firstScreen = true;                            //170102 para nao correr o risco de chamar gkAnim
                        jogarMDfase3 = false;
                    }
                }
                else
                {
                    probCalculator.setupMDparaJG();              //170104 setar a string a jogar (a mesma ate agora, repetida 3x)
                    firstScreen = false;                          //170303 faltava...
                    jogarMDfase3 = true;
                }
            }
        }
        probCalculator.ResetToInitialMachine();
        uiManager.CorrectPhaseArt(gameSelected);
        uiManager.initSpeedGKAnim();   //180510 to change this specific phase (there is only 3 gkAnim, but many phases)
        uiManager.initKeyboardTimeMarkers();
        StartGame(gameSelected);
    }


    //---------------------------------------------------------------------------------------
    public void StartGame(int gameSelected)       //Josi: 161209: incluir parâmetro para o jogo selecionado
    {
        //180524
        uiManager.initKeyboardTimeMarkers();

        //170316 sessionTime: conta todo o tempo da sessão, incluindo paradas
        startSessionTime = Time.realtimeSinceStartup;
        uiManager.userAbandonModule = false;     //180326 reset when start new game

        //170911 resolver como nasce o botão play/pause
        if (PlayerPrefs.GetInt("startPaused") == 1)
        {  //iniciar com pausa
            if (gameSelected == 4)
            {                           //se AR, inicia com "aperteTecla": nao vale PlayPause
                uiManager.buttonPause.SetActive(false);
                uiManager.buttonPlay.SetActive(true);
                changeAlpha(4, 0.5f);
            }
            else
            {
                if (gameSelected == 5 && firstScreen)
                {        //se JM e fase memorizacao "aperteTela", idem
                    uiManager.mdButtonPause.SetActive(false);
                    uiManager.mdButtonPlay.SetActive(true);
                    changeAlpha(5, 0.5f);
                }
                else
                {                                      //senao, permitir Pausar
                    uiManager.buttonPause.SetActive(false);
                    uiManager.buttonPlay.SetActive(true);
                }
            }

            uiManager.pausePressed = true;

        }
        else
        {  //nao iniciar pausado, selecionado menu "Jogar"
            if (probCalculator.getShowPlayPauseButton())
            {
                if (gameSelected == 4)
                {                           //se AR, inicia com "aperteTecla": nao vale PlayPause
                    uiManager.buttonPause.SetActive(false);
                    uiManager.buttonPlay.SetActive(false);
                }
                else
                {
                    if (gameSelected == 5 && firstScreen)
                    {        //se JM fase memorizacao "aperteTela", idem
                        uiManager.mdButtonPause.SetActive(false);
                        uiManager.mdButtonPlay.SetActive(false);
                    }
                    else
                    {                                      //senao, permitir Pausar
                        uiManager.buttonPause.SetActive(true);
                        uiManager.buttonPlay.SetActive(false);
                    }
                }
            }
            else
            {
                uiManager.buttonPause.SetActive(false);
                uiManager.buttonPlay.SetActive(false);
                uiManager.mdButtonPause.SetActive(false);
                uiManager.mdButtonPlay.SetActive(false);
            }
            uiManager.pausePressed = false;
        }

        //170316 tempos de relaxTime
        startRelaxTime = 0;
        endRelaxTime = 0;
        totalRelaxTime = 0;

        //170912 inicializar tempos gerados pelos botões Play/Pause
        initialPauseTime = 0;
        startOtherPausesTime = 0;
        numOtherPauses = 0;
        otherPausesTime = 0;
        otherPausesTotalTime = 0;  //170919
        minHitsInSequence = 0;     //170921

        uiManager.ResetEventList(gameSelected);   //inicializa lista de eventos e o placar
        game.SetActive(true);                     //GameScene
        intro.SetActive(false);                   //IntroScene(1)
        betweenLevels.SetActive(false);
        gameCanvas.interactable = true;           //GameUICanvas
        quitGameMenu.SetActive(false);           //GiveUpMenu: quer abandonar?

        //170223 carregar o num de jogadas para descanso
        playsToRelax = probCalculator.getPlaysToStartRelax();

        uiManager.aguardandoTeclaBMcomTempo = false;   //170102 
        uiManager.btnExit.SetActive(true);             //170311 fica falso nos casos onde há o "aperteTecla"


        //180410 if parametrized, show "attention point" in middle screen
        if (probCalculator.attentionPointActive())
        {
            uiManager.attentionPointColor(0);        //on Inspector: 0: start, 1:correct, 2:wrong        
        }


        //Josi: 161209: ativar "o que chutar" se BM, ou o painel de historico se JG
        if (gameSelected == 1)
        {                      //BM
            bmMsg.SetActive(false);                  //BM msg tutorial ou aperteTecla
            aperteTecla.SetActive(false);            //BM msg aperteTecla
            logBox.SetActive(false);
            mdFirstScreen.SetActive(false);           //MD inicio
            firstScreen = false;                      //MD

            frameChute.SetActive(true);              //BM: contorno amarelo para as setas de direcao
            uiManager.showNextKick(probCalculator.GetNextKick());  //colocar o primeiro simbolo

        }
        else
        {
            if (gameSelected == 2)
            {                  //JG
                bmMsg.SetActive(false);
                aperteTecla.SetActive(false);                //BM msg aperteTecla
                frameChute.SetActive(false);                 //BM: contorno amarelo para as setas de direcao
                mdFirstScreen.SetActive(false);               //MD inicio
                firstScreen = false;                          //MD
                                                              //mdTutorial.SetActive (false);

                //170622 agora existe o param showHistory para indicar se é para mostrar ou nao o historico na fase
                if (probCalculator.getCurrentShowHistory())
                {
                    logBox.SetActive(true);                  //parametrizado para mostrar historico
                }
                else
                {
                    logBox.SetActive(false);                  //parametrizado para NAO mostrar historico
                }

                uiManager.btnsAndQuestion.SetActive(true);    //setas de direcao
                uiManager.movementTimeA = Time.realtimeSinceStartup; //170309 para nao precisar descontar tempo das animacoes (impreciso)

            }
            else
            {
                if (gameSelected == 3)
                {                      //MD
                    bmMsg.SetActive(false);                  //MD msg tutorial ou aperteTecla
                    aperteTecla.SetActive(false);            //BM msg aperteTecla
                    frameChute.SetActive(false);             //BM: contorno amarelo para as setas de direcao

                    if (jogarMDfase3)
                    {
                        logBox.SetActive(true);
                        uiManager.btnExit.SetActive(true);                   //170311 na tela dos simbolos vale o EXIT
                        uiManager.btnsAndQuestion.SetActive(true);

                        uiManager.movementTimeA = Time.realtimeSinceStartup; //170309 para nao precisar descontar tempo das animacoes (impreciso)															
                        mdFirstScreen.SetActive(false);               //MD inicio
                        firstScreen = false;                          //MD
                    }
                    else
                    {
                        logBox.SetActive(false);             //MD first tela nao traz historico
                        firstScreen = true;                   //MD telaInicial
                        uiManager.showFirstScreenMD(gameSelected);
                    }
                }
                else
                {
                    if (gameSelected == 4)
                    {                  //BM com tempo
                        frameChute.SetActive(false);         //BM: contorno amarelo para as setas de direcao
                        logBox.SetActive(false);
                        mdFirstScreen.SetActive(false);   //MD inicio
                        firstScreen = false;                  //MD

                        bmMsg.SetActive(true);                      //BM frame msg tutorial ou aperteTecla
                        aperteTecla.SetActive(true);                //BM msg aperteTecla
                        uiManager.btnsAndQuestion.SetActive(true);  //fica apenas a msg "aperte uma tecla"
                        uiManager.aguardandoTeclaBMcomTempo = true;
                        uiManager.btnExit.SetActive(false);         //170322 enquanto "aperte tecla" nao vale o EXIT

                        uiManager.decisionTimeA = Time.realtimeSinceStartup;  //170113 apareceu "aperte tecla": inicia-se a contagem do tempo de decisão
                    }
                    else
                    {
                        if (gameSelected == 5)
                        {                      //170124 Jogo da memória (mantida versao jogo=3 com input de teclado)
                            bmMsg.SetActive(false);                  //msg tutorial ou aperteTecla
                            aperteTecla.SetActive(false);            //BM msg aperteTecla
                            frameChute.SetActive(false);             //BM: contorno amarelo para as setas de direcao

                            if (jogarMDfase3)
                            {
                                logBox.SetActive(true);
                                uiManager.btnExit.SetActive(true);         //170311 na tela dos simbolos vale o EXIT
                                uiManager.btnsAndQuestion.SetActive(true);
                                uiManager.movementTimeA = Time.realtimeSinceStartup; //170309 para nao precisar descontar tempo das animacoes (impreciso)

                                mdFirstScreen.SetActive(false);   //MD inicio
                                firstScreen = false;                  //MD

                            }
                            else
                            {
                                logBox.SetActive(false);             //MD first tela nao traz historico
                                firstScreen = true;                   //MD telaInicial
                                uiManager.showFirstScreenMD(gameSelected);
                            }
                        }
                    }
                }
            }
        }


//        //180123 EEG valid for all game modules, not only JG
//        if (probCalculator.getSendMarkersToEEG() != "none")
//        {
//            uiManager.sendStartMoveToSerial();
//        }

        uiManager.BtwnLvls = false;
        playing = true;
    }



    //---------------------------------------------------------------------------------------
    //170125 Jogo da Memória: experimentador passa a Jogar jogo do Goleiro fase 3 (com modificacoes)
    public void jogarMemoriaFase3()
    {
        //170217 para contar o tempo em que o experimentador selecionou Jogar ao inves de Mostrar Again;
        //       talvez seja melhor nao marcar este tempo... fica assim e se for necessario, melhoramos este trecho
        RandomEvent eLog = new RandomEvent();
        eLog.decisionTime = uiManager.decisionTimeB - uiManager.decisionTimeA;  //170214: tempo desde que aparece a tela até que 
        eLog.time = Time.realtimeSinceStartup - uiManager.decisionTimeB;        //170214: tempo desde que apertou "aperte uma tecla quando pronto" até selecionar um botao "Mostrar de novo" ou "Jogar"
        uiManager._eventsFirstScreen.Add(eLog);

        jaPasseiPorFirstScreen = true;
        NewGame(5);     //170126 melhor entrar no fluxo e nao repetir aqui
    }



    //---------------------------------------------------------------------------------------
    public void NextLevel()
    {
        if (!probCalculator.CanGoToNextMachine())
        {
            uiManager.ResetEventList(PlayerPrefs.GetInt("gameSelected"));
            GoToIntro();
        }
        else
        {
            uiManager.ResetEventList(PlayerPrefs.GetInt("gameSelected"));
            probCalculator.GotoNextMachine();
            uiManager.CorrectPhaseArt(PlayerPrefs.GetInt("gameSelected"));
            uiManager.initSpeedGKAnim();   //180510 to change this specific phase (there is only 3 gkAnim, but many phases)

            //180619 reset screens; there is an error in AMPARO experiment that keeps this screen active on the next play
            bmGameLover.SetActive(false);  //180619
            bmGameOver.SetActive(false);   //180619

            StartGame(PlayerPrefs.GetInt("gameSelected"));
        }
    }


    //---------------------------------------------------------------------------------------
    public void ShowInBetween(int gameSelected)   //Josi: 161226: parametro jogo jogado
    {
        playing = false;
        betweenLevels.SetActive(false); //171220

        //170124 solicitado pelo Prof Andre Frazao manter apenas acertos/jogadas sem porcentual
        //Josi: trazer o placar para esta tela de sobreposicao, de troca de nivel
        //		string concordaAcerto = " acertos em ";
        //		string concordaJogada = " jogadas (";
        //		if (uiManager.success == 1) {
        //			concordaAcerto = " acerto em ";
        //		}
        //		if (probCalculator.GetCurrentPlayLimit(gameSelected) == 1) {
        //			concordaJogada = " jogada (";
        //		}

        //170124 solicitado pelo Prof Andre Frazao manter apenas acertos/jogadas sem porcentual
        //placarFinal: xxx acertos em yyy jogadas (zzz.zz%)
        //placarFinal.text = uiManager.success.ToString ().PadLeft (3).Trim () + concordaAcerto
        //	+ probCalculator.GetCurrentPlayLimit (gameSelected).ToString ().PadLeft (3).Trim () + concordaJogada 
        //	+ ((uiManager.success * 100f) / (float)probCalculator.GetCurrentPlayLimit (gameSelected)).ToString ("F2").Trim () + "%)";
        // 170412 refeito todo o trecho considerando o novo param finalScoreboard
        //placarFinal.text = uiManager.success.ToString ().PadLeft (3).Trim () + "/" +
        //	probCalculator.GetCurrentPlayLimit (gameSelected, uiManager.phaseZeroJG).ToString ().PadLeft (3).Trim (); //170216 novo param no PlayLimit


        //170412 para atender aos Prof Bruno e Prof Andre, criado o param finalScoreboard com as opcoes long (bruno), short/none (andre)
        //171006 translate
        string concordaAcerto = translate.getLocalizedValue("concordaAcertos");
        string concordaJogada = translate.getLocalizedValue("concordaJogadas");

        if (probCalculator.getCurrentFinalScoreboard() == "long")
        {
            if (uiManager.success == 1)
            {
                //171006 translation; concordaAcerto = " acerto em ";
                concordaAcerto = translate.getLocalizedValue("concordaAcerto");
            }
            if (probCalculator.GetCurrentPlayLimit(gameSelected) == 1)
            {
                //171006 translation; concordaJogada = " jogada (";
                concordaJogada = translate.getLocalizedValue("concordaJogada");
            }

            //placarFinal: xxx acertos em yyy jogadas (zzz.zz%)
            placarFinal.text = uiManager.success.ToString().PadLeft(3).Trim() + concordaAcerto
                + probCalculator.GetCurrentPlayLimit(gameSelected).ToString().PadLeft(3).Trim() + concordaJogada
                + ((uiManager.success * 100f) / (float)probCalculator.GetCurrentPlayLimit(gameSelected)).ToString("F2").Trim() + "%)";
        }
        else
        {
            if (probCalculator.getCurrentFinalScoreboard() == "short")
            {
                //placarFinal: xxx/yyy
                placarFinal.text = uiManager.success.ToString().PadLeft(3).Trim() + "/" +
                probCalculator.GetCurrentPlayLimit(gameSelected).ToString().PadLeft(3).Trim(); //170216 novo param no PlayLimit
            }
            else
            {
                //placarFinal: null
                placarFinal.text = "";
            }
        }


        if (probCalculator.CanGoToNextMachine())
        {
            game.SetActive(true);
            intro.SetActive(false);

            bmMsg.SetActive(false);    //Josi: 161212: desativar tutorial BM
            logBox.SetActive(false);   //              e historico de jogadas

            //180619 reset screens; there is an error in AMPARO experiment that keeps this screen active on the next play
            bmGameLover.SetActive(false);  //180619
            bmGameOver.SetActive(false);   //180619

            betweenLevels.SetActive(true);
            uiManager.BtwnLvls = true;

            //BM(1) e BMcomTempo(4) têm apenas um nivel: fica no campinho; MD(3) e Memoria(5) idem, soh profissional
            if ((gameSelected == 1) || (gameSelected == 4) || (gameSelected == 3) || (gameSelected == 5))
            {
                //170927 novo param em btLevelController para precisar o nome do jogo
                int bmMode = (probCalculator.getMinHitsInSequence() > 0) ? 2 : 1;

                btLevelsController.EndGame(gameSelected, bmMode);
            }
            else
            {
                //no JG nao há este apendice ao nome, entao vai zero
                if (probCalculator.GetCurrMachineIndex() + 1 >= uiManager.GetTotalLevelArts())
                {
                    btLevelsController.PostEndGame(gameSelected, 0);
                }
                else
                {
                    btLevelsController.MiddleGame(gameSelected, 0);
                }
            }

            gameCanvas.interactable = false;

            //161207: passa a gravar ao chegar na tela betweenLevels, nao ao Avancar
            uiManager.SendEventsToServer(gameSelected);  //170109

        }
        else
        {

            //Josi: 161207: passa a gravar ao chegar na tela betweenLevels, nao ao Avancar; ultimo nivel eh um caso especial
            uiManager.SendEventsToServer(gameSelected);   //170109

            uiManager.ResetEventList(gameSelected);
            game.SetActive(true);
            intro.SetActive(false);
            betweenLevels.SetActive(true);

            //170927 novo param em btLevelController para precisar o nome do jogo
            int bmMode = (probCalculator.getMinHitsInSequence() > 0) ? 2 : 1;

            btLevelsController.EndGame(gameSelected, bmMode);    //170927 novo param bmMode para AQ/AR minHits ou minSequ
            gameCanvas.interactable = false;
        }
    }


    //---------------------------------------------------------------------------------------
    //180628 changed by LocalizationManager.clickSair()
    public void CloseApp()
    {
        translate.clickSair();
    }



    //---------------------------------------------------------------------------------------
    //Josi: valer o botão Nao do "deseja abandonar este jogo?"; apenas remove a tela da mensagem e volta aa tela anterior
    public void keepOnGame()
    {
        quitGameMenu.SetActive(false);

        //170311 AQ com tempo: se user EXIT e desiste,o click pode acabar valendo para o "aperteTecla"
        if (PlayerPrefs.GetInt("gameSelected") == 4)
        {

            uiManager.aguardandoTeclaBMcomTempo = true;
            uiManager.btnsAndQuestion.SetActive(true);         //fica apenas a msg "aperte uma tecla"

            bmMsg.SetActive(true);              //BM frame msg tutorial ou aperteTecla
            uiManager.btnExit.SetActive(false);  //170322 entra o "aperte tecla" passa a nao valer o EXIT

            aperteTecla.SetActive(true);        //BM msg aperteTecla
            frameChute.SetActive(false);

            //170914 se "aperte tecla", desativar Play/Pause
            uiManager.buttonPause.SetActive(false);
            uiManager.buttonPlay.SetActive(false);

            //nao iniciar tempos, para que some com esta espera
        }
        else
        {
            if (PlayerPrefs.GetInt("gameSelected") == 5)
            {
                if (!jaPasseiPorFirstScreen)
                {
                    uiManager.aguardandoTeclaMemoria = true;

                    //170915 se "aperte tecla", desativar Play/Pause
                    uiManager.mdButtonPause.SetActive(false);
                    uiManager.mdButtonPlay.SetActive(false);
                }
                else
                {
                    uiManager.btnExit.SetActive(true);
                }
            }
        }
    }


    //---------------------------------------------------------------------------------------
    //Josi: botao SAIR na tela inicial de menu de jogos
    //180628 changed by Exit Icon: betweenLevels screen
    //public void Sair()
    //{
    //    //170322 unity3d tem erro ao usar application.Quit
    //    //       workaround: http://unity3dtrenches.blogspot.com.br/2015/10/unity-3d-compiled-game-freezes-or.html
    //    //Application.Quit ();
    //    if (!Application.isEditor)
    //    {  //if in the editor, this command would kill unity...
    //        if (Application.platform == RuntimePlatform.WebGLPlayer)
    //        {
    //            Application.OpenURL(PlayerPrefs.GetString("gameURL"));
    //        }
    //        else
    //       {
    //            //171121 not working kill()
    //            if ((Application.platform == RuntimePlatform.IPhonePlayer) ||
    //                (SystemInfo.deviceModel.Contains("iPad")))
    //            {           //try #IF UNITY_IOS
    //                Application.Quit();
    //            }
    //            else
    //            {
    //                System.Diagnostics.Process.GetCurrentProcess().Kill();
    //            }
    //        }
    //    }
    //}


    //---------------------------------------------------------------------------------------
    public bool playing = false;  //180402 public now: needed to avoid capture keys when gameOver/gameLover active
    public void Update()
    {
        //Josi: outra maneira de Sair, sem clicar no botão: apertar a tecla ESCAPE
        //      https://docs.unity3d.com/ScriptReference/Application.Quit.html
        //
        if (Input.GetKey("escape"))
        {
            translate.clickSair();

        }

        //170915 trocado por "se nao pausado, pegaInput" dado que nao funcionou pegar o current;
        //       algum dia deve-se  identar todo este jogo...
        if (!uiManager.pausePressed)
        {

            //170222 pegar tecla do "aperte qualquer tecla para continuar" após descanso
            if (uiManager.aguardandoTeclaPosRelax)
            {
                if (Input.GetKeyDown("space") || Input.GetMouseButtonDown(0))  //they want to stop the game in mobiles?!
                {  //170223 aceitar tecla especifica para não confundir com as do jogo e a msg passar em branco
                   //if (Input.anyKey) {               //para aceitar qualquer tecla
                    uiManager.aguardandoTeclaPosRelax = false;
                    //uiManager.estouNoPegaQualquerTecla = true;  //170110 para aceitar qualquer tecla, inclusive as do jogo
                    relaxTime.SetActive(false);

                    endRelaxTime = Time.realtimeSinceStartup;
                    totalRelaxTime = totalRelaxTime + (endRelaxTime - startRelaxTime);  //170317

                    uiManager.movementTimeA = Time.realtimeSinceStartup; //170413 inicializar tempo de movto aqui, logo após sumir a tela de relax
                                                                         //estava dando erro de tempo negativo no move logo após a tela; nao entendi porque - mudei a estrategia
                }
            }


            //170311 pegar "qualquer tecla" se ha erro no config
            if (waitingKeyToExit)
            {
                if (Input.anyKey || Input.GetMouseButtonDown(0))
                {       //para aceitar qualquer tecla!
                        //170322 unity3d tem erro ao usar application.Quit
                        //       workaround: http://unity3dtrenches.blogspot.com.br/2015/10/unity-3d-compiled-game-freezes-or.html
                        //Application.Quit ();

                    //180105 the function verifies if serial isOpen before close
                    //uiManager.closeSerialPort();

                    if (!Application.isEditor)
                    {  //if in the editor, this command would kill unity...
                        if (Application.platform == RuntimePlatform.WebGLPlayer)
                        {
                            Application.OpenURL(PlayerPrefs.GetString("gameURL"));
                        }
                        else
                        {
                            //171121 not working kill()
                            if ((Application.platform == RuntimePlatform.IPhonePlayer) ||
                                (SystemInfo.deviceModel.Contains("iPad")))
                            {           //try #IF UNITY_IOS
                                Application.Quit();
                            }
                            else
                            {
                                System.Diagnostics.Process.GetCurrentProcess().Kill();
                            }
                        }
                    }
                }
            }

            //170925 take "spacebar" after "game over" (better keep separated from gameLover...)
            //       Input.GetMouseButtonDown(0) simulates a tap on mobile devices
            if (waitingKeyGameOver)
            {
                if (Input.GetKeyDown("space") || Input.GetMouseButtonDown(0))
                {  //180620 aceitar tecla especifica para não confundir com as do jogo e a msg passar em branco
                   //if (Input.anyKey) {               //para aceitar qualquer tecla
                    waitingKeyGameOver = false;
                    bmGameOver.SetActive(false);
 //@@                   if (PlayerPrefs.GetInt("gameSelected") == 2)
 //@@                   {
                        ShowInBetween(PlayerPrefs.GetInt("gameSelected"));
 //@@                   }
 //@@                   else
 //@@                   {
 //@@                       GoToIntro();  //180627 in this way, the player can't see the betweenLevels screen, creating a different protocol
 //@@                   }
                }
            }

            //180321 take "spacebar" after "game lover"
            //       Input.GetMouseButtonDown(0) simulates a tap on mobile devices
            if (waitingKeyGameLover)
            {
                if (Input.GetKeyDown("space")  || Input.GetMouseButtonDown(0))
                {  //180620 aceitar tecla especifica para não confundir com as do jogo e a msg passar em branco
                   //if (Input.anyKey) {                //para aceitar qualquer tecla
                    waitingKeyGameLover = false;
                    bmGameLover.SetActive(false);
 //@@                   if (PlayerPrefs.GetInt("gameSelected") == 2)
 //@@                   {
                        ShowInBetween(PlayerPrefs.GetInt("gameSelected"));
 //@@                   }
 //@@                   else
 //@@                   {
 //@@                   GoToIntro();  //180627 in this way, the player can't see the betweenLevels screen, creating a different protocol
 //@@               }
                }
            }
        }
    }


    //---------------------------------------------------------------------------------------
    //170912 para trocar o alpha do "aperte uma tecla" que fica mais claro se o jogo está em modo Pause
    public void changeAlpha(int game, float alpha)
    {
        Color colorPressKey;
        if (game == 4)
        {
            colorPressKey = aperteTecla.GetComponent<Text>().color;
            colorPressKey.a = alpha;
            aperteTecla.GetComponent<Text>().color = colorPressKey;
        }
        else
        {
            if (game == 5)
            {
                colorPressKey = mdAperteTecla.GetComponent<Text>().color;
                colorPressKey.a = alpha;
                mdAperteTecla.GetComponent<Text>().color = colorPressKey;
            }
        }
    }


    //---------------------------------------------------------------------------------------
    //180105 to show error messages that avoid to continue in the game
    public void showErrorMessage(string txtMsgKey, int powerOfTwo)
    {
        errorNumber = errorNumber - powerOfTwo;
        if (txtMessage.text == string.Empty)
        {
            txtMessage.text = translate.getLocalizedValue(txtMsgKey);
        }
        else
        {
            txtMessage.text = txtMessage.text + "\n" + translate.getLocalizedValue(txtMsgKey);
        }
    }




    // -----------------------------------------------------------------------------------------------------
    //180605 credits screen (by Carlos Ribas): change from LoadStages to GameFlow, near other buttons (Tutorial, About, etc)
    public void showCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    // -----------------------------------------------------------------------------------------------------
    //180605 tutorial screen: change from LoadStages to GameFlow, near other buttons (Tutorial, About, etc)
    //180626 temporarily shows the old four frames
    public void showTutorial()
    {
        scrTutorial.SetActive(true);
    }

    // -----------------------------------------------------------------------------------------------------
    //180605 about screen: change from LoadStages to GameFlow, near other buttons (Tutorial, About, etc)
    public void showAbout()
    {
        SceneManager.LoadScene("About");
    }

    // -----------------------------------------------------------------------------------------------------
    //180605 prizes screen: change from LoadStages to GameFlow, near other buttons (Tutorial, About, etc)
    public void showPrizes()
    {
        //SceneManager.LoadScene("Credits");
        bkgPrizes.SetActive(true);
    }


    // -----------------------------------------------------------------------------------------------------
    //180614 come back to Team Selection
    public void backToTeamSelection()
    {
        SceneManager.LoadScene("Configurations");
    }



    // -----------------------------------------------------------------------------------------------------
    // before, this was a question after inform playerAlias and was used for all games played in the same session
    // now, it is a option above the game menu
    // 180627 StartGamePaused yes or no (COM pausa/SEM pausa (default)
    public void startGamePaused(bool startPaused)
    {
        //170913 catar o parametro que indica se é para iniciar os jogos com pausa ou não
        PlayerPrefs.SetInt("startPaused", startPaused ? 1 : 0); //menu "Jogar com pausa" selecionado
    }


}