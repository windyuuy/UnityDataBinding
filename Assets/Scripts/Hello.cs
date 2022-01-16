using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBinding;

public class Hello : MonoBehaviour
{
    public void Start()
    {
        Debug.Log("lkjwef");
        var sampleHost = new SampleHost();

        sampleHost.GetHost()._Swatch("QQ+hello.KKK", (host, value, oldValue) =>
        {
            console.log("value changed;", value);
        });

        sampleHost.QQ = 2134;
        sampleHost.hello.KKK = 3244;
        vm.Tick.next();

    }

    void Update()
    {
        
    }
}
