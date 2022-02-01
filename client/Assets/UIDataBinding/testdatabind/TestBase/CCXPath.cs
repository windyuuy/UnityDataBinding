
using System.Linq;
using UnityEngine;

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
			seekNodeByName(child, name);
		}
		return null;
	}
}

