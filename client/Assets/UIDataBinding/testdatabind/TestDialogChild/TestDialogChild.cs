using UnityEngine;
using UnityEngine.UI;

namespace UI.DataBinding
{
	class C1Item
	{
		public string AV2;
	}

	class TRawData
	{
		public C1Item C1 = new C1Item();
	}
	public class TestDialogChild : TestBase
	{

		TRawData rawData => (TRawData)_rawData;

		C1Item rawData2 = new C1Item()
		{
			AV2 = "BBB",
		};

		TRawData rawData3 = new TRawData()
		{
			C1 = new C1Item()
			{
				AV2 = "CCC",
			},
		};

		protected override void initTestData()
		{
			this._rawData = new TRawData()
            {
				C1=new C1Item()
                {
					AV2="AAA",
                },
            };
		}

		public override void test()
		{
			this.testCustomData();
			this.testAutoBind();
			this.testAutoBindSubKey();
		}

		public virtual void testCustomData()
		{
			var CustomDataNode = this.cn("CustomData")!;
			var label = CustomDataNode.cn("Label")?.GetComponent<Text>()!;
			var ccDialogChild = CustomDataNode.GetComponent<CCDialogChild>()!;
			assert(label.text == "label");
			this.tick();
			assert(label.text == "label");
			ccDialogChild.observeData(this.rawData2);
			this.tick();
			assert(label.text == this.rawData2.AV2);
		}

		public virtual void testAutoBindSubKey()
		{
			var AutoBindSubKeyNode = this.cn("AutoBindSubKey")!;
			var label = AutoBindSubKeyNode.cn("Label")?.GetComponent<Text>()!;
			this.tick();
			assert(label.text == this.rawData.C1.AV2);
			this.observeData(this.rawData3);
			this.tick();
			assert(label.text == this.rawData3.C1.AV2);
		}

		public virtual void testAutoBind()
		{
			var AutoBindNode = this.cn("AutoBind")!;
			var label = AutoBindNode.cn("Label")?.GetComponent<Text>()!;
			var ccDialogChild = AutoBindNode.GetComponent<CCDialogChild>()!;
			this.tick();
			assert(label.text == this.rawData.C1.AV2);
			ccDialogChild.observeData(this.rawData3);
			this.tick();
			assert(label.text == this.rawData3.C1.AV2);
		}

	}
}
