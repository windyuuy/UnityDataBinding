// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UISys.Runtime;
// using UnityEditor;
// using UnityEngine;
//
// namespace UISys.Editor
// {
// 	[CanEditMultipleObjects, CustomEditor(typeof(UILayer), true)]
// 	public class UILayerEditor : UnityEditor.Editor
// 	{
// 		const string UICoverMark = "UICover";
//
// 		public override void OnInspectorGUI()
// 		{
// 			serializedObject.UpdateIfRequiredOrScript();
//
// 			serializedObject.DrawScriptView();
//
// 			var anyChanged = false;
//
// 			var comp = (UILayer)this.target;
//
// 			var tagsCompProp = serializedObject.FindProperty("tagsComp");
// 			EditorGUI.BeginChangeCheck();
// 			EditorGUILayout.PropertyField(tagsCompProp);
// 			if (EditorGUI.EndChangeCheck())
// 			{
// 				anyChanged = true;
// 			}
//
// 			if (tagsCompProp.boxedValue == null)
// 			{
// 				var transformTagsComp = comp.GetComponent<TransformTagsComp>();
// 				tagsCompProp.objectReferenceValue = transformTagsComp;
// 				if (transformTagsComp != null)
// 				{
// 					anyChanged = true;
// 				}
// 			}
//
// 			var presetTagValues = comp.LayerTags;
// 			var presetTags = presetTagValues;
//
// 			#region LayerTags
//
// 			string mainTag = null;
// 			bool isUICover = false;
// 			if (presetTagValues.Length > 0)
// 			{
// 				var index = Array.IndexOf(presetTagValues, UICoverMark);
// 				isUICover = index >= 0;
// 			}
//
// 			var rect = EditorGUILayout.GetControlRect(true, 22);
// 			EditorGUI.BeginChangeCheck();
// 			var isUICoverSelected = EditorGUI.Toggle(rect, "全屏界面", isUICover);
// 			if (EditorGUI.EndChangeCheck())
// 			{
// 				anyChanged = true;
// 			}
//
// 			if (anyChanged)
// 			{
// 				var sr = new SerializedObject(tagsCompProp.objectReferenceValue);
// 				sr.Update();
// 				var tagsProp = sr.FindProperty("tags");
// 				// var tagsProp = tagsCompProp.FindPropertyRelative("tags");
// 				if (isUICoverSelected && Array.IndexOf(comp.LayerTags, UICoverMark) == -1)
// 				{
// 					// comp.LayerTags = comp.LayerTags.Append(UICoverMark).ToArray();
// 					tagsProp.InsertArrayElementAtIndex(1);
// 					tagsProp.GetArrayElementAtIndex(1).stringValue = UICoverMark;
// 				}
// 				else if (!isUICoverSelected && Array.IndexOf(comp.LayerTags, UICoverMark) >= 0)
// 				{
// 					// comp.LayerTags = comp.LayerTags.Where(tag => tag != UICoverMark).ToArray();
// 					for (var i = 0; i < tagsProp.arraySize; i++)
// 					{
// 						if (tagsProp.GetArrayElementAtIndex(i).stringValue == UICoverMark)
// 						{
// 							tagsProp.DeleteArrayElementAtIndex(i);
// 							break;
// 						}
// 					}
// 				}
//
// 				sr.ApplyModifiedProperties();
//
// 				serializedObject.ApplyModifiedProperties();
// 			}
//
// 			#endregion
//
// 			// base.DrawDefaultInspector();
// 		}
//
// 	}
// }