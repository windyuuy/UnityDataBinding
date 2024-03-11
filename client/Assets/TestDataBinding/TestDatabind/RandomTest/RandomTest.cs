using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBinding;
using DataBinding.UIBind;

namespace TestRandom
{
	class TestData : IStdHost
	{
		public string hello { get; set; } = "hello1";
		public string imageSrc { get; set; } = "image2";
	}
	public class RandomTest : MonoBehaviour
	{
		TestData testData = new TestData();
		// Start is called before the first frame update
		void Awake()
		{
			var ccDataHost = this.GetOrAddComponent<DataHostComp>();
			ccDataHost.ObserveData(this.testData);

			this.testData.hello = "hello2";
			this.testData.imageSrc = "doge_cartoon";
			VM.Tick.Next();
		}

	}

}