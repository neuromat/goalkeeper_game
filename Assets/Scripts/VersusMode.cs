/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// 
/************************************************************************************/
using UnityEngine;
using System.Collections;

public class VersusMode : MonoBehaviour 
{
	public UIManager ui;
	public ProbCalculator probs;
	
	static private VersusMode _instance;
	static public VersusMode instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = GameObject.Find("VersusModeManager").GetComponent<VersusMode>();
			}
			
			return _instance;
		}
	}
	
	public void ChooseGKMove()
	{		
		if(!probs.gameObject.activeInHierarchy)
		{
			probs.gameObject.SetActive(true);
			probs.Start();
		}
		int e = probs.GetEvent(0);  //170130 apenas para compilar; acrescentado param para atender ao Jogo da Memoria (5) e 
		//       comparar input com correto e avancar ou nao no MDIndex; nao vale nos demais jogos;
		//*      170216 o param false equivale a saber se o jogo está na phaseZero do JG
		//Debug.Log("Resultado "+ e); //170915
		string gkMove = "";
				
		if(e == 0) //esquerda
		{
			gkMove = "esquerda";
		}
		else if (e == 1)
		{
			gkMove = "direita";
		}else
		{
			gkMove = "centro";
		}
		
		ui.BtnActionGetEvent(gkMove);
	}
	
	void Start ()
	{
		ui = GetComponent<UIManager>();
	}
	
	void Update()
	{
	
	}
}
