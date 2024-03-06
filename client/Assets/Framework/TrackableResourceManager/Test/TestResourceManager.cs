using System;
using System.Collections;
using ResourceManager.Trackable.Runtime;
using UnityEngine;

namespace ResourceManager.Trackable.Test
{
	public abstract class A<T>
	{
		public static int i = 0;
		public int id = i++;
	}

	public class AA:A<int>
	{
	}
	
	public class AB:A<float>
	{
	}

	public class TestResourceManager : MonoBehaviour
	{
		private void Start()
		{
			var c = new AA().id;
			var d = new AB().id;
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