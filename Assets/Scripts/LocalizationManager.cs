/**
 * Module written by Josi Perez <josiperez.neuromat@gmail.com> (out/17)
 *
 * Responsible for loading localization files from StreamingAssets/i18n
 * Name <locale>.json (example: en_us.json, pt_br.json)
 * Manual reading to keep small files, using pairs "msgCalledInTheCode":"translation"
 * (example: "locale":"en_us") 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;


public class LocalizationManager : MonoBehaviour {

	static private LocalizationManager _instance;
	static public LocalizationManager instance
	{	get
		{	if(_instance == null)
			{
				_instance = GameObject.Find("LocalizationManager").GetComponent<LocalizationManager>();
			}
			return _instance;
		}	
	}


	private string missingTextString = "@";              //171004 missing key msgCalledInTheCode
	public static Dictionary<string, string> localizedText = new Dictionary<string, string>();
	private bool isRunning = false;                      //171023 not sure if it is necessary...
    public Text txtVersion;                              //180627 put at the first scene

    // -----------------------------------------------------------------------------------------------------
    //171004 json reading of selected language
    //171023 coroutines do not appear in the inspector to call onClick, so, one "public void" was created 
    public void loadLocalizedText(string language) {

		if (isRunning == false) {
			//180126 IPAddress Prof Gubi idea  (can know the machine, not the player! privacy respected!)
			//180130 to load IPinfo (needed in ServerOperations)
			//var ipaddress = Network.player.externalIP; //return Intranet IP if is the case... 
			PlayerPrefs.DeleteAll();            //to remove values from previous execution, in the editor
			StartCoroutine (readLocalizedText (language));

            //180627 goes out from Configuration and goes to Localization - first scene
            PlayerPrefs.SetString("gameURL", "http://game.numec.prp.usp.br");
            PlayerPrefs.SetString("version", txtVersion.text);    //180402 to save version game in results file
            PlayerPrefs.SetInt("startPaused", 0);                 //180627 by default, game not start paused
        }
	}


	// -----------------------------------------------------------------------------------------------------
	//171023 separating the reading coroutine to call from the inspector based on gamer selection
	IEnumerator readLocalizedText(string language)
	{
		bool error = false;
		isRunning = true;   //171023

		string filePath;
		if (Application.platform == RuntimePlatform.Android) { 
			filePath = Application.streamingAssetsPath + "/i18n/" + language + ".json";
		} else {
			if (Application.platform == RuntimePlatform.WebGLPlayer) { 
				filePath = Application.streamingAssetsPath + "/i18n/" + language + ".json";
			} else {
				filePath = "file://" + Application.streamingAssetsPath + "/i18n/" + language + ".json";
			}
		}


		WWW www = new WWW (filePath);
		yield return www;

		isRunning = false;

		if (!string.IsNullOrEmpty (www.error)) {
			error = true;
		}

		//171020 if the file does not exist send msg to the output.txt and it does not stop the game,
		//       but all translatable texts will appear as @ - as defined above (arbitrary)
		if (!error) {         //found the file, let's read
			string dataAsJson = www.text;

			//remove keys, quotation marks, \ r \ n - to stay "msgCalledInTheCode":"translation"
			string[] keyValueArray = dataAsJson.Replace ("{", string.Empty).Replace ("}", string.Empty).Replace ("\"", string.Empty).Replace ("\r", string.Empty).Split (new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

			string tmp;
			for (int i = 0; i < keyValueArray.Length; i++) {	
				//json: in all key/value pairs there is the comma separating between items, except in the last pair
				tmp = keyValueArray [i].Substring (keyValueArray [i].IndexOf (":") + 1);
				tmp = (i == keyValueArray.Length - 1) ? tmp : tmp.Substring (0, tmp.Length - 1);

				localizedText.Add (keyValueArray [i].Substring (0, keyValueArray [i].IndexOf (":")), tmp);   
			}
		} else {
			Debug.LogError (">>> Cannot find localization file StreamingAssets" + "/i18n/" + language + ".json");
		}

        SceneManager.LoadScene("TCLE");
    }
		
    public string getLocalizedValue(string key)
    {	
		if (localizedText.ContainsKey (key)) return localizedText [key];
		else return missingTextString;
    }
		
	public void Update()
	{
		if (Input.GetKey ("escape")) {
			clickSair ();
		}
	}

    public void clickVoltar(int where)
    {
        if (where == 1)
        {
            SceneManager.LoadScene("About");
        }
        else {
            SceneManager.LoadScene("MainScene");
        }
        
    }

    public void clickSair ()
    {   //170322 unity3d tem erro ao usar application.Quit
        //       workaround: http://unity3dtrenches.blogspot.com.br/2015/10/unity-3d-compiled-game-freezes-or.html
        //Application.Quit ();
        if (!Application.isEditor) {  //if in the editor, this command would kill unity...
			if (Application.platform == RuntimePlatform.WebGLPlayer) {
				Application.OpenURL("https://game.numec.prp.usp.br");
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


    // -----------------------------------------------------------------------------------------------------
	//170712 put links to logos
	//170721 in webGL link open on the same window game; trying externalEval
	//170810 in Android, call openURL with protocol - http:// or whatever
	//171205 change the routine from LoadStages.cs to credits.cs
    //180607 changed here to be used in other scripts (credits, about, etc)
	public void clickLogos(int where) {
		if (where == 1) {
			#if UNITY_WEBGL
			Application.ExternalEval("window.open('http://neuromat.numec.prp.usp.br/pt-br','CEPID NeuroMat')");
			#else
			Application.OpenURL ("http://neuromat.numec.prp.usp.br/pt-br");
			#endif
		} else {
			if (where == 2) {
				#if UNITY_WEBGL
				Application.ExternalEval("window.open('https://amparo.numec.prp.usp.br/','NeuroMat Amparo')");
				#else
				Application.OpenURL ("https://amparo.numec.prp.usp.br/");
				#endif
			} else {
				if (where == 3) {
					#if UNITY_WEBGL
					Application.ExternalEval("window.open('http://abraco.numec.prp.usp.br/','NeuroMat Abraco')");
					#else
					//Application.OpenURL ("http://neuromat.numec.prp.usp.br/pt-br/content/neuromat-initiative-address-research-and-education-brachial-plexus-injuries");
					Application.OpenURL ("http://abraco.numec.prp.usp.br/");
					#endif
				} else {
					if (where == 4) {
						#if UNITY_WEBGL
						Application.ExternalEval("window.open('http://fapesp.br','FAPESP')");
						#else
						Application.OpenURL ("http://fapesp.br"); 
						#endif
					} else { 
                        if (where == 5) { 
                        #if UNITY_WEBGL
						Application.ExternalEval("window.open('http://neuromat.numec.prp.usp.br/pt-br/nes','NES')");
                        #else
                        Application.OpenURL("http://neuromat.numec.prp.usp.br/pt-br/nes");
                        #endif
                        }
                    }
                }
			}
		}
	}

}