
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CCXPath
{

}

public static class CCXPathExt
{
	public static Transform cn(this Component comp, string xpath)
	{
		return comp.transform.Find(xpath);
	}
	public static Transform cn(this Transform trans, string xpath)
	{
		return trans.Find(xpath);
	}

	public static Transform[] cns(this Component comp, string xpath)
	{
		return comp.transform.GetChildren().Where(t => t.name == xpath).ToArray();
	}
	public static Transform[] cns(this Transform trans, string xpath)
	{
		return trans.GetChildren().Where(t => t.name == xpath).ToArray();
	}
	public static void cnrs(this Transform trans, string[] xpath, int index, List<Transform> rets)
	{
		var curPath = xpath[index];
		var formatReg = new Regex(@"^(.*?)(?:\[(\d*)((?:\:)?)(\d*)\])?$");
		var info = formatReg.Match(curPath);
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
					cnrs(child, xpath, index + 1, rets);
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
	public static Transform[] cnrs(this Component comp, string xpath)
    {
		return cnrs(comp.transform,xpath);
    }
	public static Transform[] cnrs(this Transform trans, string xpath)
    {
		var rets=new List<Transform>();
		var paths = xpath.Split(new char[] { '/' });
		cnrs(trans, paths, 0, rets);
		return rets.ToArray();
	}
	public static Transform cnr(this Component comp, string xpath)
    {
		return cnr(comp.transform, xpath);
    }
	public static Transform cnr(this Transform trans, string xpath)
	{
		var rets=cnrs(trans,xpath);
		if(rets.Length > 0)
        {
			return rets[0];
        }
        else
        {
			return null;
        }
	}

	public static Transform cnrf(this Component comp, string xpath)
	{
		return cnrf(comp.transform, xpath);
	}
	public static Transform cnrf(this Transform trans, string xpath)
	{
		var rets = cnrs(trans, xpath);
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
	public static Transform cnrff<T>(this Component comp, string xpath)
	{
		var ret=cnrff<T>(comp.transform, xpath);
		return ret;
	}
	public static T cnrff<T>(this Transform trans, string xpath) where T : Component
	{
		var ret = cnrf(trans, xpath);
		var comp=ret?.GetComponent<T>();
		return comp;
	}
	public static T cnc<T>(this Transform trans) where T : Component
    {
		return trans.GetComponent<T>();
    }

	public static Transform seekNodeByName(this Component comp, string name)
	{
		var trans = seekNodeByName(comp.transform, name);
		if (trans != null)
		{
			return trans;
		}
		return null;
	}
	public static Transform seekNodeByName(this Transform comp, string name)
	{
		for (var i = 0; i < comp.childCount; i++)
		{
			var child = comp.GetChild(i);
			if (child.name == name)
			{
				return child;
			}
			var subResult=seekNodeByName(child, name);
			if(subResult != null)
            {
				return subResult;
            }
		}
		return null;
	}
}

