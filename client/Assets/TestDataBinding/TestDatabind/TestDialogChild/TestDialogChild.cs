using UnityEngine;
using UnityEngine.UI;

namespace DataBinding.UIBind
{
	class C1Item : IStdHost
	{
		public string AV2 { get; set; }
	}

	class TRawData:IStdHost
	{
		public C1Item C1 { get; set; } = new C1Item();
	}
	public class TestDialogChild : TestBase
	{

		TRawData RawData => (TRawData)_rawData;

		readonly C1Item _rawData2 = new C1Item()
		{
			AV2 = "BBB",
		};

		readonly TRawData rawData3 = new TRawData()
		{
			C1 = new C1Item()
			{
				AV2 = "CCC",
			},
		};

		protected override void InitTestData()
		{
			this._rawData = new TRawData()
			{
				C1 = new C1Item()
				{
					AV2 = "AAA",
				},
			};
		}

		public override void Test()
		{
			this.TestCustomData();
			this.TestAutoBind();
			this.TestAutoBindSubKey();
		}

		public virtual void TestCustomData()
		{
			var CustomDataNode = this.CN("CustomData")!;
			var label = CustomDataNode.CN("Label")?.GetComponent<Text>()!;
			var ccDialogChild = CustomDataNode.GetComponent<CCDialogChild>()!;
			Assert(label.text == "label");
			this.Tick();
			Assert(label.text == "label");
			ccDialogChild.ObserveData(this._rawData2);
			this.Tick();
			Assert(label.text == this._rawData2.AV2);
		}

		public virtual void TestAutoBindSubKey()
		{
			var AutoBindSubKeyNode = this.CN("AutoBindSubKey")!;
			var label = AutoBindSubKeyNode.CN("Label")?.GetComponent<Text>()!;
			this.Tick();
			Assert(label.text == this.RawData.C1.AV2);
			this.ObserveData(this.rawData3);
			this.Tick();
			Assert(label.text == this.rawData3.C1.AV2);
		}

		public virtual void TestAutoBind()
		{
			var AutoBindNode = this.CN("AutoBind")!;
			var label = AutoBindNode.CN("Label")?.GetComponent<Text>()!;
			var ccDialogChild = AutoBindNode.GetComponent<CCDialogChild>()!;
			this.Tick();
			Assert(label.text == this.RawData.C1.AV2);
			ccDialogChild.ObserveData(this.rawData3);
			this.Tick();
			Assert(label.text == this.rawData3.C1.AV2);
		}

	}
}
