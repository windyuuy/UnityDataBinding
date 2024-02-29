using System;
using UISys.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UISys.Editor
{
	[CustomPropertyDrawer(typeof(OpenLayerConfig))]
	public class ShowLayerConfigEditor : UnityEditor.PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			try
			{
				DrawProps(position, property, label);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		private void DrawProps(Rect position, SerializedProperty property, GUIContent label)
		{
			var layerProp = property.FindPropertyRelative("layer");
			var layerRootRefProp = property.FindPropertyRelative("layerRootRef");
			var uriProp = property.FindPropertyRelative("uri");

			EditorGUILayout.BeginHorizontal();

			EditorUtils.DrawAssetReferenceT<UILayer>(layerProp);

			EditorGUILayout.PropertyField(layerRootRefProp, GUIContent.none);
			EditorGUILayout.PropertyField(uriProp, GUIContent.none);
			EditorGUILayout.EndHorizontal();
		}

	}
}