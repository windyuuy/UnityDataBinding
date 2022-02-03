using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// 重命名 编辑器
/// <para>ZhangYu 2018-06-21</para>
/// </summary>
//[CanEditMultipleObjects][CustomEditor(typeof(ClassName))]
public class RenameEditor : Editor
{

    // 绘制GUI
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        drawProperties();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }

    // 绘制属性
    protected virtual void drawProperty(string property, string label)
    {
        SerializedProperty pro = serializedObject.FindProperty(property);
        if (pro != null) EditorGUILayout.PropertyField(pro, new GUIContent(label), true);
    }

    // 绘制所有属性
    protected virtual void drawProperties()
    {
        // 获取类型和可序列化属性
        Type type = target.GetType();
        List<FieldInfo> fields = new List<FieldInfo>();
        FieldInfo[] array = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        fields.AddRange(array);

        // 获取父类的可序列化属性
        while (IsTypeCompatible(type.BaseType) && type != type.BaseType)
        {
            type = type.BaseType;
            array = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            fields.InsertRange(0, array);
        }

        // 绘制所有属性
        for (int i = 0; i < fields.Count; i++)
        {
            FieldInfo field = fields[i];

            // 非公有但是添加了[SerializeField]特性的属性
            if (!field.IsPublic)
            {
                object[] serials = field.GetCustomAttributes(typeof(SerializeField), true);
                if (serials.Length == 0) continue;
            }

            // 公有但是添加了[HideInInspector]特性的属性
            object[] hides = field.GetCustomAttributes(typeof(HideInInspector), true);
            if (hides.Length != 0) continue;

            // 绘制符合条件的属性
            RenameInEditorAttribute[] atts = (RenameInEditorAttribute[])field.GetCustomAttributes(typeof(RenameInEditorAttribute), true);
            drawProperty(field.Name, atts.Length == 0 ? field.Name : atts[0].name);
        }

    }

    // 脚本类型是否符合序列化条件
    protected virtual bool IsTypeCompatible(Type type)
    {
        if (type == null || !(type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(ScriptableObject))))
            return false;
        return true;
    }

}
