using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UISys.Editor
{
	public static class EditorUtils
	{
		public static void DrawScriptView(this SerializedObject serializedObject)
		{
			GUI.enabled = false;
			var script=serializedObject.FindProperty("m_Script");
			EditorGUILayout.PropertyField(script);
			GUI.enabled = true;
		}

		public static readonly AssetReferenceGameObject EmptyReferenceGameObject = new(null);

		public static AssetReferenceGameObject GetEmptyReferenceGameObject(AssetReferenceGameObject empty)
		{
			if (empty.RuntimeKeyIsValid())
			{
				return EmptyReferenceGameObject;
			}

			return empty;
		}
		
		public static void DrawAssetReferenceT<T>(SerializedProperty layerProp, Rect layerRect) where T : Component
		{
			DrawAssetReferenceT(layerProp, typeof(T), layerRect);
		}
		public static void DrawAssetReferenceT(SerializedProperty layerProp, Type type, Rect layerRect)
		{
			var objRef = (AssetReferenceGameObject)layerProp.boxedValue;
			var nullRef = EditorUtils.GetEmptyReferenceGameObject(objRef);
			var value2 = objRef != null && objRef.RuntimeKeyIsValid()
				? AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(objRef.AssetGUID),
					type)
				: null;
			var gType = typeof(GameObject);
			EditorGUI.BeginChangeCheck();
			var uiLayer = EditorGUI.ObjectField(layerRect, GUIContent.none, value2,
				gType, false);
			if (EditorGUI.EndChangeCheck())
			{
				string guid = null;
				if (uiLayer != null && uiLayer is GameObject gameObject)
				{
					uiLayer = gameObject.GetComponent(type);
					guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(uiLayer));
				}

				if (string.IsNullOrEmpty(guid))
				{
					layerProp.boxedValue = nullRef;
				}
				else
				{
					layerProp.boxedValue = new AssetReferenceGameObject(guid);
				}
			}
		}
	}
}