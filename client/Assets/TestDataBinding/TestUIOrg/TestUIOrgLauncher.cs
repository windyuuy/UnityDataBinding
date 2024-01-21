using System;
using System.Collections;
using System.Collections.Generic;
using gcc.layer;
using UnityEngine;

public class TestUIOrgLauncher : MonoBehaviour
{
    public UILayerRoot layerRoot;
    // Start is called before the first frame update
    void Start()
    {
        var layerInit=new UILayerInit();
        layerInit.Init(layerRoot);

        HomeAPI.ShowUIExample1();
    }

    private void Update()
    {
        VM.Tick.Next();
    }
}
