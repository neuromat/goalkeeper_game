/**************************************************************************************/
//  Module written by Josi Perez <josiperez.neuromat@gmail.com> (jun/18)
//
//	Responsible for show About details for Goalkeeper Game;
//  >>> missing code to increase/decrease the screen on android devices (pinch/zoom)
/**************************************************************************************/
using UnityEngine;
using UnityEngine.UI;               //171006 type Text variables
using UnityEngine.SceneManagement;  //170407 LoadScene

using TMPro;                        //180612 to OnPointerClick() function
//using UnityEngine.EventSystems;     //idem


public class about : MonoBehaviour
{
	private LocalizationManager translate;  //171006 instance declaration to allow calling scripts from another script
	//171006 elements to translate
	public Text txtVoltar;
	public Text txtSair;

    //180607 menu itens for About
    public Text txtGame;
    public Text txtWhat;
    public Text txtSoftware;
    public Text txtDoc;
    public Text txtCreditos;

    //180612 explain itens
    public GameObject txtExpWhat;
    public GameObject txtExpSoftware;
    public GameObject txtExpDoc;

    // -----------------------------------------------------------------------------------------------------
    // Use this for initialization
    void Start ()	{   
		//171005 instance declaration to allow calling scripts from another script
		translate = LocalizationManager.instance;

		//171006 to change names and jobs headers (psis and qsis people)
		txtVoltar.text = translate.getLocalizedValue ("voltar");
		txtSair.text = translate.getLocalizedValue ("exit");

        //180612 gameTitle
        txtGame.text = translate.getLocalizedValue("jogo");

        //180612 about: menu itens
        txtWhat.text = translate.getLocalizedValue("what");
        txtSoftware.text = translate.getLocalizedValue("software");
        txtDoc.text = translate.getLocalizedValue("doc");
        txtCreditos.text = translate.getLocalizedValue("creditos");

        //180612 about: explain menu itens
        txtExpWhat.GetComponentInChildren<TMPro.TMP_Text>().text = translate.getLocalizedValue("txtWhat").Replace("\\n", "\n");
        txtExpSoftware.GetComponentInChildren<TMPro.TMP_Text>().text = translate.getLocalizedValue("txtSoftware").Replace("\\n", "\n");
        txtExpDoc.GetComponentInChildren<TMPro.TMP_Text>().text = translate.getLocalizedValue("txtDoc").Replace("\\n", "\n");


        //180607 activate the first page: what (arbitrary)
        txtExpWhat.SetActive(true);
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
    //       SceneManager.LoadScene ("MainScene");
	//}


	// -----------------------------------------------------------------------------------------------------
	//170407 Exit button
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


    // ----------------------------------------------------------------------------------------------------
    //180607 show text for the About menu selected (what is, software, license, etc)
    public void showTextAboutItem(int what)
    {
        if (what == 4)
        {
            SceneManager.LoadScene("Credits");
        }
        else
        {
            txtExpWhat.SetActive(what == 1);
            txtExpSoftware.SetActive(what == 2);
            txtExpDoc.SetActive(what == 3);
        }
    }


    // ----------------------------------------------------------------------------------------------------
    //180612 to recognize links in the middle of textMeshPro texts
    //       the cliclable part should be between something like that: <link="docJG">abc</link>
    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    int linkIndex = TMP_TextUtilities.FindIntersectingLink(txtExpDoc.GetComponentInChildren<TMPro.TMP_Text>(), Input.mousePosition, null);
    //    if (linkIndex != -1)
    //    {
    //        //TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
    //        TMP_LinkInfo linkInfo = txtExpDoc.GetComponentInChildren<TMPro.TMP_Text>().textInfo.linkInfo[linkIndex];
    //        switch (linkInfo.GetLinkID())
    //        {
    //            case "docJG":
    //                Application.OpenURL("http://answers.unity3d.com");
    //                break;
    //            case "id_2":
    //                //Do something else
    //                break;
    //        }
    //    }
    //}

}

