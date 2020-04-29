/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// Rewrited by Josi Perez <josiperez.neuromat@gmail.com>
//
// Module responsible for loading teams and its configuration phases 
// from a local directory or from the web (server neuroMat)
/************************************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;


public delegate void OnLoadPkgExternal(string path);
public delegate void OnLoadPkgInternal();
public delegate void OnMenuTimeout();


public class LoadStages : MonoBehaviour 
{
	List <SourcePath> treeSourcePaths = new List<SourcePath> ();


	public GridLayoutGroup g;                    //scene Configuration/Canvas/Menu/menuPacotes - grid de 1 coluna para receber os botoes dinamicos
	public GameObject btnPrefab;
	public Text warning;
	public OnMenuTimeout OnMenuTimeOutHandler;
	public OnLoadPkgExternal OnLoadPkgExternalHandler;
	public OnLoadPkgInternal OnLoadPkgInternalHandler;

//	private Button firstPacketButton;            //161207: salvar PRIMEIRO botao dinamico de pacotes e invocar onClick forcado

	private Button[] buttons;                    //170921 para chamar posteriormente os botões dinamicos e dar um destaque ao selecionado
	private byte dynamicButtonDefaultAlpha = 70; //170921 chamado 2x; só para nao esquecer um deles algum dia

	private LocalizationManager translate;       //171005 trazer script das rotinas de translation

	//171006 translations
	//public Text Creditos;                      //180614 goes out from Team Screen and goes to Menu Screen
	public Text headTitle;
	//public Text txtVersion;                    //180402 to save version used
	public Text selectTeam;
	public Text txtNext;
	public Text txtSair;

    public Text txtVersion;                      //180627 put at the first scene, available for all

    //171114 read CustomTrees from webServer neuromat if Android
    //171124 same for iOS
    public static readonly string androidTreesServerLocation = "game.numec.prp.usp.br/game/CustomTreesANDROID/";
	public static readonly string iosTreesServerLocation = "game.numec.prp.usp.br/game/CustomTreesIOS/";
	public static readonly string webProtocol = "http://";


	//180126 Link https://www.myip.com/api-docs/
	//       {"ip":"66.249.75.9","country":"United States","cc":"US"}
	static readonly string getMyIP = "https://api.myip.com"; 
	[Serializable]
	public class webIPinfo {
		public string ip;
		public string country;
		public string cc;
	}

	//170417 aumentar num de fases
	public static string [] files = new string[] {
		"tree1",
		"tree2",
		"tree3",
		"tree4",
		"tree5",
		"tree6",
		"tree7",
		"tree8"
	};


	// -----------------------------------------------------------------------------------------------------
	void AddSourcePath(string url)
	{
		SourceType st;			// web or file

		if (url.StartsWith ("http")) {
			st = SourceType.web;
		} else	{
			st = SourceType.file;
		}

		SourcePath sp = new SourcePath ();
		sp.url = url;
		sp.sourceType = st;
		treeSourcePaths.Add(sp);
	}

	// -----------------------------------------------------------------------------------------------------
	// Where to get trees from?
    void DefaultTreeSourceInit ()
	{
		string url;
		SourceType st;

		if (Application.dataPath.StartsWith ("http")) 
		{
			url = Application.dataPath + "/CustomTrees/";
			st = SourceType.web;
		}
		else
		{
			//StreamingAssets is packed in the build
			//171124 also for iOS
			string c_trees;
			if ((Application.platform == RuntimePlatform.Android)  ||
				(Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad"))) {  
				c_trees = Application.streamingAssetsPath + "/CustomTrees/index.info";
			} else {
				c_trees = Application.dataPath + "/CustomTrees/";
			}


            //171124 includes iOS
			if ((Application.platform != RuntimePlatform.Android)  &&
				(Application.platform != RuntimePlatform.IPhonePlayer) && (!SystemInfo.deviceModel.Contains("iPad"))) {  
				if (!Directory.Exists (c_trees)) {
					//print("Não achei, criando o default!");

					Directory.CreateDirectory (c_trees);
					Directory.CreateDirectory (c_trees + "/Pacote1");       //G1
					File.AppendAllText (c_trees + "/index.info", "Pacote1");   //G1

					TextAsset texto;
					texto = (TextAsset)Resources.Load ("CustomTrees/Pacote1/tree1");   //G1
					File.AppendAllText (c_trees + "/Pacote1/tree1.txt", texto.text);      //G1

					texto = (TextAsset)Resources.Load ("CustomTrees/Pacote1/tree2");  //G1
					File.AppendAllText (c_trees + "/Pacote1/tree2.txt", texto.text);     //G1

					texto = (TextAsset)Resources.Load ("CustomTrees/Pacote1/tree3");  //G1
					File.AppendAllText (c_trees + "/Pacote1/tree3.txt", texto.text);     //G1
				}
				url = "file://"+c_trees;
				st = SourceType.file;
			} else {
				url = c_trees; //"file://"+c_trees;
				st = SourceType.file;
			}
		}

		
		SourcePath sp = new SourcePath ();
		sp.url = url;
		sp.sourceType = st;
		treeSourcePaths.Add(sp);
	}


	//--------------------------------------------------------------------------------------
	// Use this for initialization
	void Start ()
	{
		//180209 to avoid delays in LocalizationManager (original place); here have enough time
		StartCoroutine (readMachineIP ());  

		int i = 0;

		//171005 instance declaration to allow calling scripts from another script
		translate = LocalizationManager.instance;

		//171006 texts to change on the interface
		//Creditos.text = translate.getLocalizedValue ("creditos");
		headTitle.text = translate.getLocalizedValue ("jogo");
		selectTeam.text = translate.getLocalizedValue ("escolha");
		txtNext.text = translate.getLocalizedValue ("avancar");
		txtSair.text = translate.getLocalizedValue ("sair").Replace("\\n","\n");

        //170310 delete PlayerPrefs; be careful: LoadStages loaded the packet name to use later
        //180130 PlayerPrefs.DeleteAll ();     deleted at beginning, on LocalizationManager

        //170622 in webGL mode, define the page to go when the user select Exit/ESC
        //180627 goes out from here and goes to Localization
        //PlayerPrefs.SetString ("gameURL", "http://game.numec.prp.usp.br");
        //PlayerPrefs.SetString ("version", txtVersion.text);       //180402 to save version game in results file
        txtVersion.text = PlayerPrefs.GetString("version");         //180627 taked from Localization


        warning.color = new Color32(255,255,255,0); 
		if(warning != null) {
			warning.text = System.String.Empty;   //170110 era ""; Use System.String.Empty instead of "" when dealing with lots of strings
		}

		// No path found, use default
		if(i == 0)	{
			DefaultTreeSourceInit();      //carrega as arvores do diretorio CustomTrees
		} else	{
			if(warning != null)	{
				//warning.text = "Carregado das preferencias de usuario:\n";
				warning.text = translate.getLocalizedValue ("loadPrefs");  //171006
			}
		}

		// read the trees in coroutines
		foreach(SourcePath s in treeSourcePaths) {
			StartCoroutine (ReadSource(s.url, s.sourceType));
		}
	}



	//--------------------------------------------------------------------------------------
	//leitura do index.info que contem os nomes dos times, separados por ";"
	IEnumerator ReadSource(string url, SourceType st)
	{
		//170815 era assim: WWW www = new WWW (url+"index.info");
		string fileToAccess;
		//android
		if (Application.platform == RuntimePlatform.Android) { 
			//=================================================
			//try to read from the server, mainly
			//171127 else read local CustomTrees
			fileToAccess = webProtocol + androidTreesServerLocation + "index.info";
			WWW internet = new WWW (fileToAccess);
			yield return internet;
			if ((internet.error != null) && (internet.error != "")) {
				//if not connection, read CustomTrees local
				fileToAccess = url;
			}
			//=================================================


		//171124 iOS (iPad/iPhone)
		} else {
			if ((Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains ("iPad"))) { 
				//=================================================
				//try to read from the server, mainly
				//171127 else read local CustomTrees
				fileToAccess = webProtocol + iosTreesServerLocation + "index.info";
				WWW internet = new WWW (fileToAccess);
				yield return internet;
				if ((internet.error != null) && (internet.error != "")) {
					//if not connection, read CustomTrees local
					fileToAccess = st + "://" + url;
				}
				//=================================================

			//standalone
			} else {
				fileToAccess = url + "index.info";
			}
		}

		//read index.info content: the team names
		WWW www = new WWW (fileToAccess);
		yield return www;


		if ((www.error != null) && (www.error != "")) {   //180411 was left transparent to avoid show "debug" messages for user; error messages should appear
			//warning.color = new Color32(255,255,255,255); 

			if(warning != null)
				warning.text += "url: "+ url + " Status: "+www.error + "\n";
			else
				warning.text = "url: "+ url + "\nError: "+www.error + "\n";  //171005 generic msg
		} else	{
			if (warning != null) {
				warning.text += warning.text = "url: "+ url + " Status: Success\n";
			}
				

			//put the team names in a vector
			if (www.text != null) {
				string[] files = www.text.Split(';'); // package list  

				if(LoadedPackage.packages == null)	{
					LoadedPackage.packages = new Dictionary<string, Package> ();
				}

				//create a button for each team name ========================================================================

				//170817 android keep index.info in the variable - remove
				//171124 ios idem
				if ((Application.platform == RuntimePlatform.Android) ||
					(Application.platform == RuntimePlatform.IPhonePlayer) || (SystemInfo.deviceModel.Contains("iPad"))) {
					url = fileToAccess.Replace("index.info","");
				}


				//170920 changing dynamic grid to accept until 20 teams
				//171129 refactoring the cellSizes to adapt to iPad
				//180315 changing text size to increase size in Android and others
				g.GetComponent<RectTransform> ().localScale = Vector3.one;              //let the parent with initial size (1,1,1)
				float xCellWidth = g.GetComponent<RectTransform> ().rect.width / 1.5f;  //not occupy all cell 180315 from 1.5 to 1.1f
				float xCellHeight = g.GetComponent<RectTransform> ().rect.height;

				if (files.Length <= 10) {                                               //180403 to better adapt Amparo experiment
					g.constraintCount = 1;                                              //1column
					xCellHeight = xCellHeight / files.Length;
					xCellHeight -= g.spacing.x;                     
					g.cellSize = new Vector2 (xCellWidth, ((files.Length == 1) ? xCellHeight / 3.0f : xCellHeight / 0.9f));
				} else {
					g.constraintCount = 2;                                             //2 columns
					xCellHeight = xCellHeight / (files.Length/2);
					xCellHeight -= g.spacing.x;
					//g.cellSize = new Vector2 (xCellWidth/1.5f, xCellHeight/((files.Length >11)?1.1f:1.4f)); 
					g.cellSize = new Vector2 (xCellWidth/1.3f, xCellHeight/1.3f);     //180315 size in Android was very small
				}                                                                     //180403 change from 1.5

				int i = 0;  //salvar dados do primeiro botao de pacotes; se houver apenas um e o user avancar sem selecionar, fica valendo este
				foreach(string f in files)
				{
					if(g != null) // is there a layout?
					{             // it is a grid defined in LoadStages.cs refering scene Configuration/Canvas/menu/menuPacotes
						LoadedPackage.packages[url + f] = new Package(url, f);

						GameObject go = Instantiate(btnPrefab);      //the image button; a prefab to repeat each new team

						go.transform.SetParent(g.transform);
						go.GetComponentInChildren<Text>().text = f;
						
                        //180316 better separate the cases: standalone/web and mobiles for a letter more compatible
						#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
						go.GetComponentInChildren<Text>().resizeTextMaxSize  = 40;
						#else
						go.GetComponentInChildren<Text>().resizeTextMaxSize  = 80;
						#endif

						go.name = f;

						//170921 clarear a imagem de fundo do botão, para destacar depois, o escolhido
						go.GetComponent<Image>().color = new Color32(255,255,255,dynamicButtonDefaultAlpha);

						//171129
						go.GetComponent<RectTransform>().localScale = Vector3.one;  

						Button b = go.GetComponent<Button>();
						AddListener(b, url+f);

						//Josi: 161207: salvar botao para onClick simulado do primeiro pacote na lista
						if (i == 0) {                 
							//firstPacketButton = go.GetComponent<Button>(); //botao
							//AddListener (firstPacketButton, url + f);      //e conteudo do que fazer em onClick
							b.onClick.Invoke();                              //nao deu certo chamar ao sair... fica assim...

							//teamSelected = f;                           //170310 salvar nome do primeiro pacote e mudar depois se user clicar outro
							//170310 save teamSelected in PlayerPrefs to use after
							PlayerPrefs.SetString ("teamSelected", f);

							i++;
						}
					}
				}
			}

		}
	}


	//--------------------------------------------------------------------------------------
	void AddListener(Button b, string value) 
	{	//Load configuration file
		b.onClick.AddListener(() => LoadTreePackageFromExternalSource(value));
	}


	//--------------------------------------------------------------------------------------
	//aqui le os arquivos de configuracao do time selecionado
	IEnumerator LoadExternal(String url)
	{
		//170815 diferentes paths conforme o ambiente
		//       a url Android continha o index.info
		string fileToAccess;
		string url2mobiles;       //180220 to solve paths with accents in iOS/Android environments


		//170817 url android está carregando o nome do arquivo
		if (Application.platform == RuntimePlatform.Android) {
			url = url.Replace ("index.info", "");
		}
		warning.text = "***** LoadExternal url = " + url;
		

		// Antes de mais nada, limpamos o que já existe
		LoadedPackage.packages [url].stages.Clear ();

		//180220 in iOS paths, necessary to change to HTML codes (HTML URL Encoding Reference);
		//    tip found in
		//    1) https://forum.unity.com/threads/resources-load-with-special-characters-in-the-file-name-ios-and-mac.372881/
		//    2) https://answers.unity.com/questions/546213/handling-special-characters-aeouuouo-in-unity.html 
		//    fileToAccess = fileToAccess.Normalize(System.Text.NormalizationForm.FormD);
		//    fileToAccess = WWW.EscapeURL(fileToAccess,System.Text.Encoding.UTF8);
		//    fileToAccess = fileToAccess.Replace("á","%C3%A1");  //worked finally...
		//
		#if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX  || UNITY_ANDROID
		url2mobiles = convertPathToMobiles(url);
		#endif

		// para o total de fases (até 8) em um mesmo time, ler as configuracoes;
		// isto precisa melhorar e virar um unico arquivo...
		for (int i = 0; i < files.Length; i++)
		{
			//180220 use url2iOS only for www access; after that comes to the normal
			#if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_ANDROID
			warning.text = url2mobiles + "/" + files [i] + ".txt";
			#else
			warning.text = url + "/" + files [i] + ".txt";
			#endif		
			fileToAccess = warning.text;

			WWW www = new WWW (fileToAccess);
			yield return www;

			if((www.error != null) && (www.error != ""))   //170817 melhor "parentezar"
			{
				if(warning != null)	{
					warning.text = "Failed to upload file " + files[i]+".txt\n" + www.error; //171005generic msg
				}
			} else {
				if (www.text != null)  {
					LoadedPackage.packages[url].stages.Add(www.text); 
				}				
			}

		}

		LoadedPackage.loaded = LoadedPackage.packages[url];

		if(warning != null)
		{   //171005 translation
			warning.text = translate.getLocalizedValue("loadPckgs") + "\n" + LoadedPackage.packages[url].name;

			//packetSelected = LoadedPackage.packages[url].name.ToString();  //170310 salvar nome do pacote selecionado pelo user
			//170310 salvar nome do pacote selecionado pelo user em PlayerPrefs
			PlayerPrefs.SetString ("teamSelected", LoadedPackage.packages[url].name);


			//170921 destacar o time selecionado
			//lembrar de repintar todos, por possiveis selecionados antes (o 1o escolhido programaticamente)
			buttons = g.GetComponentsInChildren<Button>();
			for (int xx=0; xx < buttons.Length; xx++) {
				//buttons [xx].GetComponentInChildren<Text> ().SetNativeSize ();
				if (buttons [xx].name == LoadedPackage.packages [url].name) {
					//buttons [xx].GetComponentInChildren<Text> ().fontStyle = FontStyle.Bold; ficou muito feio
					buttons [xx].GetComponentInChildren<Text> ().color = new Color32(255,255,0,255);
					buttons[xx].GetComponent<Image>().color = new Color32(255,255,255,255);
				} else {
					buttons [xx].GetComponentInChildren<Text> ().color = Color.white;
					buttons[xx].GetComponent<Image>().color = new Color32(255,255,255,dynamicButtonDefaultAlpha);
				}
			}
		}


		if (OnLoadPkgExternalHandler == null)
		 	OnLoadPkgExternalHandler = null;//StartCoroutine(MiscUtils.WaitAndLoadLevel("MainScene", 3));
		else
			OnLoadPkgExternalHandler(url);
	}


	//--------------------------------------------------------------------------------------
	public void LoadTreePackageFromExternalSource(string path) 
	{
		StartCoroutine(LoadExternal (path));
	}



	//--------------------------------------------------------------------------------------
	public void LoadDefaultPackage()
	{
		LoadTreePackageFromResources ();

		if(warning != null)
		{   //171011
			//warning.text = "Fases carregadas com sucesso de Pacote de fases padrao";
			warning.text = "Success loading Default Package Phases";
		}

		if (OnLoadPkgInternalHandler == null)
			StartCoroutine(MiscUtils.WaitAndLoadLevel("MainScene", 1));
		else
			OnLoadPkgInternalHandler();
	}



	//--------------------------------------------------------------------------------------
	static public void LoadTreePackageFromResources ()
	{
		Package pkg;
		if (LoadedPackage.packages == null || !LoadedPackage.packages.ContainsKey ("Resources/default"))
		{
			LoadedPackage.packages = new Dictionary<string, Package>();
			pkg = new Package ("Resources", "default");
		}
		else 
		{
			pkg = LoadedPackage.packages ["Resources/default"];
		}

		foreach (string file in files) 
		{
			var tree = Resources.Load("Trees/"+file) as TextAsset;
			if(tree == null)
			{
				Debug.Log(">>> error loading Resources/Trees");  //keep this error; goes to the output_log.txt
				return;
			}
			
			GameObject debugLoadedTrees = GameObject.FindGameObjectWithTag("debugLoadedTrees");
			if(debugLoadedTrees != null)
			{
				if(tree != null)
					debugLoadedTrees.GetComponent<Text>().text += "Loaded: "+file+ "\n";
				else
					debugLoadedTrees.GetComponent<Text>().text += "Could not load: "+file+ "\n";
				
			}
			
			string json = tree.text;
			pkg.stages.Add(json);
		}

		LoadedPackage.loaded = pkg;
	}


	// -----------------------------------------------------------------------------------------------------
	//180126 get machine/device IP and country from app by myip.com
	IEnumerator readMachineIP()
	{
		WWW myIPinfo = new WWW (getMyIP);
		yield return myIPinfo;

		if (string.IsNullOrEmpty (myIPinfo.error)) {
			//Debug.Log ("info = " + myIPinfo.text); 
			webIPinfo IPdata = JsonUtility.FromJson<webIPinfo>(myIPinfo.text);
			PlayerPrefs.SetString ("IP", IPdata.ip);
			PlayerPrefs.SetString ("Country", IPdata.cc);

		} else {
			//Debug.Log ("erro ao ler myIP");  then, use Network or internetReachability
			//var ipaddress = Network.player.externalIP;  //return Intranet IP if is the case...
			//Application.internetReachability: not trustable
			PlayerPrefs.SetString ("IP", "UNASSIGNED");
			PlayerPrefs.SetString ("Country", "XX");
		}
	}


	// -----------------------------------------------------------------------------------------------------
	//Josi: botao SAIR na tela inicial de menu de jogos
    //180627 centralized at Localization
	//public void Sair ()	{
		//170322 unity3d tem erro ao usar application.Quit
		//       workaround: http://unity3dtrenches.blogspot.com.br/2015/10/unity-3d-compiled-game-freezes-or.html
		//Application.Quit ();
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



	// -----------------------------------------------------------------------------------------------------
	//170407 tela de créditos (pedido Carlos Ribas)
    //180614 out from team selection and goes to menu screen
	//public void showCreditos()	{
	//	SceneManager.LoadScene ("Credits");
	//}


    // -----------------------------------------------------------------------------------------------------
	//180220 to convert special characters into HTML reference code, using UTF-8
	//       https://answers.unity.com/questions/546213/handling-special-characters-aeouuouo-in-unity.html 
	//       https://www.w3schools.com/tags/ref_urlencode.asp
	public string convertPathToMobiles(string url2mobiles)
	{
		string[] symbol = new string[]     {" ",  "À",     "Á",     "Â",     "Ã",     "Ç",     "È",     "É",     "Ê",     "Ì",     "Í",     "Î",     "Ñ",     "Ò",     "Ó",     "Ô",     "Õ",     "Ù",     "Ú",     "Û",     "à",     "á",     "â",     "ã",     "ç",     "è",     "é",     "ê",     "ì",     "í",     "î",     "ñ",     "ò",     "ó",     "ô",     "õ",     "ù",     "ú",     "û" };
		string[] symbolHTML = new string[] {"%20","%C3%80","%C3%81","%C3%82","%C3%83","%C3%87","%C3%88","%C3%89","%C3%8A","%C3%8C","%C3%8D","%C3%8E","%C3%91","%C3%92","%C3%93","%C3%94","%C3%95","%C3%99","%C3%9A","%C3%9B","%C3%A0","%C3%A1","%C3%A2","%C3%A3","%C3%A7","%C3%A8","%C3%A9","%C3%AA","%C3%AC","%C3%AD","%C3%AE","%C3%B1","%C3%B2","%C3%B3","%C3%B4","%C3%B5","%C3%B9","%C3%BA","%C3%BB" };

		for (var i = 0; i < symbol.Length; i++) {
			url2mobiles = url2mobiles.Replace (symbol [i], symbolHTML [i]);
		}
		return url2mobiles;
	}
	
	

	// -----------------------------------------------------------------------------------------------------
	public void ToGame (int error)             //170310 param error, vindo do probs.confValidation
	{                                          
		SceneManager.LoadScene ("MainScene");
	}



	// -----------------------------------------------------------------------------------------------------
	void Update () 
	{
		//Josi: outra maneira de Sair, sem clicar no botão: apertar a tecla ESCAPE
		//      https://docs.unity3d.com/ScriptReference/Application.Quit.html
		//
		if (Input.GetKey ("escape")) {
			//170322 unity3d tem erro ao usar application.Quit
			//       workaround: http://unity3dtrenches.blogspot.com.br/2015/10/unity-3d-compiled-game-freezes-or.html
			//Application.Quit ();
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
}



public class Package
{
	public string path;
	public string name;

	// Initializes a new instance of the <see cref="Package"/> class.
	public Package (string _path, string _name)
	{
		path = _path;
		name = _name;
		stages = new List<string> ();
	}
	
	public List<string> stages;
}

public static class LoadedPackage
{
	public static Dictionary<string,Package> packages;
	public static Package loaded;
}

public struct SourcePath
{
	public string url;
	public SourceType sourceType;
}


public enum SourceType {web, file};


