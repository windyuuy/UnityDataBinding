using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI
{
	[ExecuteInEditMode]
	public class MyVerticalLayoutGroup: VerticalLayoutGroup
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
			DrivenRectTransformTracker.StopRecordingUndo	();
			base.SetLayoutHorizontal();
			m_Tracker.Clear();
			DrivenRectTransformTracker.StartRecordingUndo	();
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void SetLayoutVertical()
		{
			DrivenRectTransformTracker.StopRecordingUndo	();
			base.SetLayoutVertical();
			m_Tracker.Clear();
			DrivenRectTransformTracker.StartRecordingUndo	();
		}
	}
}