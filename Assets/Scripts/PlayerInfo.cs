/************************************************************************************/
// Module written by scaroni <renato.scaroni@gmail.com>
// Rewrited by Josi Perez <josiperez.neuromat@gmail.com>
//
// Static class that holds all the info entered in the logUser.cs
/************************************************************************************/

using UnityEngine;
using System.Collections;

public class PlayerInfo 
{
	public static string alias = System.String.Empty;        //Use System.String.Empty instead of "" when dealing with lots of strings
	//public static string gender = System.String.Empty;     //170829 comentar se não estão em uso
	//public static string age = System.String.Empty;
	//public static string education = System.String.Empty;
	//public static string destro = System.String.Empty;
	public static bool agree = false;     //170829 concordo/não concordo com o TCLE
}
