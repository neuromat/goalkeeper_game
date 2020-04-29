/************************************************************************************/
//  Module written by scaroni <renato.scaroni@gmail.com>
//  Changed by Josi to unity3D 2018-1-4f1
//	Deprecated Module. Used for abstraction when dealing with 2 option probability
//	tree
/************************************************************************************/

using System;
using System.Globalization;  //180614 convert "0.7" was giving 7...

public class JsonTreeNode
{
	public string probEvent0;
	public JsonTreeNode right;
	public JsonTreeNode left;
	public string label;
	public string id;
	


	public int GetId()
	{
		return Convert.ToInt16(id);
	}

	public float GetProbEvent0()
	{
        return float.Parse(probEvent0, CultureInfo.InvariantCulture.NumberFormat);
        //return (float)Convert.ToDouble(probEvent0);
	}
			
	public JsonTreeNode ()
	{
	}
}
