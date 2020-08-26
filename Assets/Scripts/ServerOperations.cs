// Module written by scaroni <renato.scaroni@gmail.com>
// Rewrited by Josi Perez <josiperez.neuromat@gmail.com>
//
// Responsible for making all http requests in all environments (standalone, android and web),
// to save results.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;				
using JsonFx.Json;
using System.Text;
using System.Security.Cryptography;
using UnityEngine.UI;

public class ServerOperations
{
	private LocalizationManager translate;
	private UIManager uiManager;
	public bool postDone = false;
	public WWW www = null;

	static private ServerOperations _instance;
	static public ServerOperations instance
	{
		get {
			if(_instance == null) {
				_instance = new ServerOperations();
			}
			return _instance;
		}
	}

	public IEnumerator UploadFile(string loginURL, WWWForm formData, UIManager uiManager)
	{
		uiManager.www = new WWW (loginURL, formData);
		uiManager.showMsg.GetComponent<Text>().text = translate.getLocalizedValue("txtSendingPlayData").Replace("\\n", "\n");
		yield return uiManager.www;
		uiManager.sentFile = true;

		// DEBUG: test for connection via GET
		GKGConfigContainer gkgConfig = GKGConfigContainer.Load();
		yield return new WWW(gkgConfig.configItems[0].URL + "/get_sent.html");
	}

	public IEnumerator LogUserActivity(string filename, string content, UIManager uiManager = null)
	{
		if (Application.platform != RuntimePlatform.WebGLPlayer) yield return "";

		WWWForm formData = new WWWForm ();

		formData.AddField("content", content);
		string loginURL = Application.absoluteURL + filename;

		www = new WWW(loginURL, formData);
		//To differentiate from registering user entry than user activity (plays)
		if (uiManager)
		{
			uiManager.showMsg.GetComponent<Text>().text = translate.getLocalizedValue("txtRegisteringUserEntry").Replace("\\n", "\n");
		}
		yield return www;
		//To differentiate from registering user entry than user activity (plays)
		if (uiManager) postDone = true;

		if (www.error != null) {
			Debug.Log ("Error logging user activity to the server: " + www.error);
		}
	}

}


