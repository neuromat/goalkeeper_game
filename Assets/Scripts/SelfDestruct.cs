using UnityEngine;
using System.Collections;

public class SelfDestruct : MonoBehaviour 
{
	public float time = 2;
	public bool destroyDefinetly = true;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		time -= Time.deltaTime;
		if(time <= 0)
		{
			if(destroyDefinetly)
			{
				Destroy(gameObject);
			}
			else
			{
				gameObject.SetActive(false);
			}	
		}
	}
}
