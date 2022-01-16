using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBindService;
using UnityEditor.Callbacks;
using UnityEditor;

public class DataBindEntry
{
    private static bool hasGen = false;
    [PostProcessBuild(1000)]
    private static void OnPostprocessBuildPlayer(BuildTarget buildTarget, string buildPath)
    {
        hasGen = false;
    }

    [PostProcessScene]
    public static void TestInjectMothodOnPost()
    {
        if (hasGen == true) return;
        hasGen = true;

        TestInjectMothod();
    }
    [InitializeOnLoadMethod]
    public static void TestInjectMothod()
    {

        var pathToBuiltProject = @".\Library\ScriptAssemblies\Assembly-CSharp.dll";
        //var fss2 = new System.IO.FileStream(pathToBuiltProject, System.IO.FileMode.OpenOrCreate);
        Debug.Log(pathToBuiltProject);
        //try
        //{
        BindEntry.InjectDataBind(pathToBuiltProject, new InjectOptions()
            {
                useSymbols = true,
            });

        //}catch(System.Exception e)
        //{
        //    Debug.LogError(e);
        //}
    }

}
