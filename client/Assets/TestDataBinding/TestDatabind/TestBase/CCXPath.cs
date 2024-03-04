
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CCXPath
{

}

public static class CCXPathExt
{
	public static Transform CN(this Component comp, string xpath)
	{
		return comp.transform.Find(xpath);
	}
	public static Transform CN(this Transform trans, string xpath)
	{
		return trans.Find(xpath);
	}

	public static Transform[] CNS(this Component comp, string xpath)
	{
		return comp.transform.GetChildren().Where(t => t.name == xpath).ToArray();
	}
	public static Transform[] CNS(this Transform trans, string xpath)
	{
		return trans.GetChildren().Where(t => t.name == xpath).ToArray();
	}
	private static readonly Regex FormatReg = new Regex(@"^(.*?)(?:\[(\d*)((?:\:)?)(\d*)\])?$");
	public static void CNRS(this Transform trans, string[] xpath, int index, List<Transform> rets)
	{
		var curPath = xpath[index];
		var info = FormatReg.Match(curPath);
		var subMatchRule = info.Groups[1].Value;
		int matchIndexMin;
		var minExist = int.TryParse(info.Groups[2].Value, out matchIndexMin);
		if (!minExist)
        {
			matchIndexMin = int.MinValue;
		}
		var rangeSplitExist=!string.IsNullOrEmpty(info.Groups[3].Value);
		int matchIndexMax;
		if(!int.TryParse(info.Groups[4].Value, out matchIndexMax))
        {
            if (rangeSplitExist)
            {
				matchIndexMax = int.MaxValue;
            }
            else
            {
                if (minExist)
                {
					matchIndexMax = matchIndexMin;
                }
                else
                {
					matchIndexMax = int.MaxValue;
				}
			}
		}
		var reg=new Regex($"^{subMatchRule}$");
        if (xpath.Length > index + 1)
        {
			var matchedIndex = 0;
			for (var i = 0; i < trans.childCount; i++)
			{
				var child = trans.GetChild(i);
				if (reg.IsMatch(child.name))
				{
                    if (matchedIndex < matchIndexMin)
                    {
						matchedIndex++;
						continue;
                    }
					if(matchedIndex > matchIndexMax)
                    {
						break;
                    }
					matchedIndex++;
					CNRS(child, xpath, index + 1, rets);
				}
			}
        }
        else
        {
			var matchedIndex = 0;
			for (var i = 0; i < trans.childCount; i++)
			{
				var child = trans.GetChild(i);
				if (reg.IsMatch(child.name))
				{
					if (matchedIndex < matchIndexMin)
					{
						matchedIndex++;
						continue;
					}
					if (matchedIndex > matchIndexMax)
					{
						break;
					}
					matchedIndex++;
					rets.Add(child);
				}
			}
		}
    }
	public static Transform[] CNRS(this Component comp, string xpath)
    {
		return CNRS(comp.transform,xpath);
    }
	public static Transform[] CNRS(this Transform trans, string xpath)
    {
		var rets=new List<Transform>();
		var paths = xpath.Split(new char[] { '/' });
		CNRS(trans, paths, 0, rets);
		return rets.ToArray();
	}
	public static Transform CNR(this Component comp, string xpath)
    {
		return CNR(comp.transform, xpath);
    }
	public static Transform CNR(this Transform trans, string xpath)
	{
		var rets=CNRS(trans,xpath);
		if(rets.Length > 0)
        {
			return rets[0];
        }
        else
        {
			return null;
        }
	}

	public static Transform CNRF(this Component comp, string xpath)
	{
		return CNRF(comp.transform, xpath);
	}
	public static Transform CNRF(this Transform trans, string xpath)
	{
		var rets = CNRS(trans, xpath);
		if (rets.Length == 1)
		{
			return rets[0];
		}
		else if(rets.Length==0)
		{
			return null;
        }
        else
        {
			throw new System.Exception($"too many matched results: {rets.Length}");
        }
	}
	public static Transform CNRFF<T>(this Component comp, string xpath)
	{
		var ret=CNRFF<T>(comp.transform, xpath);
		return ret;
	}
	public static T CNRFF<T>(this Transform trans, string xpath) where T : Component
	{
		var ret = CNRF(trans, xpath);
		var comp=ret?.GetComponent<T>();
		return comp;
	}
	public static T CNC<T>(this Transform trans) where T : Component
    {
		return trans.GetComponent<T>();
    }

	public static Transform SeekNodeByName(this Component comp, string name)
	{
		var trans = SeekNodeByName(comp.transform, name);
		if (trans != null)
		{
			return trans;
		}
		return null;
	}
	public static Transform SeekNodeByName(this Transform comp, string name)
	{
		for (var i = 0; i < comp.childCount; i++)
		{
			var child = comp.GetChild(i);
			if (child.name == name)
			{
				return child;
			}
			var subResult=SeekNodeByName(child, name);
			if(subResult != null)
            {
				return subResult;
            }
		}
		return null;
	}
}

