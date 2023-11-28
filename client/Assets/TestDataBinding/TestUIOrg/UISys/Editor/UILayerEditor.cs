using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(UILayer), true)]
public class UILayerEditor : Editor
{
    static string[] presetTags;
    static string[] presetTagValues;
    static UILayerEditor()
    {
        var fileds = typeof(LayerTags).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        presetTags = fileds.Select(f => f.Name).ToArray();
        presetTagValues = fileds.Select(f => f.GetValue(null) as string).ToArray();
    }

    static List<string> tagList = new List<string>();
    public override void OnInspectorGUI()
    {
        var anyChanged = false;

        serializedObject.Update();

        #region LayerTags
        SerializedProperty layerTagsProperty = serializedObject.FindProperty("layerTags");
        string mainTag = null;
        bool isUICover = false;
        const string UICoverMark = "UICover";
        if (layerTagsProperty.arraySize > 0)
        {
            var orderTag = layerTagsProperty.GetArrayElementAtIndex(0).stringValue;
            var curIndex0 = Array.IndexOf(presetTagValues, orderTag);
            if (curIndex0 == -1)
            {
                curIndex0 = 1;
            }
            mainTag = presetTags[curIndex0];

            for (var ii = 0;ii < layerTagsProperty.arraySize; ii++)
            {
                var ele = layerTagsProperty.GetArrayElementAtIndex(ii);
                if (ele.stringValue == UICoverMark)
                {
                    isUICover = true;
                }
            }
        }

        // 兼容旧的
        SerializedProperty tagProperty = serializedObject.FindProperty("tag");
        var isLayerOrder = tagProperty != null;
        if (isLayerOrder)
        {
            mainTag = presetTags[tagProperty.intValue];
        }

        var selectedMainTag = mainTag;
        Rect rect = EditorGUILayout.GetControlRect(true, 22);
        var curIndex = System.Array.IndexOf(presetTags, mainTag);
        EditorGUI.BeginChangeCheck();
        var index = EditorGUI.Popup(rect, "LayerTag", curIndex, presetTags);
        if (EditorGUI.EndChangeCheck())
        {
            if (index >= 0)
            {
                var selectTag = presetTags[index];

                if (selectTag != mainTag || isLayerOrder)
                {
                    anyChanged = true;
                    //Debug.Log($"[c] select {selectTag}");
                    var selectValue = presetTagValues[index];
                    selectedMainTag = selectValue;
                }
            }
        }
        
        rect = EditorGUILayout.GetControlRect(true, 22);
        EditorGUI.BeginChangeCheck();
        var isUICoverSelected = EditorGUI.Toggle(rect, "全屏界面", isUICover);
        if (EditorGUI.EndChangeCheck())
        {
            anyChanged = true;
        }

        if (anyChanged)
        {
            tagList.Clear();
            if (selectedMainTag != null)
            {
                tagList.Add(selectedMainTag);
            }

            if (isUICoverSelected)
            {
                tagList.Add(UICoverMark);
            }

            layerTagsProperty.ClearArray();
            for(var ii=0;ii<tagList.Count;ii++)
            {
                layerTagsProperty.InsertArrayElementAtIndex(ii);
                layerTagsProperty.GetArrayElementAtIndex(ii).stringValue = tagList[ii];
            }
        }
        #endregion

#if false
        #region IsCover

        SerializedProperty isCoverProperty = serializedObject.FindProperty("isCoverDefault");
        anyChanged |= DrawToggle("全屏遮盖", isCoverProperty);

        #endregion

        #region IsCover

        SerializedProperty allowCoverredProperty = serializedObject.FindProperty("allowCoverred");
        anyChanged |= DrawToggle("可被全屏遮盖", allowCoverredProperty);

        #endregion
#else
        SerializedProperty isCoverProperty = serializedObject.FindProperty("isCoverDefault");
        isCoverProperty.boolValue = serializedObject.targetObject is UIScene;
#endif

        if (anyChanged)
        {
            serializedObject.ApplyModifiedProperties();
        }

        base.DrawDefaultInspector();

    }

    public static bool DrawToggle(string title,SerializedProperty isCoverProperty)
    {
        EditorGUI.BeginChangeCheck();
        var isCover = EditorGUILayout.Toggle(title, isCoverProperty.boolValue);
        if (EditorGUI.EndChangeCheck())
        {
            isCoverProperty.boolValue = isCover;
            return true;
        }

        return false;
    }
    public static bool DrawToggle(string title,SerializedObject serializedObject, string propKey)
    {
        SerializedProperty allowCoverredProperty = serializedObject.FindProperty(propKey);
        return DrawToggle(title, allowCoverredProperty);
    }
}
