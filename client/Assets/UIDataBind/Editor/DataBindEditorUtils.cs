using DataBind.UIBind;
using UnityEditor;
using UnityEngine;

namespace UIDataBind.Editor
{
	public class DataBindEditorUtils
	{
		public static void DrawDebugView(IRawObjObservable compDataBindHub)
		{
			var shapeRect = EditorGUILayout.GetControlRect(true);
			var click = GUI.Button(shapeRect, "Debug");
			if (click)
			{
				DataBindHubEditorWindow.ShowBindRelation(compDataBindHub);
			}
		}
	}
}