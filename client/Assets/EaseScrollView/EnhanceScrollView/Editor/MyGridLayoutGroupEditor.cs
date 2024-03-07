using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
	[CustomEditor(typeof(MyGridLayoutGroup), true)]
	[CanEditMultipleObjects]
	/// <summary>
	/// Custom Editor for the MyGridLayout Component.
	/// Extend this class to write a custom editor for a component derived from GridLayout.
	/// </summary>
	public class MyGridLayoutGroupEditor : GridLayoutGroupEditor
	{
		protected SerializedProperty SyncSiblingIndex;
		protected SerializedProperty HideUnused;

		protected bool ShowSampleTransform = false;

		protected override void OnEnable()
		{
			base.OnEnable();
			SyncSiblingIndex = serializedObject.FindProperty("syncSiblingIndex");
			HideUnused = serializedObject.FindProperty("hideUnused");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();
			EditorGUILayout.PropertyField(SyncSiblingIndex, true);
			EditorGUILayout.PropertyField(HideUnused, true);
			serializedObject.ApplyModifiedProperties();

			EditorGUI.BeginChangeCheck();
			var show = EditorGUILayout.Toggle("ShowSampleTransform", ShowSampleTransform);
			if (EditorGUI.EndChangeCheck())
			{
				ShowSampleTransform = show;

				var layout = (MyGridLayoutGroup)target;
				var transform = layout.transform;
				for (var i = 0; i < transform.childCount; i++)
				{
					var child = transform.GetChild(i);
					if (child.name=="$SampleTrans")
					{
						if (show)
						{
							child.gameObject.hideFlags = HideFlags.None;
						}
						else
						{
							child.gameObject.hideFlags = HideFlags.HideAndDontSave;
						}
					}
				}
			}

			var enablePreviewProp = serializedObject.FindProperty("enablePreview");
			var previewCountProp = serializedObject.FindProperty("previewCount");
			EditorGUI.BeginChangeCheck	();
			EditorGUILayout.PropertyField(enablePreviewProp);
			EditorGUILayout.PropertyField(previewCountProp);
			if (EditorGUI.EndChangeCheck())
			{
				var layout = target;
				var transform = (RectTransform)((LayoutGroup)layout).transform;
				LayoutRebuilder.MarkLayoutForRebuild(transform);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}