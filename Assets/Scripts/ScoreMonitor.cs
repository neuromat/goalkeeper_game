/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// Rewrited by Josi Perez <josiperez.neuromat@gmail.com>
//
// Module responsible for manage the history moves in the Goalkeeper Game
/************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ScoreMonitor : MonoBehaviour 
{
	public UIManager manager;
	public int nShoots;
	public List<ScoreElement> elements;
	
	private Animator anim;

	//Josi: atualiza o historico de jogadas (setas verdes e pretas)
	public void UpdateMenu()
	{
//		if (PlayerPrefs.GetInt ("gameSelected") == 2)  //Josi: era assim
		if ((PlayerPrefs.GetInt ("gameSelected") == 2) || (PlayerPrefs.GetInt ("gameSelected") == 3) || (PlayerPrefs.GetInt ("gameSelected") == 5))     //Josi 161214: se JG ou MD ou Jogo da memoria
		{   
			List<RandomEvent> events = manager.events;

			//Josi: no ultimo jogo entra com events.Count=0, gerando events[-1] entrando em loop com indice outOfRange **
			if (events.Count > 0) {
				
				//print (transform.parent.parent.name);
				RandomEvent e = events [events.Count - 1];

				//Josi: estes sprites estão em Sprites/Interface/Luvas* e tem o desenho da flecha, e da luva (aparentemente um bom lugar para liberar bytes)!
				//      as imagens sao trazidas da definicao no UIManager
				if (events.Count <= nShoots) {
					if (e.correct) {
						elements [events.Count - 1].img.sprite = manager.rightUISprite [e.resultInt];
					} else {
						elements [events.Count - 1].img.sprite = manager.wrongUISprite [e.resultInt];
					}
				} else {
					if (e.correct) {
						elements [nShoots].img.sprite = manager.rightUISprite [e.resultInt];
					} else {
						elements [nShoots].img.sprite = manager.wrongUISprite [e.resultInt];
					}

					//anim.SetTrigger ("slide");  //170108 nao encontrado slide; as vezes gerava erro -> trocado por Reposition
					Reposition();
				}
			}  //Josi: fim do if eventCount > 0
		} //Josi: fim do if JG
	}
	
	
	public void Reposition () //Josi: acerta o historico de chutes (seta verde se correto, seta preta se errado)
	{
//		if (PlayerPrefs.GetInt ("gameSelected") == 2)  //Josi: era assim		
		if ((PlayerPrefs.GetInt ("gameSelected") == 2) || (PlayerPrefs.GetInt ("gameSelected") == 3) || (PlayerPrefs.GetInt ("gameSelected") == 5))     //Josi 161214: se JG ou MD ou memoria
		{   
			for (int i = 0; i < nShoots; i++) {
				elements [i].img.sprite = elements [i + 1].img.sprite;
			}
		}
	}


	public void Reset ()
	{
		for (int i = 0; i < nShoots; i++) {
			elements [i].img.sprite = manager.neutralUISprite;
		}
	}


	int stillStateHash;
	int currentState;
	void Start()
	{
//		if (PlayerPrefs.GetInt ("gameSelected") == 2)  //Josi: era assim
		if ((PlayerPrefs.GetInt ("gameSelected") == 2) || (PlayerPrefs.GetInt ("gameSelected") == 3) || (PlayerPrefs.GetInt ("gameSelected") == 5))     //Josi 161214: se JG ou MD ou Memoria
		{ 
			anim = GetComponent<Animator> ();
			stillStateHash = anim.GetCurrentAnimatorStateInfo (0).shortNameHash;
			currentState = stillStateHash;
		}
	}
	
	void OnEnable () 
	{
//		if (PlayerPrefs.GetInt ("gameSelected") == 2)  //Josi: era assim
		if ((PlayerPrefs.GetInt ("gameSelected") == 2) || (PlayerPrefs.GetInt ("gameSelected") == 3) || (PlayerPrefs.GetInt ("gameSelected") == 5))     //Josi 161214: se JG ou MD ou Memoria
		{  
			anim = GetComponent<Animator> ();
			foreach (ScoreElement e in elements) {
				e.img.sprite = manager.neutralUISprite;
			}
		}
	}
	
	void Update () 
	{
//		if (PlayerPrefs.GetInt ("gameSelected") == 2)  //Josi: era assim		
		if ((PlayerPrefs.GetInt ("gameSelected") == 2) || (PlayerPrefs.GetInt ("gameSelected") == 3) || (PlayerPrefs.GetInt ("gameSelected") == 5))     //Josi 161214: se JG ou MD ou Memoria
		{ 
			AnimatorStateInfo currentBaseState = anim.GetCurrentAnimatorStateInfo (0);
		
			if (currentState != currentBaseState.shortNameHash) {
				if (currentBaseState.shortNameHash == stillStateHash) {
					Reposition ();
				}
			}		
			currentState = currentBaseState.shortNameHash;
		}
	}
}
