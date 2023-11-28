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

        var layerMG = layerRoot.LayerManager;
        layerMG.ShowLayer(new ShowLayerParam(
            "UIExample1", null, "Assets/TestDataBinding/TestUIOrg/UIExample1/Layer/Viarant/UIExample1.prefab"));
    }

    private void Update()
    {
        VM.Tick.Next();
    }
}
