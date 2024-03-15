using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DataBind.VM.Tick.Next();
    }

    public void OnClick()
    {
        //TestData.Inst.testkey = "hellox";
        //TestData.Inst.data2=new TestData2();
        TestData.Inst.data2.testkey2 = 655; ;
        var kk=new TestData3();
    }
}
