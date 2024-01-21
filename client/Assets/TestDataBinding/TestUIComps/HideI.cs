using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideI : MonoBehaviour
{
	private void Start()
	{
		gameObject.hideFlags = HideFlags.HideInHierarchy;
		for (var i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.hideFlags = HideFlags.None;
		}
	}
}
