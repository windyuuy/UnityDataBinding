using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
/// <summary>
/// ������ �༭��
/// <para>ZhangYu 2018-06-21</para>
/// </summary>
//[CanEditMultipleObjects][CustomEditor(typeof(ClassName))]
public class RenameEditor : Editor
{

    // ����GUI
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawProperties();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }

    // ��������
    protected virtual void DrawProperty(string property, string label)
    {
        SerializedProperty pro = serializedObject.FindProperty(property);
        if (pro != null) EditorGUILayout.PropertyField(pro, new GUIContent(label), true);
    }

    // ������������
    protected virtual void DrawProperties()
    {
        // ��ȡ���ͺͿ����л�����
        Type type = target.GetType();
        List<FieldInfo> fields = new List<FieldInfo>();
        FieldInfo[] array = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        fields.AddRange(array);

        // ��ȡ����Ŀ����л�����
        while (IsTypeCompatible(type.BaseType) && type != type.BaseType)
        {
            type = type.BaseType;
            array = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            fields.InsertRange(0, array);
        }

        // ������������
        for (int i = 0; i < fields.Count; i++)
        {
            FieldInfo field = fields[i];

            // �ǹ��е��������[SerializeField]���Ե�����
            if (!field.IsPublic)
            {
                object[] serials = field.GetCustomAttributes(typeof(SerializeField), true);
                if (serials.Length == 0) continue;
            }

            // ���е��������[HideInInspector]���Ե�����
            object[] hides = field.GetCustomAttributes(typeof(HideInInspector), true);
            if (hides.Length != 0) continue;

            // ���Ʒ�������������
            RenameInEditorAttribute[] atts = (RenameInEditorAttribute[])field.GetCustomAttributes(typeof(RenameInEditorAttribute), true);
            DrawProperty(field.Name, atts.Length == 0 ? field.Name : atts[0].name);
        }

    }

    // �ű������Ƿ�������л�����
    protected virtual bool IsTypeCompatible(Type type)
    {
        if (type == null || !(type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(ScriptableObject))))
            return false;
        return true;
    }

}
#endif
