using UnityEngine;
using UnityEngine.UI;
using DataBinding;

namespace DataBinding.UIBind.Tests.TestBasic
{
	class TRawData : IStdHost
	{
		public string kk { get; set; }
	}

	public class TestBasic : TestBase
	{

		//#endregion

		private TRawData rawData => (TRawData)_rawData;
		protected override void initTestData()
		{
			this._rawData = new TRawData()
			{
				kk = "kjkj",
			};
		}
		public override void test()
		{
			// case 1
			this.tick();
			var labelNode = this.seekNodeByName("Label");
			var label = labelNode.GetComponent<Text>();
			var sample1 = "hello";
			this.rawData.kk = sample1;
			this.tick();
			assert(this.rawData.kk == label.text);
			var Node4 = this.seekNodeByName("Node4");
			Node4.SetParent(null);
			var sample2 = "hello2";
			this.rawData.kk = sample2;
			this.tick();
			assert(label.text == sample1);
			Node4.SetParent(this.seekNodeByName("Node2"));
			assert(label.text == sample2);
			var sample3 = "hello3";
			this.rawData.kk = sample3;
			this.tick();
			assert(label.text == sample3);
			var parent0 = labelNode.parent;
			labelNode.SetParent(null);
			this.rawData.kk = "hello5";
			this.tick();
			assert(label.text == sample3);
			labelNode.SetParent(parent0);
			assert(label.text == "hello5");
			label.text = "done";

		}

	}
}
