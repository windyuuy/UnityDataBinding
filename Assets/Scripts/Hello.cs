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

        sampleHost.Watch("QQ+hello.KKK", (host, value, oldValue) =>
        {
            Debug.Log("value changed;");
            Debug.Log(value);
        });
        sampleHost.QQ = 2134;
        sampleHost.hello.KKK = 3242;
        vm.Tick.next();

    }

    void Update()
    {
        
    }
}
