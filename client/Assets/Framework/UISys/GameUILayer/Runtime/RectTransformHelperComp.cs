using UnityEngine;

namespace UISys.Runtime
{
	[AddComponentMenu("UISys/RectTransformHelper")]
	[RequireComponent(typeof(RectTransform))]
	public class RectTransformHelperComp: MonoBehaviour
	{
		protected RectTransform RectTransform => (RectTransform)this.transform;
		
		[UIAction]
		public void SetPosition(RectTransform target)
		{
			RectTransform.anchoredPosition = target.anchoredPosition;
		}

		[UIAction]
		public void SetContentSize(RectTransform target)
		{
			RectTransform.sizeDelta = target.sizeDelta;
			RectTransform.localPosition = target.localPosition;
		}
	}
}