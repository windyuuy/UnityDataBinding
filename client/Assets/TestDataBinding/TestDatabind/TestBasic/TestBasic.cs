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

		private TRawData RawData => (TRawData)_rawData;
		protected override void InitTestData()
		{
			this._rawData = new TRawData()
			{
				kk = "kjkj",
			};
		}
		public override void Test()
		{
			// case 1
			this.Tick();
			var labelNode = this.SeekNodeByName("Label");
			var label = labelNode.GetComponent<Text>();
			var sample1 = "hello";
			this.RawData.kk = sample1;
			this.Tick();
			Assert(this.RawData.kk == label.text);
			var Node4 = this.SeekNodeByName("Node4");
			Node4.SetParent(null);
			var sample2 = "hello2";
			this.RawData.kk = sample2;
			this.Tick();
			Assert(label.text == sample1);
			Node4.SetParent(this.SeekNodeByName("Node2"));
			Assert(label.text == sample2);
			var sample3 = "hello3";
			this.RawData.kk = sample3;
			this.Tick();
			Assert(label.text == sample3);
			var parent0 = labelNode.parent;
			labelNode.SetParent(null);
			this.RawData.kk = "hello5";
			this.Tick();
			Assert(label.text == sample3);
			labelNode.SetParent(parent0);
			Assert(label.text == "hello5");
			label.text = "done";

		}

	}
}
