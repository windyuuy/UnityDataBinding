using System;
using System.Collections.Generic;
using System.Linq;
using UISys.Runtime;
using UnityEditor;
using UnityEngine;

namespace UISys.Editor
{
	[CanEditMultipleObjects, CustomEditor(typeof(TransformTagsComp), true)]
	public class TransformTagsCompEditor : UnityEditor.Editor
	{
		private string[] _presetTags;
		private string[] _presetTagValues;

		static readonly List<string> TagList = new();

		public override void OnInspectorGUI()
		{
			var anyChanged = false;

			serializedObject.Update();

			serializedObject.DrawScriptView();

			var configProp = serializedObject.FindProperty("config");
			var tagsProp = serializedObject.FindProperty("tags");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(configProp);
			if (EditorGUI.EndChangeCheck())
			{
				anyChanged = true;
			}

			var comp = (TransformTagsComp)target;
			_presetTagValues = comp.PresetTags;
			_presetTags = _presetTagValues;

			if (_presetTagValues.Length > 0)
			{
				#region LayerTags

				string mainTag = null;
				if (tagsProp.arraySize > 0)
				{
					var orderTag = tagsProp.GetArrayElementAtIndex(0).stringValue;
					var curIndex0 = Array.IndexOf(_presetTagValues, orderTag);
					if (curIndex0 >= 0)
					{
						mainTag = _presetTags[curIndex0];
					}
				}

				var curMask = 0;
				if (tagsProp.arraySize > 0)
				{
					for (var ii = 0; ii < tagsProp.arraySize; ii++)
					{
						var tag = tagsProp.GetArrayElementAtIndex(ii).stringValue;
						var index = Array.IndexOf(_presetTags, tag);
						curMask |= (1 << index);
					}
				}

				Rect rect = EditorGUILayout.GetControlRect(true, 22);
				var curIndex = Array.IndexOf(_presetTagValues, mainTag);
				EditorGUI.BeginChangeCheck();
				var mainIndex = EditorGUI.Popup(rect, "MainTag", curIndex, _presetTags);
				if (EditorGUI.EndChangeCheck())
				{
					anyChanged = true;

					if (curIndex >= 0)
					{
						curMask &= ~(1 << curIndex);
					}

					if (mainIndex >= 0)
					{
						curMask |= (1 << mainIndex);
					}
				}

				rect = EditorGUILayout.GetControlRect(true, 22);
				EditorGUI.BeginChangeCheck();
				var mask = EditorGUI.MaskField(rect, "Tags", curMask, _presetTags);
				if (EditorGUI.EndChangeCheck())
				{
					anyChanged = true;
				}

				if (anyChanged)
				{
					TagList.Clear();

					if (mask != 0)
					{
						if (curIndex >= 0)
						{
							mask &= ~(1 << mainIndex);
							var selectMainTag = _presetTagValues[mainIndex];
							TagList.Add(selectMainTag);
						}
					}

					if (mask != 0)
					{
						var index = 0;
						while (mask != 0 && index < _presetTagValues.Length)
						{
							if ((mask & 1) == 1)
							{
								var selectTag = _presetTagValues[index];
								TagList.Add(selectTag);
							}

							mask >>= 1;
							index++;
						}
					}

					tagsProp.arraySize = TagList.Count;
					for (var ii = 0; ii < TagList.Count; ii++)
					{
						tagsProp.GetArrayElementAtIndex(ii).stringValue = TagList[ii];
					}
				}

				EditorGUILayout.PropertyField(tagsProp, TagsViewLabel);
			}

			#endregion

			if (anyChanged)
			{
				serializedObject.ApplyModifiedProperties();

				((TransformTagsComp)target).EmitTagsChanged();
			}

			// base.DrawDefaultInspector();
		}

		protected static readonly GUIContent TagsViewLabel = new GUIContent("TagsView", "readonly tags view");
	}
}