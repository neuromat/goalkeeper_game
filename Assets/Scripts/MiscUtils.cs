/************************************************************************************/
//  Module written by scaroni <renato.scaroni@gmail.com>
//
//	A module containing some useful methods. Intended to be used as a static class
/************************************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MiscUtils : MonoBehaviour 
{
	public static IEnumerator WaitAndLoadLevel(string level, float t)
	{
		yield return new WaitForSeconds(t);

//		Application.LoadLevel (level);
		SceneManager.LoadScene(level);
	}
}
