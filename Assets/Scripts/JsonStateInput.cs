using System;
using System.Globalization;  //180614 convert "0.7" was giving 7...

public class JsonStateInput
{
	public string path;
	public string probEvent0;
	public string probEvent1;
	
	public float GetProbEvent0()
	{
        return float.Parse(probEvent0, CultureInfo.InvariantCulture.NumberFormat);
        //return (float)Convert.ToDouble(probEvent0);  //convert "0.7" was giving 7
    }

    public float GetProbEvent1()
	{
        return float.Parse(probEvent1, CultureInfo.InvariantCulture.NumberFormat);
        //return (float)Convert.ToDouble(probEvent1);  //convert "0.7" was giving 7
    }
}