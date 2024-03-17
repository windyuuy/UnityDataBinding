using DataBind.UIBind;
using UnityEditor;

namespace UIDataBind.Editor
{
	[CustomEditor(typeof(DataHostComp))]
	public class DataHostEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var comp = (DataHostComp)this.target;
			var compDataBindHub = comp.DataHub;

			DataBindEditorUtils.DrawDebugView(compDataBindHub);
		}
	}
}