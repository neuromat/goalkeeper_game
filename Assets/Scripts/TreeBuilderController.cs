/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// 
/************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;
using System;
using System.IO;
using System.Text;

[System.Serializable]
public class TestCase
{
	public VisualTreeNode node;
	public string prob0;
	public string prob1;
	public string prob2;
}

public class TreeBuilderController : MonoBehaviour 
{
	public string selectedPack;
	public string loadedPackKey;
	public VisualTreeManager tree;
	public GameObject menu;
	public GameObject defaultButton;
	public InputField playLimit;

	private int selectedTreeIndex = 0;

	//Josi era assim
	//	private string [] possibleTrees = 
	//	{
	//		"tree1",
	//		"tree2",
	//		"tree3",
	//		"tree4",
	//		"tree5",
	//		"tree6"
	//	};
	//Josi fica assim (ler apenas as 3 arvores dadas
	private string [] possibleTrees = 
	{
		"tree1",
		"tree2",
		"tree3"
	};
	public InputField sequencia;  //Josi para definir a entrada da sequencia otima



	private JsonInput currTreeStateMachine;

	public void WriteTreeToFile(string json)
	{
		string path = Application.dataPath+"/"+selectedPack+possibleTrees[selectedTreeIndex]+".txt";
		
		// This text is always added, making the file longer over time
		// if it is not deleted.;
		// GUBI: really? It would be AppendAllText
		File.WriteAllText(path, json);
		
		// Open the file to read from.
//		string readText = File.ReadAllText(path);
		//print (path);
		//print ("Saved " + json + " successfully");
	}
	
	public List<TestCase> testCases;
	void PrepareTestCase()
	{
		foreach(TestCase tc in testCases)
		{
			Transform inputs = tc.node.probs.transform;
			//get prob0
			foreach(var p in inputs.GetChild(0).GetComponentsInChildren<InputField>())
			{
				p.text = tc.prob0;
				//print ("Set test "+ p.text+ " " + tc.prob0);
			}
			//get prob1
			foreach(var p in inputs.GetChild(1).GetComponentsInChildren<InputField>())
			{
				p.text = tc.prob1;
				//print ("Set test "+ p.text+ " " + tc.prob1);

			}
			//get prob2
			foreach(var p in inputs.GetChild(2).GetComponentsInChildren<InputField>())
			{
				p.text = tc.prob2;
				//print ("Set test "+ p.text+ " " + tc.prob2);
			}

		}
	}

	void GetStates(Transform go, ref List<JsonStateInput> states, string path)
	{
		Transform t = null;
		VisualTreeNode node = go.GetComponent<VisualTreeNode>();
		if(node != null && node.children != null)
		{
			t = node.children.transform;
		}
		if(t != null && t.gameObject.activeInHierarchy)
		{
			GetStates(t.GetChild(0), ref states, "0" + path);
			GetStates(t.GetChild(1), ref states, "1" + path);
			GetStates(t.GetChild(2), ref states, "2" + path);
		}
		else if(node.probs.activeInHierarchy)
		{
			Transform inputs = node.probs.transform;
			JsonStateInput s = new JsonStateInput();
			
			s.path = path;
			//get prob0
			foreach(var p in inputs.GetChild(0).GetComponentsInChildren<Text>())
			{
				if (p.name.Equals("Text"))
				{
					s.probEvent0 = p.text;
					break;
				}
			}
			//get prob1
			foreach(var p in inputs.GetChild(1).GetComponentsInChildren<Text>())
			{
				if (p.name.Equals("Text"))
				{
					s.probEvent1 = p.text;
					break;
				}
			}
			
			states.Add(s);
		}

		
	}

	private void FromVisualTreeToStateMachine()
	{
		List<JsonStateInput> states = new List<JsonStateInput>();
		
		GetStates(tree.treeRoot.transform, ref states, "");
		
		JsonInput stateMachine = new JsonInput();
		
		int depth = 0;
		foreach(var s in states)
		{
			if(s.path.Length > depth)
			{
				depth = s.path.Length;
			}
			
			//print ("path "+s.path);
			//print ("\ttprob0 "+s.probEvent0);
			//print ("\tprob1 "+s.probEvent1);
		}
		
		stateMachine.depth = depth.ToString();
		stateMachine.choices = (3).ToString();
		//*stateMachine.limitValue = (80).ToString();
		stateMachine.states = states.ToArray();
		stateMachine.limitPlays = playLimit.text;

		//Josi
		stateMachine.sequ = sequencia.text;

		currTreeStateMachine = stateMachine;
	}

	public void SaveTree ()
	{
		List<JsonStateInput> states = new List<JsonStateInput>();

		GetStates(tree.treeRoot.transform, ref states, "");

		JsonInput stateMachine;

		if(currTreeStateMachine == null)
		{
			stateMachine = new JsonInput();
		}
		else
		{
			stateMachine = currTreeStateMachine;
		}

		int depth = 0;
		foreach(var s in states)
		{
			if(s.path.Length > depth)
			{
				depth = s.path.Length;
			}

			// print ("path "+s.path);
			// print ("\ttprob0 "+s.probEvent0);
			// print ("\tprob1 "+s.probEvent1);
		}

		stateMachine.depth = depth.ToString();
		stateMachine.choices = (3).ToString();
		//* stateMachine.limitValue = (80).ToString();
		stateMachine.states = states.ToArray();
		stateMachine.limitPlays = playLimit.text;


		//Josi
		stateMachine.sequ = sequencia.text;


		string json = JsonWriter.Serialize(stateMachine);
	    LoadedPackage.packages[loadedPackKey].stages[selectedIndex] = json;
		WriteTreeToFile(json);
	}

	void SetTree(string depth, double [] probs, VisualTreeNode node)
	{
		if(!depth.Equals(""))
		{
			int nextNode = Int32.Parse(depth[depth.Length - 1].ToString());
			GameObject children = node.children;
			if(children != null && !children.activeInHierarchy)
			{
				node.probs.SetActive(false); 
				children.SetActive(true);
			}
			SetTree(depth.Substring(0, depth.Length - 1), probs, node.children.transform.GetChild(nextNode).GetComponent<VisualTreeNode>());
		}
		else
		{
			Transform probsT = node.probs.transform;

			probsT.GetChild(0).GetComponent<InputField>().text = (probs[0]).ToString();
			probsT.GetChild(1).GetComponent<InputField>().text = (probs[1]).ToString();
			probsT.GetChild(2).GetComponent<InputField>().text = (1 - probs[0] - probs[1]).ToString();
		}
	}

	void LoadTree (JsonInput stateMachine)
	{	
		if(stateMachine.limitPlays != null)
		{
			playLimit.text = stateMachine.limitPlays;
			sequencia.text = stateMachine.sequ;	//Josi
		}
		foreach(JsonStateInput s in stateMachine.states)
		{
			SetTree(s.path, new double[]{Convert.ToDouble(s.probEvent0), Convert.ToDouble(s.probEvent1)}, tree.treeRoot.GetComponent<VisualTreeNode>());
		}
	}

	// Use this for initialization
	void Start () 
	{
	}

	void SelectTreeByIndex (int index)
	{
		currTreeStateMachine = JsonFx.Json.JsonReader.Deserialize<JsonInput>(LoadedPackage.packages[loadedPackKey].stages[index]);
	}

	public void CloseMenu()
	{
		menu.SetActive(false);
	}

	int selectedIndex = 0;
	void AddListener(Button b, int i) 
	{
		b.onClick.AddListener(() => {
			if(loadedPackKey != "")
			{
				SelectTreeByIndex(0);
				tree.ResetTree();
				if(LoadedPackage.packages[loadedPackKey].stages[i] != null &&
				   LoadedPackage.packages[loadedPackKey].stages[i] != "")
				{
					SelectTreeByIndex (i);
					LoadTree(currTreeStateMachine);
				}

				selectedIndex = i;

				menu.SetActive(false);
			}
		});
	}

	void OnEnable ()
	{
		print("OI" + loadedPackKey);
		if(menu != null && loadedPackKey != "")
		{
			menu.SetActive(true);
			print(loadedPackKey);
			int nPkgs = LoadedPackage.packages[loadedPackKey].stages.Count;
			print(nPkgs);

			GridLayoutGroup grid = menu.GetComponentInChildren<GridLayoutGroup>();
			foreach(Button b in grid.GetComponentsInChildren<Button>())
			{
				Destroy(b.gameObject);
			}
			for (int i=0; i < nPkgs; i++)
			{
				print("ADICIONANDO "+i);
				GameObject go = Instantiate(defaultButton);
				go.transform.SetParent(grid.transform);
				go.name = "tree"+(i+1);
				Text btnText = go.GetComponentInChildren<Text>();
				btnText.text = go.name;
				Button btn = go.GetComponentInChildren<Button>();

				AddListener(btn, i);
			}
		}
	}

	public void OpenMenu()
	{
		menu.SetActive(true);
	}

	// Update is called once per frame
	void Update () {
		if(Input.GetKeyUp(KeyCode.S))
		{
			print("preparing statelist");
			SaveTree();
		}
		if(Input.GetKeyUp(KeyCode.T))
		{
			print ("prepared testCases");
			PrepareTestCase();
		}
		if(Input.GetKeyUp(KeyCode.U))
		{
			FromVisualTreeToStateMachine();
			tree.ResetTree();
		}
		if(Input.GetKeyUp(KeyCode.L))
		{
			LoadTree(currTreeStateMachine);
		}
	}
}
