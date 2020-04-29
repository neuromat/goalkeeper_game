/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// 
/************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VisualTreeManager : MonoBehaviour 
{
	public GameObject treeRoot;

	private void InitTree(Transform go)
	{
		VisualTreeNode node = go.GetComponent<VisualTreeNode>();

		if(node != null)
		{	
			node.probs.SetActive(true);
			foreach(InputField i in node.probs.GetComponentsInChildren<InputField>())
			{
				i.text = "";
			}

			if(go.childCount == 3)
			{
				Transform t = go.GetChild(2);
				node.children = t.gameObject;
				InitTree(t.GetChild(0));
				InitTree(t.GetChild(1));
				InitTree(t.GetChild(2));
			}
			if(go.parent.name.Equals("Tree"))
			{
				node.probs.SetActive(false);
			}
			else
			{
				if(node.children)
					node.children.SetActive(false);
			}
		}
	}

	public void ResetTree()
	{
		InitTree(treeRoot.GetComponent<Transform>());
	}


	// Use this for initialization
	void Start () 
	{
		InitTree(treeRoot.GetComponent<Transform>());
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
