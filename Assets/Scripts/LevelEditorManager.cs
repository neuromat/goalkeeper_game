/************************************************************************************/
//  Module written by scaroni <renato.scaroni@gmail.com>
//
//	This Module manages the Level Editor and makes the bridge between the package
// 	selection screen and the visual tree editor
/************************************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class LevelEditorManager : MonoBehaviour 
{
	public GameObject treeBuilder;
	public GameObject packSelector;
	public GameObject btnPrefab;
	public LoadStages loadStages;

//	private Dictionary<string, Button> packs;

	static private LevelEditorManager _instance;
	static public LevelEditorManager instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = GameObject.Find("LevelEditorManager").GetComponent<LevelEditorManager>();
			}

			return _instance;
		}
	}

	public void LoadTreeBuilder ()
	{
		treeBuilder.SetActive(true);
		packSelector.SetActive (false);
	}

	public void LoadTreeBuilder (string path)
	{
		string[] elements = path.Split('/');
		int last = elements.Length - 1;
		treeBuilder.GetComponent<TreeBuilderController>().selectedPack = elements[last-1]+"/"+elements[last]+"/";
		treeBuilder.GetComponent<TreeBuilderController>().loadedPackKey = path;
		treeBuilder.SetActive(true);
		packSelector.SetActive (false);
	}

	// Auxiliary method
	public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		// Get the subdirectories for the specified directory.
		DirectoryInfo dir = new DirectoryInfo(sourceDirName);
		
		if (!dir.Exists)
		{
			throw new DirectoryNotFoundException(
				"Source directory does not exist or could not be found: "
				+ sourceDirName);
		}
		
		DirectoryInfo[] dirs = dir.GetDirectories();
		// If the destination directory doesn't exist, create it.
		if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}
		
		// Get the files in the directory and copy them to the new location.
		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files)
		{
			string temppath = Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, false);
		}
		
		// If copying subdirectories, copy them and their contents to new location.
		if (copySubDirs)
		{
			foreach (DirectoryInfo subdir in dirs)
			{
				string temppath = Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath, copySubDirs);
			}
		}
	}

	public void LoadPackSelector ()
	{
		treeBuilder.SetActive(false);
		packSelector.SetActive (true);
	}


	//Josi: ninguém chama esta function
	public void ToConfigurations ()
	{
		//PlayerPrefs.DeleteAll();
		//Application.LoadLevel("Configurations");
		SceneManager.LoadScene("Configurations");
	}
	
	public void DeletePackage (string name)
	{
//		Destroy(packs[name].gameObject);
	}
	
	public void AddPackage ()
	{
	}

	public void AddTree ()
	{

	}

	void AddListeners(Button b, string value) 
	{
		b.onClick.AddListener(() => LoadTreeBuilder());
		foreach(Button b2 in b.GetComponentsInChildren<Button>())
		{
			if(!b2.name.Equals (b))
			{
				b2.onClick.AddListener(() => DeletePackage(value));
			}
		}
	}
	
	void Start () 
	{
		LoadPackSelector();
		OnLoadPkgInternal onLoadPkgInternalHandler = LoadTreeBuilder;
		OnLoadPkgExternal onLoadPkgExternalHandler = LoadTreeBuilder;

		loadStages.OnLoadPkgInternalHandler = onLoadPkgInternalHandler;
		loadStages.OnLoadPkgExternalHandler = onLoadPkgExternalHandler;
		loadStages.gameObject.SetActive(true);
	}
	
	public void CloseApp()
	{
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

	void Update () 
	{
	
	}
}
