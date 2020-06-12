using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;                       
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ExpandScreen : MonoBehaviour {

	public Text warningText;
	public Text btnFwrd;

	private LocalizationManager translate;   

	// Use this for initialization
	void Start () {
		translate = LocalizationManager.instance;

		warningText.text = translate.getLocalizedValue ("expandir");
 
		btnFwrd.text = translate.getLocalizedValue("avancar");

		
	}
	
	// Update is called once per frame
	void Update () {
		Screen.fullScreen = true;
	}

	public void ToGame ()             
	{    
		SceneManager.LoadScene ("Configurations");
	}

}
