using UnityEngine;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyGridLayoutGroup), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom Editor for the GridLayout Component.
    /// Extend this class to write a custom editor for a component derived from GridLayout.
    /// </summary>
    public class MyGridLayoutGroupEditor : GridLayoutGroupEditor
    {
        SerializedProperty syncSiblingIndex;
        SerializedProperty hideUnused;

        protected override void OnEnable()
        {
            base.OnEnable();
            syncSiblingIndex = serializedObject.FindProperty("syncSiblingIndex");
            hideUnused = serializedObject.FindProperty("hideUnused");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            serializedObject.Update();
            EditorGUILayout.PropertyField(syncSiblingIndex, true);
            EditorGUILayout.PropertyField(hideUnused, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
