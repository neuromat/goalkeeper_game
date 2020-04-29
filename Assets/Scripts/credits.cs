/**************************************************************************************/
//  Module written by Josi Perez <josiperez.neuromat@gmail.com> (abr/17)
//
//	Responsible for show people and articles related with the Goalkeeper Game;
//  >>> missing code to increase/decrease the screen on android devices (pinch/zoom)
/**************************************************************************************/
using UnityEngine;
using UnityEngine.UI;               //171006 type Text variables
using UnityEngine.SceneManagement;  //170407 LoadScene


public class credits : MonoBehaviour
{
	private LocalizationManager translate;  //171006 instance declaration to allow calling scripts from another script
	//171006 elements to translate
	public Text txtPessoas1;
	public Text txtPessoas2;
	public Text txtTrabalhos;
	public Text txtVoltar;
	public Text txtSair;
	//171113 designer Tom pede para colocar todos os logos dentro da tela de créditos
	public Text Realizacao;        
	public Text Apoio;


	// -----------------------------------------------------------------------------------------------------
	// Use this for initialization
	void Start ()	{   
		//171005 instance declaration to allow calling scripts from another script
		translate = LocalizationManager.instance;

		//171006 to change names and jobs headers (psis and qsis people)
		txtPessoas1.text = txtPessoas1.text.Replace("P1", translate.getLocalizedValue ("P1"));
		txtPessoas1.text = txtPessoas1.text.Replace("P2", translate.getLocalizedValue ("P2"));
		txtPessoas1.text = txtPessoas1.text.Replace("P6", translate.getLocalizedValue ("P6"));

		//180315 development people
		txtPessoas2.text = txtPessoas2.text.Replace("P3", translate.getLocalizedValue ("P3"));
		txtPessoas2.text = txtPessoas2.text.Replace("P4", translate.getLocalizedValue ("P4"));
		txtPessoas2.text = txtPessoas2.text.Replace("P5", translate.getLocalizedValue ("P5"));

		txtTrabalhos.text = txtTrabalhos.text.Replace("T1", translate.getLocalizedValue ("T1"));	
		txtVoltar.text = translate.getLocalizedValue ("voltar");
		txtSair.text = translate.getLocalizedValue ("exit");

		//171113 designer Tom pede para colocar todos os logos dentro da tela de créditos    
		Realizacao.text = translate.getLocalizedValue ("realiz");
		Apoio.text = translate.getLocalizedValue ("apoio");
	}
	
	// -----------------------------------------------------------------------------------------------------
	// Update is called once per frame
	void Update ()	{
		if (Input.GetKey ("escape")) {
			if (!Application.isEditor) {  //if in the editor, this command would kill unity...
				if (Application.platform == RuntimePlatform.WebGLPlayer) {
					Application.OpenURL (PlayerPrefs.GetString ("gameURL"));
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
	}


	// -----------------------------------------------------------------------------------------------------
	//170407 Return button
    //180627 centralized at Localization
	//public void  clickVoltar ()	{
        //180613 Credits goes out from Team Screen and goes to Main Menu
        //       SceneManager.LoadScene ("Configurations");
        //SceneManager.LoadScene("About");
    //}


	// -----------------------------------------------------------------------------------------------------
	//170407 Exit button
    //180627 centralized at Localization
	//public void clickSair ()  {
	//	if (!Application.isEditor) {  //if in the editor, this command would kill unity...
	//		if (Application.platform == RuntimePlatform.WebGLPlayer) {
	//			Application.OpenURL (PlayerPrefs.GetString ("gameURL"));
	//		} else {
	//			//171121 not working kill()
	//			if ((Application.platform == RuntimePlatform.IPhonePlayer) || 
	//				(SystemInfo.deviceModel.Contains("iPad"))) {           //try #IF UNITY_IOS
	//				Application.Quit ();     
	//			} else {
	//				System.Diagnostics.Process.GetCurrentProcess ().Kill (); 
	//			}
	//		}
	//	}
	//}

}

