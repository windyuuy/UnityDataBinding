using System;
using UnityEngine;

public static class Utils
{
	public static void ForEach<T>(this T[] ls, Action<T> call)
	{
		foreach (var e in ls)
		{
			call(e);
		}
	}

	public static void ForEachChildren(this Transform transform, Action<Transform> call)
	{
		for (var i = 0; i < transform.childCount; i++)
		{
			var e = transform.GetChild(i);
			call(e);
		}
	}

	public static void ForEachChildren(this Component comp, Action<Transform> call)
	{
		var transform = comp.transform;
		for (var i = 0; i < transform.childCount; i++)
		{
			var e = transform.GetChild(i);
			call(e);
		}
	}

	public static T GetOrAddComponent<T>(this Component comp) where T : Component
	{
		var comp1 = comp.GetComponent<T>();
		if (comp1 == null)
		{
			comp1 = comp.gameObject.AddComponent<T>();
		}
		return comp1;
	}
	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		var comp1 = gameObject.GetComponent<T>();
		if (comp1 == null)
		{
			comp1 = gameObject.AddComponent<T>();
		}
		return comp1;
	}

	public static bool IsTrue(object newValue)
	{
		if (newValue is bool)
		{
			return (bool)newValue;
		}
		return newValue != null;
	}

	public static bool IsActiveInHierarchy(this Transform transform)
	{
		return transform.gameObject.activeInHierarchy;
	}

	public static bool IsActiveInHierarchy(this Component component)
	{
		return component.gameObject.activeInHierarchy;
	}

	public static bool IsEnabledInHierarchy(this Component component)
	{
		if (component is MonoBehaviour)
		{
			var mc = component as MonoBehaviour;
			return mc.enabled && mc.gameObject.activeInHierarchy;
		}
		else
		{
			return component.gameObject.activeInHierarchy;
		}
	}

	public static Transform[] GetChildren(this Transform transform)
	{
		var children = new Transform[transform.childCount];
		for (var i = 0; i < transform.childCount; i++)
		{
			children[i] = transform.GetChild(i);
		}
		return children;
	}

	public static bool isValid(Component comp, bool strictMode = false)
	{
		return comp != null;
	}
}
