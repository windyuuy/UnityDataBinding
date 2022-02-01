using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBinding;
using UI.DataBinding;

namespace TestRandom
{
    class TestData:IStdHost
    {
        public string hello { get; set; } = "hello1";
    }
    public class RandomTest : MonoBehaviour
    {
        TestData testData=new TestData();
        // Start is called before the first frame update
        void Awake()
        {
            var ccDataHost = this.GetComponent<CCDataHost>();
            ccDataHost.observeData(this.testData);
        }

    }

}