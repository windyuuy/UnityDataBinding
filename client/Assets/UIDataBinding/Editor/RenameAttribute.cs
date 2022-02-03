using UnityEngine;
#if UNITY_EDITOR
using System;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;
#endif

/// <summary>
/// 重命名属性
/// <para>ZhangYu 2018-06-21</para>
/// </summary>

#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Field)]
#endif
public class RenameAttribute : PropertyAttribute
{

    /// <summary> 枚举名称 </summary>
    public string name = "";
    /// <summary> 文本颜色 </summary>
    public string htmlColor = "#7E7E7E";

    /// <summary> 重命名属性 </summary>
    /// <param name="name">新名称</param>
    public RenameAttribute(string name)
    {
        this.name = name;
    }

    /// <summary> 重命名属性 </summary>
    /// <param name="name">新名称</param>
    /// <param name="htmlColor">文本颜色 例如："#FFFFFF" 或 "black"</param>
    public RenameAttribute(string name, string htmlColor)
    {
        this.name = name;
        this.htmlColor = htmlColor;
    }

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RenameAttribute))]
public class RenameDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 替换属性名称
        RenameAttribute rename = (RenameAttribute)attribute;
        label.text = rename.name;

        // 重绘GUI
        Color defaultColor = EditorStyles.label.normal.textColor;
        EditorStyles.label.normal.textColor = htmlToColor(rename.htmlColor);
        bool isElement = Regex.IsMatch(property.displayName, "Element \\d+");
        if (isElement) label.text = property.displayName;
        if (property.propertyType == SerializedPropertyType.Enum)
        {
            drawEnum(position, property, label);
        }
        else
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
        EditorStyles.label.normal.textColor = defaultColor;
    }

    // 绘制枚举类型
    private void drawEnum(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();

        // 获取枚举相关属性
        Type type = fieldInfo.FieldType;
        string[] names = property.enumNames;
        string[] values = new string[names.Length];
        Array.Copy(names, values, names.Length);
        while (type.IsArray) type = type.GetElementType();

        // 获取枚举所对应的RenameAttribute
        for (int i = 0; i < names.Length; i++)
        {
            FieldInfo info = type.GetField(names[i]);
            RenameAttribute[] atts = (RenameAttribute[])info.GetCustomAttributes(typeof(RenameAttribute), true);
            if (atts.Length != 0) values[i] = atts[0].name;
        }

        // 重绘GUI
        int index = EditorGUI.Popup(position, label.text, property.enumValueIndex, values);
        if (EditorGUI.EndChangeCheck() && index != -1) property.enumValueIndex = index;
    }

    /// <summary> Html颜色转换为Color </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static Color htmlToColor(string hex)
    {
        // 编辑器默认颜色
        if (string.IsNullOrEmpty(hex)) return new Color(0.705f, 0.705f, 0.705f);

#if UNITY_EDITOR
        // 转换颜色
        hex = hex.ToLower();
        if (hex.IndexOf("#") == 0 && hex.Length == 7)
        {
            int r = Convert.ToInt32(hex.Substring(1, 2), 16);
            int g = Convert.ToInt32(hex.Substring(3, 2), 16);
            int b = Convert.ToInt32(hex.Substring(5, 2), 16);
            return new Color(r / 255f, g / 255f, b / 255f);
        }
        else if (hex == "red")
        {
            return Color.red;
        }
        else if (hex == "green")
        {
            return Color.green;
        }
        else if (hex == "blue")
        {
            return Color.blue;
        }
        else if (hex == "yellow")
        {
            return Color.yellow;
        }
        else if (hex == "black")
        {
            return Color.black;
        }
        else if (hex == "white")
        {
            return Color.white;
        }
        else if (hex == "cyan")
        {
            return Color.cyan;
        }
        else if (hex == "gray")
        {
            return Color.gray;
        }
        else if (hex == "grey")
        {
            return Color.grey;
        }
        else if (hex == "magenta")
        {
            return Color.magenta;
        }
        else if (hex == "orange")
        {
            return new Color(1, 165 / 255f, 0);
        }
#endif

        return new Color(0.705f, 0.705f, 0.705f);
    }

}
#endif

/// <summary>
/// 添加标题属性
/// <para>ZhangYu 2018-06-21</para>
/// </summary>
#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Field)]
#endif
public class TitleAttribute : PropertyAttribute
{

    /// <summary> 标题名称 </summary>
    public string title = "";
    /// <summary> 文本颜色 </summary>
    public string htmlColor = "#B3B3B3";

    /// <summary> 在属性上方添加一个标题 </summary>
    /// <param name="title">标题名称</param>
    public TitleAttribute(string title)
    {
        this.title = title;
    }

    /// <summary> 在属性上方添加一个标题 </summary>
    /// <param name="title">标题名称</param>
    /// <param name="htmlColor">文本颜色 例如："#FFFFFF" 或 "black"</param>
    public TitleAttribute(string title, string htmlColor)
    {
        this.title = title;
        this.htmlColor = htmlColor;
    }

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TitleAttribute))]
public class TitleAttributeDrawer : DecoratorDrawer
{

    // 文本样式
    private GUIStyle style = new GUIStyle(EditorStyles.label);

    public override void OnGUI(Rect position)
    {
        // 获取Attribute
        TitleAttribute rename = (TitleAttribute)attribute;
        style.fixedHeight = 18;
        style.normal.textColor = RenameDrawer.htmlToColor(rename.htmlColor);

        // 重绘GUI
        position = EditorGUI.IndentedRect(position);
        GUI.Label(position, rename.title, style);
    }

    public override float GetHeight()
    {
        return base.GetHeight() - 3;
    }

}
#endif

/// <summary>
/// 重命名脚本编辑器中的属性名称
/// <para>ZhangYu 2018-06-21</para>
/// </summary>
#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Field)]
#endif
public class RenameInEditorAttribute : PropertyAttribute
{

    /// <summary> 新名称 </summary>
    public string name = "";

    /// <summary> 重命名属性 </summary>
    /// <param name="name">新名称</param>
    public RenameInEditorAttribute(string name)
    {
        this.name = name;
    }

}
