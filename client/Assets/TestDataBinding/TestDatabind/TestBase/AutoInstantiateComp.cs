using System;
using UnityEngine;

namespace TestDataBind.TestBasic
{
	public class AutoInstantiateComp :MonoBehaviour
	{
		public GameObject[] refers = Array.Empty<GameObject>();

		protected virtual void Awake() {
			foreach (var r in this.refers)
			{
				var x = Instantiate(r, this.transform, false);
			}
		}
	}
}
