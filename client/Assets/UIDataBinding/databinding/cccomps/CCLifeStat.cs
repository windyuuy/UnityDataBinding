
using UnityEngine;

namespace UI.DataBinding
{
	public class CCLifeStat : MonoBehaviour
	{
		protected virtual void OnTransformParentChanged()
		{
			console.log("parent-changed:", this.name, this.gameObject.activeSelf, this.name);
		}

	}
}
