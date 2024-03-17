using DataBind.UIBind;
using UnityEditor;
using UnityEngine;

namespace UIDataBind.Editor
{
	[CustomEditor(typeof(DataBindBaseComp))]
	public class DataBindBaseEditor: UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			var comp = (DataBindBaseComp)this.target;
			var compDataBindHub = comp.DataBindPump;

			DataBindEditorUtils.DrawDebugView(compDataBindHub);
		}
	}
}