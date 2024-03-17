using DataBind.UIBind;
using UnityEditor;
using UnityEngine;

namespace UIDataBind.Editor
{
	[CustomEditor(typeof(DataBindHubComp))]
	public class DataBindHubEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var comp = (DataBindHubComp)this.target;
			var compDataBindHub = comp.DataBindHub;

			DataBindEditorUtils.DrawDebugView(compDataBindHub);
		}
	}
}