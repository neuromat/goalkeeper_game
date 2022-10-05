/************************************************************************************/
//  Module written by scaroni <renato.scaroni@gmail.com>
//
//	This module is responsible for managing the animation state machine for the 
//	goal animation
/************************************************************************************/

using UnityEngine;
using System.Collections;

public class GKAnimController : MonoBehaviour 
{
	public Animator ball;
	public Animator gk;
	public Animator player;
	
	public float ballDelay = 0.1f;
	public float gkDelay = 0.1f;

	public void Play(string gkDir, string ballDir)
	{
		player.SetTrigger("Play");
		StartCoroutine(DelayedPlayBall(ballDir, gkDir));
	}

	IEnumerator DelayedPlayBall(string ballDir, string gkDir)
	{
		yield return new WaitForSeconds(ballDelay);
		ball.SetTrigger(ballDir);
		StartCoroutine(DelayedPlayGK(gkDir));
	}

	IEnumerator DelayedPlayGK(string action)
	{
		yield return new WaitForSeconds(gkDelay);
		gk.SetTrigger(action);
	}
	

	// Use this for initialization
	void Start () 
	{
		ball.enabled = true;
		gk.enabled = true;
		player.enabled = true;
		// print("ballInitState " + ball.GetCurrentAnimatorStateInfo(0).shortNameHash);
		// print("gkInitState " + gk.GetCurrentAnimatorStateInfo(0).shortNameHash);
		// print("playerInitState " + player.GetCurrentAnimatorStateInfo(0).shortNameHash);		
	}
}
