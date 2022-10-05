using UnityEngine;
using System.Collections;

public class CrowdNoiseController : MonoBehaviour 
{
	public float fadeTime = 1f;
	public float soundDuration = 3f; 
	
	private float timeCount = 0;
	private float fadeTimeCount = 0;
	private AudioSource sound;
	private bool volUp = true;
	
	void Start()
	{
		timeCount = 0;
		fadeTimeCount = 0;
		volUp = true;
		sound = GetComponent<AudioSource>();
	}	
		
	public void OnEnable()
	{
		timeCount = 0;
		fadeTimeCount = 0;
		volUp = true;
	}
		
	void Update () 
	{
		if(volUp)
		{
			if(fadeTimeCount >  fadeTime)
			{
				timeCount += Time.deltaTime;
				if(timeCount > soundDuration)
				{
					volUp = false;
				}
			}
			else
			{
				sound.volume = (fadeTimeCount/fadeTime);
				fadeTimeCount += Time.deltaTime;
			}
		}
		else
		{
			if(fadeTimeCount > 0)
			{
				fadeTimeCount -= Time.deltaTime;
				sound.volume = (fadeTimeCount/fadeTime);
			}
			else
			{
				gameObject.SetActive(false);
			}
		}

	}
}
