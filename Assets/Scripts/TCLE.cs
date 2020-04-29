/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// Rewrited by Josi Perez <josiperez.neuromat@gmail.com>
//
// This module manages the data collection on the log. It initializes the form and
// saves the data on the PlayerInfo class for later persistance on remote DB
/************************************************************************************/

using UnityEngine;
using UnityEngine.UI;               //toggles
using System.Collections;
using UnityEngine.SceneManagement;  //171005 LoadScene
using TMPro;                        //171009 textMesh Pro (justified text and many other resources)

//using System.Media.Sounds;    //Josi: for SystemSounds (using UnityEditor; vale no editor não em build): https://forum.unity3d.com/threads/beep.180045/
//using UnityEngine.EventSystems; //Josi: para dar foco no apelido, ver em https://www.reddit.com/r/Unity3D/comments/2nom92/forcing_focus_on_input_field_in_46/




public class TCLE: MonoBehaviour 
{
	//public GameObject userData;                 //@@@Josi: IntroScene(1)/Canvas/LogBox/MenuGameMode
	public InputField alias;

    // public InputField age;                   //161205 pedir apenas nome
	// public GameObject Gender;                //161205 pedir apenas nome
	// public Dropdown gender;                  //161205 pedir apenas nome
	// public Dropdown education;               //161205 pedir apenas nome
	// public Dropdown destro;
    // public string gender;           
	// public string education;                 //Josi: 161205 pedir apenas nome
	// public string destro;                    //Josi: 161205 pedir apenas nome

	public Toggle agree;                        //170829 TCLE concorda 
	public Toggle notAgree;                     //170830      ou não
	public ColorBlock agreeOriginalColors;      //170926 guardar as cores originais (qdo user muda de decisao)
	public ColorBlock notAgreeOriginalColors;   //170926 guardar as cores originais (qdo user muda de decisao)

	public Text obrigaAlias;                    //Josi: para ligar msg apenas se apelido vazio 
	//@@private GameFlowManager gameFlowManager;    //Josi para continuar o jogo apos preencher dados

	//Josi 170817
	#if UNITY_ANDROID || UNITY_IOS
	private TouchScreenKeyboard mobileKeyboard;     
	#endif
	public bool isKeyboardOpen = false;         //180220 start keyboard on mobile devices

	private LocalizationManager translate;      //171009 trazer script das rotinas de translation

	//171009 translation
    //180625 screen TCLE/alias comes after language selection, not more between gameMenu and gameModule
	public Text preenchaNome;
    public Text btnAvancar;
    //public Text btnJogar;
	//public Text btnJogarPausa;
	//public Text btnMenu;
	public Text placeholder;
	//public Text tcleHeader;
	public Text tcleNotAgree;
	public Text tcleAgree;
    public Text tcleText;                //180625 now, a TMPro justified, not TextUI



	// -----------------------------------------------------------------------------------------------------
	// Checks if there is anything entered into the input field.
	public void LockInput(InputField input)	
	{
		if (input.text.Trim().Length == 0)  {   
			obrigaAlias.text = translate.getLocalizedValue ("obrigaAlias"); //171011 necessary fill playerAlias
			alias.Select();
			alias.ActivateInputField();
		}
	}



	// -----------------------------------------------------------------------------------------------------
	public void EnterData()
	{   
		if (alias.text.Trim().Length == 0) {           //to avoid fill with spaces
			obrigaAlias.text = translate.getLocalizedValue ("obrigaAlias"); //171011 necessary fill playerAlias

			//180220 problem to open keyboard on iOS; navigation vertical on inspector
			#if UNITY_ANDROID || UNITY_IOS
			//		if (! isKeyboardOpen) {
			//			isKeyboardOpen = true;
			//			mobileKeyboard = TouchScreenKeyboard.Open(alias.text, TouchScreenKeyboardType.Default, false, false, false, false, "");
			//		}
			//		if(mobileKeyboard.done == true)	{
			//			alias.text = mobileKeyboard.text;
			//			mobileKeyboard = null;
			//		} else {
			//			alias.text = mobileKeyboard.text;
			//		}
			#else
			alias.Select();
			alias.ActivateInputField();
			#endif
		} else {
			//170830 necessary to inform if agree or not in participate of the search
			if (!(agree.isOn || notAgree.isOn)) {
				//171011 obrigaAlias.text = "Por favor, informe sua concordância!";  
				obrigaAlias.text = translate.getLocalizedValue ("obrigaTCLE");
			} else {
				PlayerInfo.alias = alias.text;
				//@@@userData.SetActive (false);

                //180625 logUsers was after select a game module and now, after select idiom
                //@@gameFlowManager.NewGame (PlayerPrefs.GetInt ("gameSelected"));    //Josi: antes havia esta continuidade no onClick Unity, agora passa para ca para poder reclamar do apelido vazio
                SceneManager.LoadScene("Configurations");
            }
		}
	}




	// -----------------------------------------------------------------------------------------------------
	void Start () 
	{   //161205 pedir apenas nome
		//PlayerInfo.gender = "M";
		//PlayerInfo.education = "Fundamental Completo";
		//PlayerInfo.destro = "D";


		//171009 declarar a instance para permitir chamar rotinas do outro script
		translate = LocalizationManager.instance;

		//171009 translate
		preenchaNome.text = translate.getLocalizedValue ("preenchaNome");
		//btnJogar.text = translate.getLocalizedValue ("btnJogar");
		//btnJogarPausa.text = translate.getLocalizedValue ("btnJogarPausa");
		//btnMenu.text = translate.getLocalizedValue ("btnMenu");
		placeholder.text = translate.getLocalizedValue ("placeholder");
		//tcleHeader.text = translate.getLocalizedValue ("tcleHeader");
		tcleNotAgree.text = translate.getLocalizedValue ("tcleNotAgree");
		tcleAgree.text = translate.getLocalizedValue ("tcleAgree");

        //180625 from UiText to TMPro
		tcleText.text = translate.getLocalizedValue ("tcle").Replace("\\n","\n");
        //tcleText.GetComponentInChildren<TMPro.TMP_Text>().text = translate.getLocalizedValue("tcle").Replace("\\n", "\n");

        btnAvancar.text = translate.getLocalizedValue("avancar");

        //Josi: declare GameFlowManager to continue if data filled
        //@@gameFlowManager = GameFlowManager.instance;

        //170926 salvar as cores originais do toggle concorda/naoConcorda
        agreeOriginalColors = agree.colors;
		notAgreeOriginalColors = notAgree.colors;
	}




	// -----------------------------------------------------------------------------------------------------
	void Update () {
		//Josi 170817
		#if UNITY_ANDROID || UNITY_IOS
//		if (! isKeyboardOpen) {
//			isKeyboardOpen = true;
//			mobileKeyboard = TouchScreenKeyboard.Open(alias.text, TouchScreenKeyboardType.Default, false, false, false, false, "");
//		}
//		if(mobileKeyboard.done == true)	{
//			alias.text = mobileKeyboard.text;
//			mobileKeyboard = null;
//		} else {
//			alias.text = mobileKeyboard.text;
//		}
		#else
		alias.Select();
		alias.ActivateInputField();
		#endif
	}


	// -----------------------------------------------------------------------------------------------------
	//170829 TCLE: setar Concordo/Não concordo; se N, não se podem gravar os resultados!
	public void tcleOptionChoosed() {

		//170830 save to ask before send to write file in uiManager.SendEventsToServer (if not isOn do not record!)
		PlayerInfo.agree = agree.isOn;

		//170925 bold selected option and it is necessary to think that user can change again
		ColorBlock tmp;
		if (agree.isOn) { 
			tmp = agreeOriginalColors;
			tmp.normalColor = tmp.highlightedColor;
			agree.colors = tmp;  //verde (ok)

			notAgree.colors = notAgreeOriginalColors;
		} else {
			tmp = notAgreeOriginalColors;
			tmp.normalColor = tmp.highlightedColor;
			notAgree.colors = tmp;  //vermelho (nok)

			agree.colors = agreeOriginalColors;
		}
	}




	// -----------------------------------------------------------------------------------------------------
	void OnEnable()
	{   
        //@@@@ como resolver agora....
		//if (PlayerInfo.alias == System.String.Empty) {      //170216 Use System.String.Empty instead of "" when dealing with lots of strings;)
		//	if (!PlayerPrefs.	HasKey ("gameSelected")) {
		//		userData.SetActive (false);
		//	} else {
		//		userData.SetActive (true);
		//	}
		//} else {
		//	userData.SetActive (false);
		//}
	}


}
