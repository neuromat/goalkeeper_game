/**************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
//  
// Removed by decision of Amparo Research Group: no progressionBar
/**************************************************************************************/using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProgressionBar : MonoBehaviour 
{
	public Image fill;
	public float totalTime;
	
	private float timeTicker;
	private float initialValue = .0f;  //Josi: era assim, mas comecava preenchido....  .1f;
	private float currTarget = .1f;
	private float currBaseValue = .1f;
	private float barFactor;
	private ProbCalculator probCalculator;
	private float CurrentLimitValue = 80f; 
	
	public void SetInitialValue (float v)
	{
		initialValue = v / barFactor;
	}
	
	public void SetValue (float v)
	{
		fill.fillAmount = v / barFactor;
	}
	
	public void Grow(float target)
	{
		currTarget = target;
		currBaseValue = fill.fillAmount;
	}
	
	public void RecalculateBarFactor ()
	{
		//* float max = probCalculator.GetCurrentLimitValue()/100; //180327 obsolet param
		float max = CurrentLimitValue/100;
		barFactor = max;
	}
	
	void Start () 
	{
		fill.fillAmount = initialValue;	
		probCalculator = ProbCalculator.instance;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!currTarget.Equals(fill.fillAmount))
		{
			fill.fillAmount = Mathf.Lerp(currBaseValue, currTarget, timeTicker/totalTime);
			timeTicker += Time.deltaTime;
		}
		else
		{
			currBaseValue = currTarget;
			timeTicker = 0;	
		}
	}
}
