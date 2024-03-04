using System;
using System.Collections;
using ResourceManager.Trackable.Runtime;
using UnityEngine;

namespace ResourceManager.Trackable.Test
{
	public class TestResourceManager: MonoBehaviour
	{
		private void Start()
		{
			Debug.Log("lkwjef");
			{
				using var rr = ResourceScope.New;
			}

			StartCoroutine(DelayTest());
		}

		IEnumerator DelayTest()
		{
			yield return new WaitForEndOfFrame();
			
			{
				using var rr = ResourceScope.New;
			}
		}
	}
}