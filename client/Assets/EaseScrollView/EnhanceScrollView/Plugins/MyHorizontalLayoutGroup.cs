using UnityEngine.UI;

namespace UnityEngine.UI
{
	[ExecuteInEditMode]
	public class MyHorizontalLayoutGroup : HorizontalLayoutGroup
	{
		[SerializeField] protected bool enablePreview = true;

		public bool EnablePreview
		{
			get
			{
#if UNITY_EDITOR
				return enablePreview;
#else
                return false;
#endif
			}
		}

		[SerializeField] protected int previewCount = 20;
		public int PreviewCount => previewCount;

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void SetLayoutHorizontal()
		{
#if UNITY_EDITOR
			DrivenRectTransformTracker.StopRecordingUndo();
#endif
			base.SetLayoutHorizontal();
			m_Tracker.Clear();
#if UNITY_EDITOR
			DrivenRectTransformTracker.StartRecordingUndo();
#endif
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void SetLayoutVertical()
		{
#if UNITY_EDITOR
			DrivenRectTransformTracker.StopRecordingUndo();
#endif
			base.SetLayoutVertical();
			m_Tracker.Clear();
#if UNITY_EDITOR
			DrivenRectTransformTracker.StartRecordingUndo();
#endif
		}
	}
}