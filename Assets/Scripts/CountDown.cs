using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CountDown : MonoBehaviour 
{
	public int time;
	public Text txt;
	
	public void DecreaseTime()
	{
		time --;
		txt.text = time.ToString();
		if(time <= 0)
		{
			gameObject.SetActive(false);
		}
	}
	
	void OnEnable()
	{
		time = 3;
		txt.text = time.ToString();
	}
	
	// Use this for initialization
	void Start () 
	{
		txt = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
