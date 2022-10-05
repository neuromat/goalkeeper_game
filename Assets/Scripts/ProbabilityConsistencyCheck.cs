/************************************************************************************/
//  Module written by scaroni <renato.scaroni@gmail.com>
//
//	This module is respondible for data consistency in the probability input on the
//	level editor
/************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class ProbabilityConsistencyCheck : MonoBehaviour 
{
	private List<Text> probs  = new List<Text>();
	private int mySiblingIndex;
	private InputField inputField;

	// Use this for initialization
	void Start () 
	{
		inputField = GetComponent<InputField>();

		Transform prnt = transform.parent;
		mySiblingIndex = transform.GetSiblingIndex();

		for(int i = 0; i < 3; i++)
		{
			foreach(Text t in prnt.GetChild(i).GetComponentsInChildren<Text>())
			{
				if(t.name.Equals("Text"))
				{
					probs.Add(t);
				}
			}
		}

		inputField.onEndEdit.AddListener(DoConsistencyCheck);
	}
	
	private Dictionary<int, float> probValues = new Dictionary<int, float> ();
	public void DoConsistencyCheck (string s)
	{
		int i = mySiblingIndex;

		probValues[i] = (float)Convert.ToDouble(s);
		print(probs[(i+1)%3].text);
		probValues[(i+1)%3] = (float)Convert.ToDouble(probs[(i+1)%3].text);
		print(probs[(i+2)%3].text);
		probValues[(i+2)%3] = (float)Convert.ToDouble(probs[(i+2)%3].text);

		float sum = probValues[i] + probValues[(i+1)%3] + probValues[(i+2)%3];
		print(sum);
		if(sum > 1)
		{
			print("should update value");
			float value = 1 - (probValues[(i+1)%3] + probValues[(i+2)%3]);
			inputField.text = value.ToString();
		}
	}
}
