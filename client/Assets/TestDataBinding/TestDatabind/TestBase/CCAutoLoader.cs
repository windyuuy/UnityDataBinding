
using System;
using UnityEngine;

public class CCAutoLoader :MonoBehaviour
{
	public GameObject[] refers = Array.Empty<GameObject>();

	protected virtual void Awake() {
		foreach (var r in this.refers)
		{
			var x = Instantiate(r, this.transform, false);
		}
	}
}
