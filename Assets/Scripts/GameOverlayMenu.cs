/************************************************************************************/
//  Module written by scaroni <renato.scaroni@gmail.com>
//
//	This Module needs to be looked more closely, apparently needs refactoring
/************************************************************************************/

using UnityEngine;
using System.Collections;

public class GameOverlayMenu : MonoBehaviour 
{
	public CanvasGroup menuCanvas;

	public void OnEnable()
	{
		menuCanvas.interactable = false;
	}

	public void OnDisable()
	{
		menuCanvas.interactable = true;
		gameObject.SetActive (false);
	}

	public void CloseMenu()
	{
		gameObject.SetActive (false);
	}
}
