/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// Rewrited by Josi Perez <josiperez.neuromat@gmail.com>
//
// This Module is a holder class that abstract the json containing the state machine
// representation of a tree
/************************************************************************************/

using System;


public class JsonInput
{
	public string id;
	public string limitPlays;
	public string choices;
	public string depth;
	public bool readSequ;               //Josi: o script deve ler a sequenciaOtima ou gerar arvore randomica no JG
	public string sequ;   				//Josi: sequencia otima para a arvore, ou quase otima, no JG
	public string sequR;                //170214: tripa com a indicacao de posicoes randomicas (Y) ou não (n) na sequ manual
	public string bmSequ;               //Josi: sequencia a indicar no BM 
	public int bmLimitPlays;            //Josi: numero de vezes a jogar a bmSequ no BM
	public bool bmReadSequ;             //Josi 161227 no BM ler sequencia ou gerar sequencia?
	public int bmMinHits;				//Josi: 170124 nao avancar enquanto minHits nao tenha sido atingido
	public string mdSequ;               //Josi: sequencia a indicar no MD (atual Base memoria)
	public int mdLimitPlays;            //Josi: numero de vezes a jogar a mdSequ no MD
	public bool mdReadSequ;             //Josi 161227 no MD ler sequencia ou gerar sequencia?
	public string animationTypeJG;      //170214: LONG (3s), SHORT(1s), NONE(0s) das animacoes defendeu/perdeu+som
	public string animationTypeOthers;  //170217: LONG (3s), SHORT(1s), NONE(0s) das animacoes defendeu/perdeu+som
	public bool scoreboard;             //170214: true | false, para colocar ou não o placar no JG (na fase)
	public string finalScoreboard;      //170412: long (com porcentagem), short (acertos/jogadas), none
	public int playsToRelax;            //170215: número de jogadas onde o programa dá uma parada para que o experimento faca uma pausa
	public bool showHistory;			//170622: mostrar ou não o historico de andamento das 8 ultimas jogadas
	public string sendMarkersToEEG;     //170623: bool: enviar ou nao, marcadores para o EEG paralelo 0x378 em windows 32bits
	                                    //180103: passa a receber string: serial|parallel|none
	public string portEEGserial;        //180103: if sendMarkers = serial, send to this port; format COMx
	public string groupCode;            //180403: NES synchronyzation; 170629: to identify registries from an experiment (old researchGroup)
	public bool showPlayPauseButton;    //170918: iniciar ou não, com o jogo em Pausa, para explicações do experimentador ao jogador
	                                    //        virou o botão "Continuar com pausa" além do Continuar, no cataApelido
	public int bmMinHitsInSequence;     //170919 num min de jogadas certas em sequência, no AQ
	public int bmMaxPlays;              //170919 num max de jogadas esperando que o jogador acerte bmMinHitsInSequence
	public int mdMaxPlays;              //180320 num max de jogadas esperando que o jogador acerte 3x (ou diferente)
	public int mdMinHitsInSequence;     //180321 num min de jogadas certas em sequência, no JM; se zero, acertar 12 em qualquer posição
	public int minHitsInSequence;       //180320 assintota jogadas: núm que determina que o jogador "adivinhou" o padrão, por acertar em sequência

	public string leftInputKey;         //180328 in addition to the mouse and the arrow keys, use this key for left defense
	public string centerInputKey;       //180328 in addition to the mouse and the arrow keys, use this key for center defense
	public string rightInputKey;        //180328 in addition to the mouse and the arrow keys, use this key for right defense
	public string pausePlayInputKey;	//180403 internal control (same as playPause button for the experimenter)

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

	public JsonStateInput [] states;
	public JsonGameMenuInput [] menus;  //170221 menu com ordem e título dos jogos definidos pelo usuário

	public int GetChoices()
	{
		return Convert.ToInt16(choices);
	}
	
	public int GetDepth()
	{
		return Convert.ToInt16(depth);
	}



	public int GetLimitPlays()
	{
		if(limitPlays != null)
		{
			return Convert.ToInt16(limitPlays);
		}

		return 0;
	}
	
	public JsonInput ()
	{
	
	}
}
