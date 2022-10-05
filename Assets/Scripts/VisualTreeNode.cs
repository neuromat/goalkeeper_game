/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// 
/************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VisualTreeNode : MonoBehaviour 
{
	public GameObject probs;
	public GameObject children;

	public void ToggleNode()
	{
		if(children != null && !children.activeInHierarchy)
		{
			probs.SetActive(false); 
			children.SetActive(true);
		}
		else if(children != null && children.activeInHierarchy)
		{
			probs.SetActive(true); 
			children.SetActive(false);
		}
	}
	
	void Start () 
	{
		GetComponent<Button>().onClick.AddListener(ToggleNode);
	}
	
	void Update () {
	
	}
}
