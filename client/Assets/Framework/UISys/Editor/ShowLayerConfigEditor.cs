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

			var div = 2;
			var windowWidth = EditorGUIUtility.currentViewWidth-div*4- 50;
			var width = windowWidth / 3;
			var layerRect = new Rect(position.x, position.y, width, position.height);
			var layerRootRect = new Rect(layerRect.xMax+div, position.y, width, position.height);
			var uriRect = new Rect(layerRootRect.xMax+div, position.y, width, position.height);
			EditorUtils.DrawAssetReferenceT<UILayer>(layerProp, layerRect);

			EditorGUI.PropertyField(layerRootRect, layerRootRefProp, GUIContent.none);
			EditorGUI.PropertyField(uriRect, uriProp, GUIContent.none);
		}

	}
}