using System;
using UnityEngine;

namespace UISys.Runtime
{
	/// <summary>
	/// 管理UI层级覆盖问题
	/// </summary>
	[AddComponentMenu("UISys/TransformCoverManager")]
	public class TransformCoverManagerComp : MonoBehaviour
	{
		[SerializeField] protected bool hideOnPause = false;

		private void OnEnable()
		{
			
		}

		protected virtual void OnTransformChildrenReSort()
		{
			UpdateTransformCover();
		}

		private void UpdateTransformCover()
		{
			if (!enabled)
			{
				return;
			}

			var transformChildCount = this.transform.childCount;
			var isCover = false;
			for (var i = transformChildCount - 1; i >= 0; i--)
			{
				var child = this.transform.GetChild(i);
				var cover = child.GetComponent<TransformCoverComp>();
				if (cover == null)
				{
					if (hideOnPause && isCover)
					{
						child.gameObject.SetActive(false);
					}

					if (isCover)
					{
						child.SendMessage("OnTransformShield", SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						child.SendMessage("OnTransformExpose", SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (isCover && !cover.IgnoreCover)
				{
					// child.SendMessage("OnTransformPause", SendMessageOptions.DontRequireReceiver);
					cover.DispatchTransformPause();
				}
				else
				{
					if (cover.IsCover && child.gameObject.activeSelf)
					{
						isCover = true;
					}

					// child.SendMessage("OnTransformResume", SendMessageOptions.DontRequireReceiver);
					cover.DispatchTransformResume();
				}
			}
		}
	}
}