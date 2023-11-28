using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ParseJSDataBindAbstract;
using ParseJSDataBindAbstract.CodeWriter;
using UIDataBinding.UIATool;
using UnityEngine;
using VM;

public class UIDataBindExposer
{
    public EnvInfo EnvInfo;
    public string ExposeBind(string[] keys, string envName)
    {
        EnvInfo = new(envName);
        foreach (var key in keys)
        {
            var interpreter = new Interpreter(key);
            ParseJSDataBind.ParseTypeInfo(interpreter.Ast, EnvInfo);
            
        }
        
        var codeWriter = new CodeWriter();
        codeWriter.UnknownTypeMark = "object";
        var codeText = codeWriter.WriteCode(EnvInfo);
        return codeText;
    }

    public string ExposeBind(Transform node, string envName)
    {
        var keys = new UIDataBindScaner().ScanBindSentences(node).Distinct().ToArray();
        Debug.Log(JsonConvert.SerializeObject(keys));
        return ExposeBind(keys, envName);
    }
}

