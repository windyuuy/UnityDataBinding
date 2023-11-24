
using Game.Diagnostics.IO;
using UnityEngine;

namespace DataBinding.UIBind
{
	public class CCLifeStat : MonoBehaviour
	{
		protected virtual void OnTransformParentChanged()
		{
			Console.Log("parent-changed:", this.name, this.gameObject.activeSelf, this.name);
		}

	}
}
