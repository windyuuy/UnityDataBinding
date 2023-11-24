using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class TestUIATool : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Test1();
        Test2();
        Test3();
        Test4();
    }

    public void Test1()
    {
        var prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/TestDataBinding/TestDatabind/TestCompBind/TestCompBind.prefab");
        var codeText = new UIDataBindExposer().ExposeBind(prefab.transform, "UIAOut1");
        File.WriteAllText("Assets/TestDataBinding/TestUIATool/Output/UIAOut1.cs", codeText, Encoding.UTF8);
    }

    public void Test2()
    {
        var prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/TestDataBinding/TestDatabind/TestContainerBind/TestContainer.prefab");
        var codeText = new UIDataBindExposer().ExposeBind(prefab.transform, "UIAOut2");
        File.WriteAllText("Assets/TestDataBinding/TestUIATool/Output/UIAOut2.cs", codeText, Encoding.UTF8);
    }

    public void Test3()
    {
        var prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/TestDataBinding/TestDatabind/TestBasic/TestBasic.prefab");
        var codeText = new UIDataBindExposer().ExposeBind(prefab.transform, "UIAOut3");
        File.WriteAllText("Assets/TestDataBinding/TestUIATool/Output/UIAOut3.cs", codeText, Encoding.UTF8);
    }

    public void Test4()
    {
        var prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/TestDataBinding/TestDatabind/TestDialogChild/TestDialogChild.prefab");
        var codeText = new UIDataBindExposer().ExposeBind(prefab.transform, "UIAOut4");
        File.WriteAllText("Assets/TestDataBinding/TestUIATool/Output/UIAOut4.cs", codeText, Encoding.UTF8);
    }

}
