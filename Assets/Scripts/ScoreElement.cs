/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// 
/************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreElement : MonoBehaviour 
{
	private ScoreMonitor scoreManager;

	private Image _img;
	public Image img
	{
		get
		{
			if(_img == null)
			{
				_img = GetComponent<Image>();
			}
			return _img;
		}
	}

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
