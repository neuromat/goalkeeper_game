/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// Rewrited by Josi Perez <josiperez.neuromat@gmail.com>
//
// Module responsible to keep the contexts loaded through json files as well as manage 
// the moves to generate random probabilities associated with the correspondent tree
/************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;
using System.IO;
using System;			//for catch (Exception e)
using System.Text;      //for StringBuilder



//------------------------------------------------------------------------------------
public class StateMachine
{
	public string id;         //identificador da fase
	public int choices;       //no caso deste Jogo, 3: esquerda, direita, centro
	public int limitPlays;	  //limite de jogadas JG
	public int depth;         //altura da arvore para quando eh necessario trazer simbolos anteriores

	//Josi: JG jogadas via sequenciaOtima ou via arvore
	public bool readSequ;     //ler a sequencia manual ou gerada pelo programa a partir da arvore
	public string sequ;       //tripa com uma sequencia otima gerada a partir da arvore correspondente
	public string sequR;      //170214: tripa com a indicacao de posicoes randomicas (Y) ou não (n) na sequ manual, no JG

	//Josi: BM 
	public string bmSequ;     //sequencia que o jogador deve seguir
	public int bmLimitPlays;  //limite de jogadas seguindo a sequencia, no base motora
	public bool bmReadSequ;   //161227 no BM, ler sequ ou gerar sequ?
	public int bmMinHits;	  //170124 nao avancar enquanto minHits nao tenha sido atingido
	public int bmMinHitsInSequence;     //170919 num min de jogadas certas em sequência (amparo)
	public int bmMaxPlays;    //170919 num max de jogadas esperando que o jogador acerte bmMinHitsInSequence (amparo)


	//Josi: MD
	public string mdSequ;     //sequencia que o jogador deve ouvir do experimentador
	public int mdLimitPlays;  //limite de jogadas seguindo a sequencia, no base memoria (antigo Memoria Declarativa)
	public bool mdReadSequ;   //161227 no MD, ler sequ ou gerar sequ?
	public int mdMaxPlays;    //180320 num max de jogadas esperando que o jogador acerte JM 3x 4 símbolos (amparo)
	public int mdMinHitsInSequence; //180321 num min de jogadas certas em sequência, no JM; se zero, acertar 12 em qualquer posição


	//Josi: genericos
	public string animationTypeJG;      //170214: LONG (3s), SHORT(1s), NONE(0s) das animacoes defendeu/perdeu+som
	public string animationTypeOthers;  //170217: LONG (3s), SHORT(1s), NONE(0s) das animacoes defendeu/perdeu+som
	public bool scoreboard;             //170214: true | false, para colocar ou não o placar no JG (na fase)
	public string finalScoreboard;      //170412: long (com porcentagem), short (acertos/jogadas), none
	public int playsToRelax;            //170215: número de jogadas onde o programa dá uma parada para que o experimento faca uma pausa
	public bool showHistory;			//170622: mostrar ou não o historico de andamento das 8 ultimas jogadas
	public string sendMarkersToEEG;     //170623: bool: send or not, markers to the EEG using parallel port in LPT1 0x378 em windows 32/64bits
	                                    //180103: string: send (serial, parallel:LPT1 0x0378 in Windows 32/64bits) or not, markers to the EEG
	public string portEEGserial;        //180103: string: if sendMarkers = serial, send to this port; format COMx
	public string groupCode;            //180403: NES synchronyzation; 170629: to identify registries from an experiment (old researchGroup)

	public bool showPlayPauseButton;    //170918: iniciar ou não, com o jogo em Pausa, para explicações do experimentador ao jogador
	                                    //        virou o botão "Continuar com pausa" além do Continuar, no cataApelido
	public int minHitsInSequence;       //180320: assintota jogadas: núm que determina que o jogador "adivinhou" o padrão, por acertar em sequência

	public string leftInputKey;         //180328 in addition to the mouse and the arrow keys, use this key for left defense
	public string centerInputKey;       //180328 in addition to the mouse and the arrow keys, use this key for center defense
	public string rightInputKey;        //180328 in addition to the mouse and the arrow keys, use this key for right defense
	public string pausePlayInputKey;	//180403 internal control (same as playPause button)

	public string institution;          //180403 NES integration: to unique identify: 
	                                    //       institution+groupCode+soccerTeam+game+phase+playerAlias

	public bool attentionPoint;         //180410 to show or not an attention point (EEG experiments)
	public string attentionDiameter;     //180410 reference with default; can be negative or positive; examples: -0.5, 0.5, 1.0
	public string attentionColorStart;  //180410 pointColor for start a play
	public string attentionColorCorrect;//180410 pointColor for correct selection
	public string attentionColorWrong;  //180410 pointColor for wrong selection

	public string speedGKAnim;           //180413 animation player/ball/goalkeeper

	public string portSendData;
	public string timeFaixa0;
	public string timeFaixa1;
	public string timeFaixa2;
	public string timeFaixa3;
	public string timeFaixa4;

	//contextos e probabilidades
	public Dictionary<string, JsonStateInput> states;
	public List<string> dicKeys;
	public StateMachine()
	{
		states = new Dictionary<string, JsonStateInput> ();
		dicKeys = new List<string> ();
	}

	//170221 menu no JSON 
	public List<JsonGameMenuInput> menuList;          //system.NullReferenceObject
}


//------------------------------------------------------------------------------------
public class ProbCalculator : MonoBehaviour 
{
	public bool ehRandomKick;    //170215 para determinar se uma jogada dada é randomica (sorteada) ou não (deterministica)
	public bool configFileIncompatibleWithVersion = false;  //170626 determinar se existem estes parametros no arquivo de configuracao
	public static List<StateMachine> machines = new List<StateMachine> ();

	public int currentStateMachineIndex;
	JsonStateInput currentState;
	List<string> transitionHistory = new List<string> ();


	public int currentSequOtimaIndex = 0;      //Josi: variaveis para ler a sequOtima no JG
	public int currentBMSequIndex = 0;         //Josi: para ir lendo a bmSequ até atingir o num de jogadas pedidas no BM
	public int currentMDSequIndex = 0;         //Josi: sequ que sera passada verbalmente
	public int currentJGSequIndex = 0;         //170216: idx para seguir a sequ definida na phase0 do JG


	public String saveOriginalBMsequ;          //170108 salvar conteudo original antes de sobrepor no BM sequ dado
	public String saveOriginalMDsequ;          //170108 salvar conteudo original antes de sobrepor no MD sequ dado
	public int saveOriginalBMnumPlays;         //170126 o numero de jogadas pode ser acrescido na obrigatoriedade do num Minimo de acertos
	public int saveOriginalMDnumPlays;         //170126 o numero de jogadas sera alterado pelo size de 3x sequ


	private bool menuListAlreadyDefined = false;     //170303 se carregado menu do primeiro arq de config nao precisa mais carregar
	private bool baseMotoraAlreadyDefined = false;   //170303 se carregados params BM no primeiro arq de config nao precisa mais carregar
	private bool baseMemoriaAlreadyDefined = false;  //170303 se carregados params MD no primeiro arq de config nao precisa mais carregar

	public GameObject errorMessages;       //180115 panel for error messages reading JSON
	public Text txtMessage;                //170623 txt do erro

	static private ProbCalculator _instance;
	static public ProbCalculator instance
	{
		get {
			if(_instance == null) {
				_instance = GameObject.Find("ProbCalculator").GetComponent<ProbCalculator>();
			}
			return _instance;
		}	
	}
		

	//------------------------------------------------------------------------------------
	//read JSON configuration file; collect error message examples
	StateMachine LoadJson(string json)
	{
		JsonInput input = null;
		if(json != System.String.Empty) {    //170216 Use System.String.Empty instead of "" when dealing with lots of strings;) 
			try { 
				input = JsonReader.Deserialize<JsonInput> (json);		
			}

			catch (Exception e)	{
				errorMessages.SetActive (true);
				txtMessage.text = "INVALID JSON CONFIGURATION FILE:\n" + e.Message; //180118 generic error; show exception message
				//180322 "int32": file not saved as UTF-8 without BOM
				//       "unterminated JSON object": without commas between lines (try json validator on web)

				if (e.GetType().IsSubclassOf(typeof(Exception)))
					throw;
			}
		}   

				
		//-----------------------------------
		StateMachine s = new StateMachine ();
		s.choices = input.GetChoices();
		s.depth = input.GetDepth();
		s.limitPlays = input.GetLimitPlays();
		s.minHitsInSequence = input.minHitsInSequence;  //180320 para encerrar o JG assim que o jogador atingir a assíntota
		                                                //       (núm que define que o jogador aprendeu o padrão, por acertar em sequência)

		s.readSequ = input.readSequ;               //Josi: jogar por sequOtima ou por arvore randomica no JG
		s.sequ = input.sequ;                       //Josi: sequOtima lida no treeN.txt para o JG
		s.sequR = input.sequR;                     //170214: indica chute deterministico(n=nao randomico)/probabilistico(Y=randomico) na sequOtima manual

		if (! baseMotoraAlreadyDefined) {          //170303 se params BM* nao carregados (tree1.txt), carregar
			s.bmSequ = input.bmSequ;               //Josi: leitura da sequencia a indicar ao jogador no BM
			s.bmLimitPlays = input.bmLimitPlays;   //Josi: leitura do num de vezes a executar a sequencia do BM
			s.bmReadSequ = input.bmReadSequ;       //Josi 161227: ler sequ ou gerar sequ no BM
			s.bmMinHits = input.bmMinHits;         //Josi 170124: minimo de acertos para encerrar a fase
			s.bmMinHitsInSequence = input.bmMinHitsInSequence;    //170919 num min de jogadas certas em sequência (amparo)
			s.bmMaxPlays = input.bmMaxPlays;       //170919 num max de jogadas esperando que o jogador acerte bmMinHitsInSequence (amparo)
			baseMotoraAlreadyDefined = true;
		}

		if (! baseMemoriaAlreadyDefined) {          //170303 se params MD* nao carregados (tree1.txt), carregar
			s.mdSequ = input.mdSequ;               //Josi: leitura da sequencia a indicar ao jogador no MD
			s.mdLimitPlays = input.mdLimitPlays;   //Josi: leitura do num de vezes a executar a sequencia do MD
			s.mdReadSequ = input.mdReadSequ;       //Josi 161227: ler sequ ou gerar sequ no MD
			s.mdMaxPlays = input.mdMaxPlays;       //180320 num max de jogadas esperando que o jogador acerte 3x (ou diferente)
			s.mdMinHitsInSequence = input.mdMinHitsInSequence; //180321 num min de jogadas certas em sequência, no JM; se zero, acertar 12 em qualquer posição
			baseMemoriaAlreadyDefined = true;
		}

		s.animationTypeJG = input.animationTypeJG;          //170214 animacao defendeu/perdeu long, short ou none
		s.animationTypeOthers = input.animationTypeOthers;  //170217 animacao defendeu/perdeu long, short ou none
		s.scoreboard = input.scoreboard;                    //170214 ter ou nao o contador na tela
		s.finalScoreboard = input.finalScoreboard;          //170412 placar final
		s.playsToRelax = input.playsToRelax;                //170215 num jogadas para dar um break de descanso ao paciente
		s.showHistory = input.showHistory;					//170622 mostrar ou não o historico de andamento das 8 ultimas jogadas
		s.sendMarkersToEEG = input.sendMarkersToEEG;        //170623 enviar ou não marcadores para o EEG paralelo 0x378 em Windows32bits
		                                                    //180103 contains the port to connect with EEG, or "none" if not send
		s.portEEGserial = input.portEEGserial;              //180103 if connect by serial, inform the COMx to use
		s.groupCode = input.groupCode;                      //180403: NES synchronyzation; 170629: to identify registries from an experiment (old researchGroup)

		s.showPlayPauseButton = input.showPlayPauseButton;  //170918: iniciar ou não, com o jogo em Pausa, para explicações do experimentador ao jogador
		                                                    //        virou o botão "Continuar com pausa" além do Continuar, no cataApelido

		s.leftInputKey = input.leftInputKey;                //180328 in addition to the mouse and the arrow keys, use this key for left defense
		s.centerInputKey = input.centerInputKey;            //180328 in addition to the mouse and the arrow keys, use this key for center defense
		s.rightInputKey = input.rightInputKey;              //180328 in addition to the mouse and the arrow keys, use this key for right defense
		s.pausePlayInputKey = input.pausePlayInputKey;	    //180403 internal control (same as playPause button for the experimenter)

		s.institution = input.institution;                  //180403 NES integration: to unique identify institution+groupCode+soccerTeam+game+phase+playerAlias
		s.attentionPoint = input.attentionPoint;            //180410 to show or not an attention point (EEG experiments)
		s.attentionDiameter = input.attentionDiameter;      //180410 reference with default; can be negative or positive; examples: -0.5, 0.5, 1.0
		s.attentionColorStart = input.attentionColorStart;  //180410 pointColor for start a play
		s.attentionColorCorrect = input.attentionColorCorrect;//180410 pointColor for correct selection
		s.attentionColorWrong = input.attentionColorWrong;  //180410 pointColor for wrong selection

		s.speedGKAnim = input.speedGKAnim;                  //180413 animation player/ball/goalkeeper
		if (s.speedGKAnim == null) {                        //180510 QG para contornar experimento najman/noslen sem este param;
			s.speedGKAnim = "1.0";                          //       se for o caso, manter valor default
		}

		s.portSendData = input.portSendData;
		s.timeFaixa0 = input.timeFaixa0;
		s.timeFaixa1 = input.timeFaixa1;
		s.timeFaixa2 = input.timeFaixa2;
		s.timeFaixa3 = input.timeFaixa3;
		s.timeFaixa4 = input.timeFaixa4;

		s.id = input.id;
		foreach(JsonStateInput i in input.states) {	
			s.dicKeys.Add(i.path);
			s.states[i.path] = i;
		}

		//170221 ler o menu (game: numero dentro do programa; title: texto para o botao; sequMenu: sequencia do jogo na tela - 0 nao deve aparecer)
		if (! menuListAlreadyDefined) {                     //170303 se param menu de Jogose nao carregado, carregar
		    s.menuList = new List<JsonGameMenuInput> ();    //170224 Prof Gubi encontrou o erro da falta desta linha para inicializar o vetor
			if (input.menus != null) {                      //171009 caso nao exista o param menus ou seja treeN.txt com N>1
				foreach (JsonGameMenuInput i in input.menus) {
					s.menuList.Add (i);
					menuListAlreadyDefined = true;
				}
			}
		}
		return s;
	}


	//------------------------------------------------------------------------------------
	//171109 2 choices: 2 defense directions (left or right)
	int TwoChoices(int gameSelected, int MDinput)
	{
		if ((gameSelected == 1) || (gameSelected == 4)) {  //BM sem e com tempo
			//Josi: ler da sequ dada ou gerada - ambas guardadas aqui
			//result contem o chute que deveria ter sido dado, determinado pela sequ lida no  arq de configuracao, ou gerada
			string result = (machines [currentStateMachineIndex].bmSequ).Substring (currentBMSequIndex, 1);
			currentBMSequIndex += 1;

			//garantir que a sequ tem tamanho suficiente para o proximo chute
			if (currentBMSequIndex == (machines [currentStateMachineIndex].bmSequ).Length)
				currentBMSequIndex = 0;

			return System.Convert.ToInt16 (result);
		} //fim BM (Aquecimento)

		//-----------------------------------------------------------------------------------------------------------
		else {
			if (gameSelected == 2) {  //JG 
				if (machines [currentStateMachineIndex].readSequ) {    //Josi: ler da sequOtima
					//garantir que a sequOtima tem tamanho suficiente para o proximo chute
					if (currentSequOtimaIndex >= (machines [currentStateMachineIndex].sequ).Length) {
						currentSequOtimaIndex = 0;
					}
							
					//result contem o proximo chute, determinado pela sequOtima lida no  arq de configuracao
					string result = (machines [currentStateMachineIndex].sequ).Substring (currentSequOtimaIndex, 1);
					if ((machines [currentStateMachineIndex].sequR).Substring (currentSequOtimaIndex, 1) == "Y") {  //170215
						ehRandomKick = true;
					} else {
						ehRandomKick = false;
					}
					currentSequOtimaIndex += 1;

					string bkpResult = result;
					transitionHistory.Insert (0, bkpResult);
					return System.Convert.ToInt16 (bkpResult);

				} else { //Josi: ler da arvore
					//Josi: calculando random apenas se necessario, na versao "ler arvore"
					string result = "0";

					if ((currentState.GetProbEvent0 () == 0) || (currentState.GetProbEvent1 () == 1)) { //jogada deterministica
						ehRandomKick = false;  
						result = (currentState.GetProbEvent0 () == 1) ? "0" : "2";
						//if (currentState.GetProbEvent0 () == 1) {result = "0";} else {result = "2";}
					} else {
						ehRandomKick = true;  

						//com base no random gerado, define onde fazer o sorteio (no evento 0 ou 2)
						float r = UnityEngine.Random.Range (0.0f, 1.0f);  //Josi: como estah float, vai gerar numeros entre 0 e 1 inclusive
						if (r <= currentState.GetProbEvent0 ()) {
							result = "0";
						} else {
							result = "2";
						} 
					}
					string bkpResult = result;

					int i = -1;

					//Josi: garantir comecar de contexto
					//      @@ Insert é menos eficiente do que Add, mas trocar vai alterar muita coisa, melhor deixar para a versao Godot
					//      @@ Também penso que a cada nivel: transitionHistory.Clear() para restartar a history...
					if (transitionHistory.Count == 0) {
						for (int j = 0; j < currentState.path.Length; j++) {
							transitionHistory.Insert (0, currentState.path [j].ToString ());
						}
					}
					//logString += " estado anterior: " + currentState.path;    //Josi: comentado

					//Josi: busca o contexto da arvore, a partir da folha em maos e juntando a folha anterior, com base na altura da arvore
					while (i < machines [currentStateMachineIndex].depth) {
						if (i >= 0)
							result = transitionHistory [i] + result;
						i++;
						if (machines [currentStateMachineIndex].states.ContainsKey (result)) {
							currentState = machines [currentStateMachineIndex].states [result];
							break;
						}
					}

					transitionHistory.Insert (0, bkpResult);
					return System.Convert.ToInt16 (bkpResult);

				} //else ler da arvore

			} //fim JG

		    //-----------------------------------------------------------------------------------------------------------
		    else {
				if (gameSelected == 5) {     //Jogo Memoria
					//result contem o chute que deveria ter sido dado, determinado pela sequ lida no  arq de configuracao
					string result = (machines [currentStateMachineIndex].mdSequ).Substring (currentMDSequIndex, 1);
					if (MDinput == int.Parse (result)) {
						currentMDSequIndex += 1;

						//garantir que a sequ tem tamanho suficiente para o proximo chute, senao, circular
						if (machines [currentStateMachineIndex].mdReadSequ) {
							if (currentMDSequIndex == (machines [currentStateMachineIndex].mdSequ).Length)
								currentMDSequIndex = 0;
						}
					} else {
						//170130 acertar numJogadas e placar
						machines [currentStateMachineIndex].mdLimitPlays = machines [currentStateMachineIndex].mdLimitPlays + 1;
					}

					//nao precisaria montar o historico na firstScreen, mas para nao criar uma instancia do gameFlow
					string bkpResult = result;                          //170105 atualizar historico de chutes
					transitionHistory.Insert (0, bkpResult);

					return System.Convert.ToInt16 (result);
				}  //fim JM
			} //fim else JG
		} //fim else AQ contentar o compilador unity que percebe que pode haver saída sem return
		return 0; //QG para
	} //2choices


	//------------------------------------------------------------------------------------
	//Josi reescrito para coletar a sequ a partir de uma string
	//     161205: ler da stringOtima ou da arvore (cf parametro)
	//     161209: base Motora (vLudmila)
	//     161219: base Motora com tempo (vMElisa)
	//     170130: param MDinput para saber a direcao selecionada pelo jogador no MD, e se errada, manter-se no idx ate que acerte (vAF)
	//     170216: param phaseZeroJG para saber se estamos na phaseZero (experimental, sem historico) do JG
	int ThreeChoices(int gameSelected, int MDinput)
	{ 
		if (gameSelected == 1)      //BM
		{
		    //Josi: ler da sequ dada ou gerada - ambas guardadas aqui
			//result contem o chute que deveria ter sido dado, determinado pela sequ lida no  arq de configuracao, ou gerada
			string result = (machines [currentStateMachineIndex].bmSequ).Substring (currentBMSequIndex, 1);
			currentBMSequIndex += 1;

			//garantir que a sequ tem tamanho suficiente para o proximo chute
			if (currentBMSequIndex == (machines [currentStateMachineIndex].bmSequ).Length)
				currentBMSequIndex = 0;
		
			return System.Convert.ToInt16 (result);
		} //fim BM

//		//-----------------------------------------------------------------------------------------------------------
		else
		{
			if (gameSelected == 2)
			{   //JG
				if (machines [currentStateMachineIndex].readSequ) {    //Josi: ler da sequOtima
					//garantir que a sequOtima tem tamanho suficiente para o proximo chute
					if (currentSequOtimaIndex >= (machines [currentStateMachineIndex].sequ).Length) {
						currentSequOtimaIndex = 0;
					}
			

					//result contem o proximo chute, determinado pela sequOtima lida no  arq de configuracao
					string result = (machines [currentStateMachineIndex].sequ).Substring (currentSequOtimaIndex, 1);
					if ((machines [currentStateMachineIndex].sequR).Substring (currentSequOtimaIndex, 1) == "Y") {  //170215
						ehRandomKick = true;
					} else {
						ehRandomKick = false;
					}
					currentSequOtimaIndex += 1;

					string bkpResult = result;

					transitionHistory.Insert (0, bkpResult);
					return System.Convert.ToInt16 (bkpResult);

				} else { //Josi: ler da arvore
					//Josi: calculando random apenas se necessario, na versao "ler arvore"
					string result = "0";

					if (((currentState.GetProbEvent0 () == 0) || (currentState.GetProbEvent0 () == 1)) &&
					     (currentState.GetProbEvent1 () == 0) || (currentState.GetProbEvent1 () == 1)) {   //jogada deterministica
						ehRandomKick = false;  //170215

						if (currentState.GetProbEvent0 () == 1) {
							result = "0";
						} else if (currentState.GetProbEvent1 () == 1) {
							result = "1";
						} else {
							result = "2";
						}
					} else {
						//Josi: jogada probabilistica - já estava assim
						//      com base no random gerado, define onde fazer o sorteio (no evento 0, 1 ou 2)
						float r = UnityEngine.Random.Range (0.0f, 1.0f);  //Josi: como estah float, vai gerar numeros entre 0 e 1 inclusive: https://docs.unity3d.com/ScriptReference/Random.Range.html

						ehRandomKick = true;  //170215

						//gerando
						//print(">>>> Random:"+r);
						result = "0";

                        if (r > currentState.GetProbEvent0() && r < currentState.GetProbEvent0() + currentState.GetProbEvent1()) {
                            result = "1";
                        } else {
                            if (r >= currentState.GetProbEvent0() + currentState.GetProbEvent1()) {
                                result = "2";
                            }
                        }
					}
					string bkpResult = result;
				
					int i = -1;

					//Josi: garantir comecar de contexto
					//      @@ Insert é menos eficiente do que Add, mas trocar vai alterar muita coisa, melhor deixar para a versao Godot
					//      @@ Também penso que a cada nivel: transitionHistory.Clear() para restartar a history...
					if (transitionHistory.Count == 0) {
						for (int j = 0; j < currentState.path.Length; j++) {
							transitionHistory.Insert (0, currentState.path [j].ToString ());
						}
					}
					//logString += " estado anterior: " + currentState.path;    //Josi: comentado

					//Josi: busca o contexto da arvore, a partir da folha em maos e juntando a folha anterior, com base na altura da arvore
					while (i < machines [currentStateMachineIndex].depth) {
						if (i >= 0)
							result = transitionHistory [i] + result;
						i++;
						if (machines [currentStateMachineIndex].states.ContainsKey (result)) {
							currentState = machines [currentStateMachineIndex].states [result];
							break;
						}
					}
		
					//logString += " estado atual: "+currentState.path+" resultado: "+ bkpResult +"\n";    //Josi: comentado	
					//print ("Estado atual: "+currentState.path);                                          //Josi: comentado
		
					transitionHistory.Insert (0, bkpResult);
				
					return System.Convert.ToInt16 (bkpResult);
				} //else ler da arvore
			} //fim JG

//		//-----------------------------------------------------------------------------------------------------------
			else 
			{
				if (gameSelected == 3)      //MD
				{
					//result contem o chute que deveria ter sido dado, determinado pela sequ lida no  arq de configuracao
					string result = (machines [currentStateMachineIndex].mdSequ).Substring (currentMDSequIndex, 1);
					currentMDSequIndex += 1;

					//garantir que a sequ tem tamanho suficiente para o proximo chute
					if (currentMDSequIndex == (machines [currentStateMachineIndex].mdSequ).Length)
						currentMDSequIndex = 0;

					//nao precisaria montar o historico na firstScreen, mas para nao criar uma instancia do gameFlow
					string bkpResult = result;                            //170105 atualizar historico de chutes
					transitionHistory.Insert (0, bkpResult);

					return System.Convert.ToInt16 (result);
				} //fim MemoDecl

//		//-----------------------------------------------------------------------------------------------------------
				else 
				{
					if (gameSelected == 4) {      //BM com tempo
						//Josi: ler da sequ dada ou gerada - ambas guardadas aqui
						//result contem o chute que deveria ter sido dado, determinado pela sequ lida no  arq de configuracao
						string result = (machines [currentStateMachineIndex].bmSequ).Substring (currentBMSequIndex, 1);
						currentBMSequIndex += 1;

						//garantir que a sequ tem tamanho suficiente para o proximo chute
						if (currentBMSequIndex == (machines [currentStateMachineIndex].bmSequ).Length)
							currentBMSequIndex = 0;

						return System.Convert.ToInt16 (result);
					} //fim BM com tempo
					else {
						if (gameSelected == 5)      //Jogo Memoria
						{
							//result contem o chute que deveria ter sido dado, determinado pela sequ lida no  arq de configuracao
							string result = (machines [currentStateMachineIndex].mdSequ).Substring (currentMDSequIndex, 1);
							if (MDinput == int.Parse(result)) {
								currentMDSequIndex += 1;

								//garantir que a sequ tem tamanho suficiente para o proximo chute, senao, circular
								//180326 with incremented plays, necessary to verify for both, readSequ or not
								//if (machines [currentStateMachineIndex].mdReadSequ) {
								if (currentMDSequIndex == (machines [currentStateMachineIndex].mdSequ).Length) {
									currentMDSequIndex = 0;
								}
								//}
							} else {
								//170130 acertar numJogadas e placar
								machines [currentStateMachineIndex].mdLimitPlays = machines [currentStateMachineIndex].mdLimitPlays + 1;
							}

							//nao precisaria montar o historico na firstScreen, mas para nao criar uma instancia do gameFlow
							string bkpResult = result;                            //170105 atualizar historico de chutes
							transitionHistory.Insert (0, bkpResult);

							return System.Convert.ToInt16 (result);
						} //fim MemoDecl
					}
				} //fim else MD
			} //fim else JG
		} //fim else BM

		return 0;   //Josi do céu, que coisa horrível... o compilador reconhece que há chance de nao entrar em nenhum if e ficar sem return...
	} //fim ThreeChoices


	//------------------------------------------------------------------------------------
	//170130 chamada de uiManager.btnActionGetEvent; agora tem como param a direcao selecionada pelo jogador para nao precisar instanciar uiManager
	//170216 agora inclui saber se está no JG na phaseZero
	//171109 implementar 2choices
	public int GetEvent (int MDinput)
	{
		if(machines[currentStateMachineIndex].choices == 2)
		{
			return TwoChoices(PlayerPrefs.GetInt ("gameSelected"), MDinput);
		}
		else if(machines[currentStateMachineIndex].choices == 3)
		{
//			return ThreeCoices();        //Josi: deixa de valer a versao que le a arvore e escolhe o proximo chute
			return ThreeChoices(PlayerPrefs.GetInt ("gameSelected"), MDinput);    //Josi: passa a valer a versao que também lê uma sequOtima de chutes
		}		                                                                               //170216: incluir param que define se JG fases normais ou fase0 experimental sem historico
		return 0; //QG:só eh possivel haver 2 ou 3 choices (poderá haver 4), mas o compilador unity percebe que pode haver uma saida sem return
	}


	//--------------------------------------------------------------------------------------
	public void GotoNextMachine()
	{
		//Random.InitState(42);  //manter apenas um init
	 	currentStateMachineIndex ++;

		currentSequOtimaIndex = 0;       //Josi: iniciar a sequOtima
		currentBMSequIndex = 0;          //Josi: 161209
		currentMDSequIndex = 0;          //Josi: 161212: pointer da sequ no MD
		currentJGSequIndex = 0;          //170216 idx para phase0 do JG (melhor manter dois ponteiros para fase0 e demais...)
	}


	//------------------------------------------------------------------------------------
	public void ResetToInitialMachine()
	{
		currentStateMachineIndex = 0;
		currentSequOtimaIndex = 0;       //Josi: iniciar a sequOtima no JG
		currentBMSequIndex = 0;          //Josi: 161209: pointer da sequ no BM
		currentMDSequIndex = 0;          //Josi: 161212: pointer da sequ no MD
		currentJGSequIndex = 0;          //170216 idx para phase0 do JG (melhor manter dois ponteiros para fase0 e demais...)
	}


	//------------------------------------------------------------------------------------
	public bool CanGoToNextMachine()
	{
		if (PlayerPrefs.GetInt ("gameSelected") != 2) //170106 se BM(1), BMtempo(4) ou MD(3), todos tem apenas uma fase: notCanGo
			return false;
		
		if (currentStateMachineIndex < machines.Count-1) 
			return true;
	
		return false;	
	}



	//------------------------------------------------------------------------------------
	//Josi: ninguem chama este trecho; atencao: chama-se GotoNextMachine (nao GoTo)
	public bool GoToNextMachine()
	{
		if(currentStateMachineIndex < machines.Count) {
			currentStateMachineIndex ++;
			currentSequOtimaIndex = 0;       //Josi: iniciar a sequOtima
			return true;
		}		
		return false;
	}



	//------------------------------------------------------------------------------------
	//Josi: 161209: devolver, pela BMsequ, o valor do proximo chute a indicar
	//            QG: mesmo se sequ gerada, esta foi arquivada  em machines.bmSequ
	public string GetNextKick()
	{
		return (machines [currentStateMachineIndex].bmSequ).Substring (currentBMSequIndex, 1);
	}



	//------------------------------------------------------------------------------------
	//Josi 161229 definir a sequ para BM
	//            QG: sobrepor sobre a machines.bmSequ para nao alterar o restante do sistema
	//170126: param firstGeneration: primeira geracao de sequencia de size limitPlays
	//              jogadasAacertar: sequ precisa crescer o tanto de jogadas que faltam para atingir o minimo requerido (bmMinHits)
	//171109: consider 2 or 3 choices
	public string defineBMSequ(bool firstGeneration, int jogadasAacertar)
	{
		if (!machines [currentStateMachineIndex].bmReadSequ) {  //eh para gerar a sequencia, nao ler a sequ pronta
			int i;
			StringBuilder sequ = new StringBuilder (machines [currentStateMachineIndex].bmLimitPlays - 1);

			if (firstGeneration) {
				i = 0;
				sequ.Length = 0;
			} else {
				i = machines [currentStateMachineIndex].bmLimitPlays - jogadasAacertar;
				sequ.Insert (0, machines [currentStateMachineIndex].bmSequ);
				sequ.Length = machines [currentStateMachineIndex].bmSequ.Length;
			}

			int choices = getChoices ();                           //171109
			while (i < machines [currentStateMachineIndex].bmLimitPlays) {
				sequ.Append(getRandomSequ(choices));               //171109 criada function para devolver choice
				if (i > 1) {
					if (choices == 3) {
						//direção não pode se repetir por 3x seguidas
						while ((sequ [i - 1] == sequ [i]) && (sequ [i - 2] == sequ [i])) { 
							sequ.Length = i;
							sequ.Append (getRandomSequ(choices));  //171109 criada function para devolver choice
						}
					}  //por enquanto, sem else para 2choices
				}
			i++;
			}
			machines [currentStateMachineIndex].bmSequ = sequ.ToString();
		}
		return machines [currentStateMachineIndex].bmSequ;
	}



	//------------------------------------------------------------------------------------
	//Josi 161229 definir a sequ para MD
	//            QG: sobrepor sobre a machines.mdSequ para nao alterar o restante do sistema
	//171110: consider 2 or 3 choices
	public string defineMDSequ()
	{
		if (!machines [currentStateMachineIndex].mdReadSequ) {  //eh para gerar a sequencia, nao ler a sequ pronta
			int i = 0;
			StringBuilder sequ = new StringBuilder (machines [currentStateMachineIndex].mdLimitPlays - 1); //170102 melhorar isto e ler parametros
			sequ.Length = 0;

			int choices = getChoices ();   //171110
			while (i < machines [currentStateMachineIndex].mdLimitPlays) {  
				sequ.Append (getRandomSequ(choices));          //171110 criada function para devolver choice
				if (i > 1) {
					if (choices == 3) {
						//direção não pode se repetir por 3x seguidas
						while ((sequ [i - 1] == sequ [i]) && (sequ [i - 2] == sequ [i])) { 
							sequ.Length = i;
							sequ.Append (getRandomSequ (choices));  //171110 criada function para devolver choice
						}
					} //por enquanto, sem else para 2choices
				}
				i++;
			}
			//170205 IMEjr: na fase3, DDED ou DEDD vao circular e gerar 3 simbolos iguais; eu nao sei se isso era uma requisicao - bastava na sequ...
			if (choices == 3) {
				//direção não pode se repetir por 3x seguidas mesmo ao circular
				while (((sequ [i - 1] == sequ [0]) && (sequ [i - 1] == sequ [1])) ||
				      ((sequ [i - 1] == sequ [0]) && (sequ [i - 1] == sequ [i - 2]))) { 
					sequ.Length = i - 1;
					sequ.Append (getRandomSequ (choices));  //171110 criada function para devolver choice
				}
			}
			machines [currentStateMachineIndex].mdSequ = sequ.ToString ();
		}
	return machines [currentStateMachineIndex].mdSequ;
	}



	//------------------------------------------------------------------------------------
	//170102 para retornar a sequ MD (dada ou gerada, ambas arquivadas aqui)
	public string getMDsequ()
	{
		return machines[currentStateMachineIndex].mdSequ;
	}


	//------------------------------------------------------------------------------------
	//170108 QG repor valores originais das sequs, caso user jogue mais de uma vez
	public void resetOriginalData()
	{
		if (machines [currentStateMachineIndex].bmSequ != saveOriginalBMsequ) {
			machines [currentStateMachineIndex].bmSequ = saveOriginalBMsequ;
			machines [currentStateMachineIndex].bmLimitPlays = saveOriginalBMnumPlays;
		}
		if (machines [currentStateMachineIndex].mdSequ != saveOriginalMDsequ) {
			machines [currentStateMachineIndex].mdSequ = saveOriginalMDsequ;
			machines [currentStateMachineIndex].mdLimitPlays = saveOriginalMDnumPlays;
		}
	}



	//------------------------------------------------------------------------------------
	//170104 para carregar a sequencia a jogar na fase 3 (unica) do MD: a mesma da firstScreen mas repetida 3x
	public void setupMDparaJG ()
	{   //nao achei funcao rep/dup ou algo assim
		machines [currentStateMachineIndex].mdSequ = String.Concat (machines [currentStateMachineIndex].mdSequ,    
				                                                    machines [currentStateMachineIndex].mdSequ, 
				                                                    machines [currentStateMachineIndex].mdSequ);
		machines [currentStateMachineIndex].mdLimitPlays = machines [currentStateMachineIndex].mdSequ.Length;
	}



	//------------------------------------------------------------------------------------
	public string CurrentMachineID()
	{
		return machines [currentStateMachineIndex].id;
	}


	//------------------------------------------------------------------------------------
	//170124 ler parametro bmMinHits no Base Motora com ou sem tempo (Aquecimento)
	public int getMinHits()
	{
		return machines [currentStateMachineIndex].bmMinHits;
	}


	//------------------------------------------------------------------------------------
	//170921 ler parametro bmMinHits no Base Motora com ou sem tempo (Aquecimento)
	public int getMinHitsInSequence()
	{
		return machines [currentStateMachineIndex].bmMinHitsInSequence;
	}


	//------------------------------------------------------------------------------------
	//170124 ler parametro bmMaxPlays no Base Motora com ou sem tempo (Aquecimento)
	public int getBmMaxPlays()
	{
		return machines [currentStateMachineIndex].bmMaxPlays;
	}


	//------------------------------------------------------------------------------------
	//170124 somar um ao numero de jogadas do Base Motora - até atingir o minimo de jogadas corretas
	public void sumNumPlays(int jogadasAacertar)
	{
		machines [currentStateMachineIndex].bmLimitPlays = machines [currentStateMachineIndex].bmLimitPlays + jogadasAacertar;
	}



	//------------------------------------------------------------------------------------
	//Josi: 161212: acrescentado parametro para devolver numJogadas dependendo do jogo selecionado
	//      170216: no JG pode haver a phase0, experimental, com sequ propria e limite proprio, definido no arquivo de configuracao
	public int GetCurrentPlayLimit(int gameSelected)
	{
		int limit = 0;
		switch (gameSelected)
		{
		case 1:     //BM
			limit = machines [currentStateMachineIndex].bmLimitPlays;
			break;
		case 2:     //JG
			limit = machines [currentStateMachineIndex].limitPlays;
			break;
		case 3:     //MD (input)
			limit = machines [currentStateMachineIndex].mdSequ.Length;
			break;
		case 4:     //BMcomTempo = BM: muda apenas a operacao
			limit = machines [currentStateMachineIndex].bmLimitPlays;
			break;
		case 5:     //Jogo da memoria (declarado ao experimentador)
			//limit = machines [currentStateMachineIndex].mdLimitPlays;
			limit = machines [currentStateMachineIndex].mdMaxPlays; //180323 max limit plays to achieve the assymptote
			break;
		}
		return limit;
	}


	//------------------------------------------------------------------------------------
	//Josi: 161205 para gravar no arquivo/webserver o modo de operacao do jogo, 
	//             que tanto joga lendo uma sequencia otima como lendo a arvore, por random quando necessario
	//      170126 gravar para todos os jogos
	public bool getCurrentReadSequ(int gameSelected)
	{
		if  (gameSelected == 2)                                           //JGoleiro - mais provavel
			return machines [currentStateMachineIndex].readSequ;
		if ((gameSelected == 1) || (gameSelected == 4))                  //BM ou BMt: aquecimto ou aquecto com tempo
			return machines [currentStateMachineIndex].bmReadSequ;
		if ((gameSelected == 3) || (gameSelected == 5))                  //MD/Base Memoria (3) ou Jogo da Memória (5)
			return machines [currentStateMachineIndex].mdReadSequ;		
		return false;                                                    //QG para atender ao unity que nao garante que os ifs englobam todas as possibilidades
	}


	//------------------------------------------------------------------------------------
	//Josi: ninguem chama este trecho
	void SetInitState(JsonInput t)
	{
		int max = t.states.Length;
		int r = UnityEngine.Random.Range(0, max);

		currentState = t.states[r];
	}


	//------------------------------------------------------------------------------------
	static bool inited = false;	
	public void Start () 
	{
		//Josi ** sai daqui e entra para o trecho do "nao iniciado"; irah entrar novamente com a variavel somada
		//currentStateMachineIndex = 0;
		StateMachine tmp;
		if (!inited) {

            //180614 need this case player backToMenuTeams
            machines.Clear();


            //Josi ** entra para o trecho do "nao iniciado"; irah entrar novamente com a variavel somada
            currentStateMachineIndex = 0;

			currentSequOtimaIndex = 0; //Josi: ponteiro que le a sequ otima no JG
			currentBMSequIndex = 0;    //Josi: ponteiro que le a sequ de chutes na BM
			currentMDSequIndex = 0;    //Josi: 161212: pointer para a sequ do memoriaDeclarativa
			currentJGSequIndex = 0;    //170216

			// carrega as árvores caso ainda não tenham sido carregadas
			if (LoadedPackage.loaded == null)
				LoadStages.LoadTreePackageFromResources ();

			int i = 0;
			foreach (string s in LoadedPackage.loaded.stages) {
				//se eh o primeiro arquivo (tem param menus) e ainda nao detectou IncompatibleVersion
				//170921 new params for september/17 version
				//170922 if  there is a missing parameter in some file, error
				//180104 param portEEGserial after v180102
				//180326 param minHitsInSequence for JG + mdMaxPlays to stop JM in some play
				//180328 param user input defense keys
				//180402 param alternative for playPause button
				//180410 param attentionPoint and attentionDiameter
				//180413 param speedGKAnim
				if ((i == 0) && !configFileIncompatibleWithVersion) {   //detected error in other config file
					if (s.Contains ("limitValue") || s.Contains ("zeroPhaseJG")) {  //180328 params removed (obsolete)
						configFileIncompatibleWithVersion = true;
					} else {
						if (!(s.Contains ("menus") && s.Contains ("showHistory")
						    && s.Contains ("sendMarkersToEEG") && s.Contains ("portEEGserial")
						    && s.Contains ("groupCode") && s.Contains ("showPlayPauseButton")
						    && s.Contains ("bmMinHitsInSequence") && s.Contains ("bmMaxPlays") && s.Contains ("mdMaxPlays")
						    && s.Contains ("minHitsInSequence") && s.Contains ("leftInputKey")
						    && s.Contains ("pausePlayInputKey") && s.Contains ("institution") && s.Contains ("attentionDiameter")
							&& s.Contains ("timeFaixa0") && s.Contains ("timeFaixa1") && s.Contains ("timeFaixa2")
							&& s.Contains ("timeFaixa3") && s.Contains ("timeFaixa4")
							&& s.Contains ("speedGKAnim") && s.Contains ("portSendData"))) {
							configFileIncompatibleWithVersion = true;
						}
					}
				}
				i++;
					
				tmp = LoadJson (s);
				if (tmp != null) {
					machines.Add (tmp);
				}
			}

			//inited = true;           //180614 now, it is possible backToMenuTeams
            currentSequOtimaIndex = 0; //Josi garantir ponteiro da sequOtima
			currentBMSequIndex = 0;    //Josi: ponteiro que le a sequ de chutes na BM
			currentMDSequIndex = 0;    //Josi: 161212: pointer para a sequ do memo Declarativa

			//170108 QG como estah na mesma variavel, salvar para repor, caso user jogue mais de uma vez
			saveOriginalBMnumPlays = machines [currentStateMachineIndex].bmLimitPlays;
			saveOriginalBMsequ = machines [currentStateMachineIndex].bmSequ;
			saveOriginalMDnumPlays = machines [currentStateMachineIndex].mdLimitPlays;
			saveOriginalMDsequ = machines [currentStateMachineIndex].mdSequ;
		}

		//Josi: todo o trecho abaixo estaria melhor em outro ponto do programa, onde se soubesse qual jogo será selecionado
		//      dado que se refere especificamente ao JG no modo ler árvore - como tenho prazo e nao atrapalha, fica ai
		int max = machines[currentStateMachineIndex].states.Count;

		// Define o início da sequência aleatória.  Sempre fixa.
		//Random.InitState(42);  //manter apenas uma, no loadStages
		int index = UnityEngine.Random.Range(0, max);

		string key = machines[currentStateMachineIndex].dicKeys[index];
		currentState = machines[currentStateMachineIndex].states[key];

	}



	//------------------------------------------------------------------------------------
	//170310 verificacoes no arquivo de conf
	//171009 usando potencias de 2 -> colocar para cima novos erros
	public int configValidation() 
	{   int errorNumber = 0;

		//180105: erro 2^6 (64) - sendMarkersToEEG not more a boolean 
		if (machines [0].sendMarkersToEEG.ToLower () == "true" || machines [0].sendMarkersToEEG.ToLower () == "false") {
			errorNumber = errorNumber + 64;  //accepted values are serial, parallel or none
		} else {
			//180105: erro 2^5 (32) - if serial should to exist portEEGserial
			if (machines [0].sendMarkersToEEG.ToLower () == "serial") {
				//180202 if not have COM or not have tty, wrong!
				if (!(machines [0].portEEGserial.Contains("COM") || machines [0].portEEGserial.Contains("tty")) ) {
				    errorNumber = errorNumber + 32;  //if JG should send markers to EEG using serial port, then serial port can not be null
				}                                    // and should be something like COM? ou tty?
			}
		}
			
		//171009: erro 2^4 (16) - ver se ha pelo menos um menu a apresentar
		if (machines [0].menuList != null) {
			bool existeUmItem = false;
			int i = 0;
			while (i < machines [0].menuList.Count) { 
				if (machines [0].menuList [i].sequMenu == 1) {  //encontrado um item 1
					if (existeUmItem) {
						i = machines [0].menuList.Count;        //encontrados dois itens 1
						existeUmItem = false;
					} else {
						existeUmItem = true;
					}
				}
				i = i + 1;
			}
			if (!existeUmItem) {
				errorNumber = errorNumber + 16;  //falta pelo menos 1 item no menu
			}
		} else {
			errorNumber = errorNumber + 16;  //falta o param menus
		}

		// validar nomes diferentes de ID
		int files = machines.Count;
		for (int i = 0; i < files; i++) {   //se apenas um file, nao havera duplicacao
			for (int j = i + 1; j < files; j++) {
				if (machines [i].id == machines [j].id) {
					errorNumber = errorNumber + 8;       //nome indicado em ID estah repetido e deve ser chave única
				}
			}
		}

		//120623 validar compatibilidade dos parametros com versao rodando
		//2: existir param showHistory, sendMarkersToEEG etc (verificado no carregamento do JSon)
		if (configFileIncompatibleWithVersion) {
			errorNumber = errorNumber + 4;         //nao existe algum dos novos parametros: versão incompativel com arquivo de config
		} 

		// verificar se ambiente eh compativel com a indicacao de querer marcador EEG
		//180103 values permitted (serial|parallel|none)
		//180209 not worked send markers to virtual serial on linux; come back valid only in Windows
		if ((machines [0].sendMarkersToEEG.ToLower () == "parallel") || (machines [0].sendMarkersToEEG.ToLower () == "serial")) {

			if ((Application.platform != RuntimePlatform.WindowsPlayer) && (Application.platform != RuntimePlatform.WindowsEditor)) {
				errorNumber = errorNumber + 2;      // existe sendMarkersToEEG mas estah true em um ambiente nao permitido (não Windows)
			} else {
				//take care: here we do not have the game name to play...
				//170627 verifica se o ambiente eh 32 bits (pelos tempos de exec dos Starts nao da para ser public)
				//171017 agora vale para 32 e 64bits - em 64bits nao foi testado
				//
				//bool isWindows32bits = SystemInfo.operatingSystem.Contains ("64bit") ? false : true;
				//if (!isWindows32bits) {  
				//	errorNumber = errorNumber + 2;  // existe sendMarkersToEEG mas estah true em um ambiente nao permitido (não Windows 32bits)
				//}
			}
		} 
			
		return errorNumber;  
	}



	//-------------------------------------------------------------------------------------
	public int GetCurrMachineIndex() {
		return currentStateMachineIndex;
	}



	//-------------------------------------------------------------------------------------
	//170412 capturar o tipo de placar final escolhido pelo experimntador
	public string getCurrentFinalScoreboard() {
		return machines [currentStateMachineIndex].finalScoreboard;
	}


	//-------------------------------------------------------------------------------------
	//170215 devolver o tipo de animacao selecionado: long, short ou none
	//170217 um param para o JG e outro para os demais jogos (ate agora: Aquecto, Aquecto com tempo, Memoria)
	public string getCurrentAnimationType() {
		if (PlayerPrefs.GetInt ("gameSelected") == 2) 
			return machines [currentStateMachineIndex].animationTypeJG.ToLower().Trim();

		return machines [currentStateMachineIndex].animationTypeOthers.ToLower().Trim();
	}


	//-------------------------------------------------------------------------------------
	//170322 devolver o tempo da animacao correspondente (long, short ou none) que espera que visual e sonora tenham os mesmos tempos;
	//       talvez fosse melhor criar constantes...
	public float animationTime() {
		//170412 tempos do animation   e do sound
		//perdeu         2.0           lament            0.5
		//perdeuShort    0.40          Ahh-sound         0.1
		//defendeu       2.0           cheer             0.6
		//defendeuShort  0.40          cheerTomlijaMono  0.1

		//170418 ATENCAO: lembrar de acrescentar a diferenca de tempo - 
		//                trocar pelos comandos que calculam o tempo faltante
		if (getCurrentAnimationType () == "long")   return 2.4f;   //2.6  
		if (getCurrentAnimationType () == "short")  return 1.4f;   //1.6
		return 0.2f;                                               //0.4
	}



	//-------------------------------------------------------------------------------------
	//170215 devolver seeh para mostrar ou nao o contador do canto direito
	public bool getCurrentScoreboard() {
		return machines [currentStateMachineIndex].scoreboard;
	}


	//-------------------------------------------------------------------------------------
	//170622 devolver se é para mostrar ou não, na fase corrente do JG, o historico
	public bool getCurrentShowHistory() {
		return machines [currentStateMachineIndex].showHistory;
	}


	//-------------------------------------------------------------------------------------
	//170623 devolver se é para enviar ou não, marcadores para o EEG; it is unique for all games played
	//180104 can now contains parallel, serial or none - not more a boolean function
	public string getSendMarkersToEEG() {
		return machines [0].sendMarkersToEEG.ToLower();
	}


	//-------------------------------------------------------------------------------------
	//180104 devolver a porta serial COMx conectada ao EEG
	public string getPortEEGserial() {
		return machines [0].portEEGserial;
	}

	//-------------------------------------------------------------------------------------
	public string getPortSendData() {
		return machines [0].portSendData;
	}

	//-------------------------------------------------------------------------------------
	public float getTimeFaixa0() {
		return float.Parse(machines [0].timeFaixa0, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
	}

	//-------------------------------------------------------------------------------------
	public float getTimeFaixa1() {
		return float.Parse(machines [0].timeFaixa1, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
	}

	//-------------------------------------------------------------------------------------
	public float getTimeFaixa2() {
		return float.Parse(machines [0].timeFaixa2, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
	}

	//-------------------------------------------------------------------------------------
	public float getTimeFaixa3() {
		return float.Parse(machines [0].timeFaixa3, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
	}

	//-------------------------------------------------------------------------------------
	public float getTimeFaixa4() {
		return float.Parse(machines [0].timeFaixa4, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
	}

	//-------------------------------------------------------------------------------------
	//170222 devolver número de jogadas para descanso
	public int getPlaysToStartRelax() {
		return machines [currentStateMachineIndex].playsToRelax;
	}


	//-------------------------------------------------------------------------------------
	//170918 devolver se é para mostrar botões Play/Pause ou nao;
	//       este param só existe no primeiro arquivo e vale por todo o jogo
	public bool getShowPlayPauseButton() {
		return machines [0].showPlayPauseButton;
	}


	//-------------------------------------------------------------------------------------
	//180320 return numMax plays before to go to gameOver screen in JM
	public int getMDMaxPlays() {
		return machines [0].mdMaxPlays;
	}


    //-------------------------------------------------------------------------------------
	//180320 return asymptote plays (number of plays in sequence that defines the player recognizes the sequences)
	public int getJGminHitsInSequence() {
		return machines [currentStateMachineIndex].minHitsInSequence;
	}


    //-------------------------------------------------------------------------------------
	//180322 return minHitsInSequence for Memory module (assymptote)
	public int getMDminHitsInSequence() {
		return machines [currentStateMachineIndex].mdMinHitsInSequence;
	}


    //-------------------------------------------------------------------------------------
	//170417 montar string para apresentar no arquivo de resultados;
	//       tree=tree="context;prob0;prob1 | context;prob0;prob1 | ...
	public string stringTree()
	{
		string treeContextsAndProbabilities = "";
		for (int i = 0; i < machines [currentStateMachineIndex].dicKeys.Count; i++) {
			if (i > 0) {
				treeContextsAndProbabilities = treeContextsAndProbabilities + " | ";
			}
			treeContextsAndProbabilities = treeContextsAndProbabilities 
			    + machines [currentStateMachineIndex].dicKeys [i] + ";"
				+ machines [currentStateMachineIndex].states [machines [currentStateMachineIndex].dicKeys [i]].probEvent0 + ";"
				+ machines [currentStateMachineIndex].states [machines [currentStateMachineIndex].dicKeys [i]].probEvent1;	
		}
		return treeContextsAndProbabilities;
	}


    //-------------------------------------------------------------------------------------
    //171109 pegar o valor de choices nesta fase
	public int getChoices()
	{
		return machines [currentStateMachineIndex].choices;
	}


    //-------------------------------------------------------------------------------------
	//171109 devolve uma direção de chute cf o número de opções
	//       2: devolve 0 ou 2
	//       3: devolve 0, 1 ou 2
	public string getRandomSequ(int choices) {
		string randomDirection;
		randomDirection = UnityEngine.Random.Range (0, choices).ToString ();
		if ((choices == 2) && (randomDirection == "1"))
			randomDirection = "2";
		return randomDirection;
	}


	//-------------------------------------------------------------------------------------
	//180328 return the user defined key to accept defense at left (0), center (1) or right (2)
	public KeyCode acceptedKey(int direction) {
		if (direction == 0) {
			return (KeyCode)Enum.Parse(typeof(KeyCode), machines [0].leftInputKey);  //syntax to convert string into keycode
		} else {
			if (direction == 1) {
				return (KeyCode)Enum.Parse(typeof(KeyCode), machines [0].centerInputKey);
			} else {
				return (KeyCode)Enum.Parse(typeof(KeyCode), machines [0].rightInputKey);
			}
		}
	}



	//-------------------------------------------------------------------------------------
	//180402 keyboard alternative for playPause button
	public KeyCode playPauseKey() {
		return (KeyCode)Enum.Parse(typeof(KeyCode), machines[0].pausePlayInputKey);  //syntax to convert string into keycode
	}


	//------------------------------------------------------------------------------------
	//180410 attentionPoint active or not
	public bool attentionPointActive() {
		return machines [0].attentionPoint;
	}

	//------------------------------------------------------------------------------------
	public float attentionDiameter() {
		return float.Parse(machines [0].attentionDiameter, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
	}

	//------------------------------------------------------------------------------------
	public Color attentionColors(int what) {
		//if (what == 0) return string2Color (machines [0].attentionColorStart);
		//if (what == 1) return string2Color (machines [0].attentionColorCorrect);
		//return string2Color (machines [0].attentionColorWrong);

		Color paramColor = new Color();
		if (what == 0) {
			ColorUtility.TryParseHtmlString (machines [0].attentionColorStart, out paramColor);
		} else {
			if (what == 1) {
				ColorUtility.TryParseHtmlString (machines [0].attentionColorCorrect, out paramColor);
			} else {
				ColorUtility.TryParseHtmlString (machines [0].attentionColorWrong, out paramColor);
			}
		}
		return paramColor;
	}


	//------------------------------------------------------------------------------------
	//180413 to define animation player/ball/goalkeeper speed, for phase!
	//need to change to string because decimal separator depending on culture settings (locale): dot or comma
	public float speedGKAnim(int idx) {
		if (machines [idx].speedGKAnim == null) {
			return 1.0f;
		} else {
			return float.Parse (machines [idx].speedGKAnim, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}
	}

}
