/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// Rewrited by Josi Perez <josiperez.neuromat@gmail.com>
//
//	This Module is responsible for loading the screen between levels
/************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BetweenLevelsController : MonoBehaviour 
{
//	public GameObject btnExit;
	public GameObject btnContinue;
//	public GameObject btnsAndQuestion;
	public GameObject btnMenu;   //Josi: 161212: botao Menu no entre jogos - mainScene/GameScene/BetweenLevelsCanvas/Panel/

	public ProbCalculator probCalculator;  //Josi: 161214: para trazer o nivel do jogo na tela de BetweenLevels

	public Text msg;
	public Text apelidoJogador;    //Josi: mainScene/GameScene/BetweenLevelsCanvas/Panel/ApelidoJogador
	public Text jogoSelecionado; //Josi: 161214: mainScene/GameScene/BetweenLevelsCanvas/jogoSelecionado


//	private readonly string endMsg = "Parabéns!! Você é um ótimo goleiro! Obrigado por jogar o NeuroGol!";  //Josi 161215 Magda: este nome nao eh mais usado
	//171010 sai o readonly (soh alteravel na inicializacao) - serao preenchidas no start
	private string endMsg;      //= "Você completou a fase! Obrigado por jogar o Jogo do Goleiro!";
	private string middleMsg;   // = "Parabéns, você atingiu o próximo nível!";
	private string postEndMsg;  // = "Parabéns, agora você já é um profissional! Deseja encarar desafios ainda maiores?";

	//Josi: 161226: msg especificas para os jogos BM e MD - confirmar textos
//	private readonly string endMsgBM = "Parabéns!! Você é um ótimo goleiro! Obrigado por jogar Base Motora!";
//	private readonly string endMsgMD = "Parabéns!! Você é um ótimo goleiro! Obrigado por jogar Memória Declarativa!";
	private string endMsgBM;   // = "Você completou a fase! Obrigado por jogar o Aquecimento!";
	private string endMsgMD;   // = "Você completou a fase! Obrigado por jogar o Jogo da Memória!";

	private LocalizationManager translate;        //171009 trazer script das rotinas de translation

	// Use this for initialization
	void Start () {

		//171009 declarar a instance para permitir chamar rotinas do outro script
		translate = LocalizationManager.instance;

		//171010 translate msg
		endMsg =  translate.getLocalizedValue ("obrigado") + translate.getLocalizedValue ("game2");
		endMsgBM = translate.getLocalizedValue ("obrigado") + translate.getLocalizedValue ("game1");
		endMsgMD = translate.getLocalizedValue ("obrigado") + translate.getLocalizedValue ("game5");
		middleMsg = translate.getLocalizedValue ("middleMsg");
		postEndMsg = translate.getLocalizedValue ("endMsg");

		//171010 translate botoes
		btnContinue.GetComponentInChildren<Text>().text = translate.getLocalizedValue ("proxFase").Replace("\\n","\n");;
		btnMenu.GetComponentInChildren<Text>().text = translate.getLocalizedValue ("menu1").Replace("\\n","\n");
		//btnExit.GetComponentInChildren<Text>().text = translate.getLocalizedValue ("sair").Replace("\\n","\n");

		//btnExit.SetActive (true);
		btnContinue.SetActive (true);
		btnMenu.SetActive (true);     //180510: botao Menu em todos os betweenLevel por uniformidade

		msg.text = middleMsg;
	}


	//----------------------------------------------------------------------------------------
	public void MiddleGame(int gameSelected, int bmMode)
	{
		//btnExit.SetActive (true);
		btnContinue.SetActive (true);

		btnMenu.SetActive (true);               //180510: botao Menu em todos os betweenLevel por uniformidade
		msg.text = middleMsg;
		gameSelectedText (gameSelected, bmMode);
	}


	//----------------------------------------------------------------------------------------
	public void PostEndGame(int gameSelected, int bmMode) 
	{
		//btnExit.SetActive (true);
		btnContinue.SetActive (true);

		btnMenu.SetActive (true);                //Josi: 161212: botao Menu
		msg.text = postEndMsg;
		gameSelectedText (gameSelected, bmMode); //180327 no more phaseZero parameters
	}


	//----------------------------------------------------------------------------------------
	public void EndGame(int gameSelected, int bmMode)  //161226 parametro jogo jogado, com msg especifica para cada um
	{                                                                    //170927 bmMode (AQ/AR): 1=minHits, 2=minSequ
		//btnExit.SetActive (true);
		btnContinue.SetActive (false);
		btnMenu.SetActive (true);

		if ((gameSelected == 1) || (gameSelected == 4)) { //BM ou BMcomTempo
			msg.text = endMsgBM;
		} else {
			if (gameSelected == 2) { 
				msg.text = endMsg;
			} else {
				if ((gameSelected == 3) || (gameSelected == 5)) { 
					msg.text = endMsgMD;
				}
			}
		}								
		gameSelectedText (gameSelected, bmMode); //180327 no more phaseZero parameters
		                                         //170927 bmMode (AQ/AR): 1=minHits, 2=minSequ
	}


	//----------------------------------------------------------------------------------------
	//161214 para mostrar no betweenLevels, o jogo jogado e o nivel
	public void gameSelectedText(int gameSelected, int bmMode)  //180327 no more phaseZero parameters  
	{                                                           //170927 bmMode (AQ/AR): 1=minHits, 2=minSequ
		apelidoJogador.text = PlayerInfo.alias.ToUpper();       //Josi: 161214
		string level = (probCalculator.GetCurrMachineIndex () + 1).ToString();

		//171009 translate nome do jogo e manter adaptacoes
		string gameN;
		gameN = "game" + gameSelected.ToString();
		jogoSelecionado.text = translate.getLocalizedValue (gameN);  //171009
		if (gameSelected == 2) {
			jogoSelecionado.text = jogoSelecionado.text + " " + translate.getLocalizedValue ("fase");
			jogoSelecionado.text = jogoSelecionado.text + level;
		}


		//170927 se AQ/AR indicar o modo de jogo: por min de jogadas certas ou por min de jogadas certas em sequ
		if (gameSelected == 1 || gameSelected == 4) { 
			string bmModeText = " (minSequ)";    //mais provavel de ocorrer
			if (bmMode == 1) {
				bmModeText = " (minHits)";
			};
			jogoSelecionado.text = jogoSelecionado.text + bmModeText;
		}
	}

}
