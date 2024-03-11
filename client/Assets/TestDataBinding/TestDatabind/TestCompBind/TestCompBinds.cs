using DataBinding;
using DataBinding.UIBind;
using Game.Diagnostics.IO;
using TestDataBinding.TestBasic;
using UnityEngine;
using UnityEngine.UI;

namespace TestDataBinding.Tests.TestCompBinds
{
	using TButtonBindCallback = System.Action<DataBinding.UIBind.ButtonBindComp, double>;

	public class TRawData:IStdHost
	{
		public bool enabled { get; set; } = false;
		public bool gray { get; set; } = false;
		public TButtonBindCallback doClick { get; set; } = (ButtonBindComp binder, double index) =>
		{
			Console.Log("click", binder.name);
		};
		public string label { get; set; } = "hello";
		public string spriteUrl { get; set; } = "doge_cartoon";
		public bool visible { get; set; } = false;
		public float progress { get; set; } = 0;
		public bool isToggleCheck { get; set; } = false;
	}

	public class TestCompBinds : TestBase
	{

		TRawData RawData => (TRawData)_rawData;

		protected override void InitTestData()
		{
			this._rawData = new TRawData()
			{
				enabled = false,
				gray = false,
				doClick = (ButtonBindComp binder, double index) =>
				{
					Console.Log("click0", binder.name);
				},
				label = "hello",
				spriteUrl = "doge_cartoon",
				visible = false,
				progress = 0,
				isToggleCheck = false,
			};
		}

		public override void Test()
		{
			var label0 = "label";
			// var label1 = "hello";


			var BtnBind = this.CN("Button")!.GetComponent<ButtonBindComp>();
			var Btn = BtnBind.target!;
			this.RawData.doClick = (ButtonBindComp binder, double index) =>
			{
				Console.Log("click2", binder.name);
			}; ;
			this.RawData.enabled = true;
			this.Tick();
			Assert(Btn.interactable == this.RawData.enabled);
			this.RawData.gray = true;
			this.Tick();
			Assert(BtnBind.isGray == this.RawData.gray);


			var LabelBind = this.CN("Label")!.GetComponent<SimpleBindComp>();
			var Label0 = LabelBind.target as Text;
			Assert(Label0.text == this.RawData.label);
			this.RawData.label = "jkkjkfje";
			this.Tick();
			Assert(Label0.text == this.RawData.label);
			this.RawData.label = label0;
			this.Tick();
			Assert(Label0.text == this.RawData.label);


			var SpriteBind = this.CN("Sprite")!.GetComponent<SimpleBindComp>();
			var Sprite0 = SpriteBind.target as Image;
			Assert(SpriteBind.spriteTextureUrl == this.RawData.spriteUrl);
			this.RawData.spriteUrl = "doge_cartoon";
			this.Tick();
			Assert(SpriteBind.spriteTextureUrl == this.RawData.spriteUrl);
			this.RawData.spriteUrl = "";
			this.Tick();
			Assert(Sprite0.sprite == null);
			this.RawData.spriteUrl = "doge_cartoon";
			this.Tick();
			Assert(SpriteBind.spriteTextureUrl == this.RawData.spriteUrl);


			this.Tick();
			var VisibleBind = this.CN("Visible")!.GetComponent<ActiveBindComp>();
			var Visible0 = VisibleBind.gameObject;
			Assert(Visible0.activeSelf == this.RawData.visible);
			this.RawData.visible = true;
			this.Tick();
			Assert(Visible0.activeSelf == this.RawData.visible);
			this.RawData.visible = false;
			this.Tick();
			// @ts-ignore;
			Assert(Visible0.activeSelf == this.RawData.visible);
			this.RawData.visible = true;
			this.Tick();
			Assert(Visible0.activeSelf == this.RawData.visible);
			this.RawData.visible = false;
			this.Tick();
			Assert(Visible0.activeSelf == this.RawData.visible);


			var ProgressBind = this.CN("ProgressBar")!.GetComponent<SimpleBindComp>();
			var Progress0 = ProgressBind.target as Slider;
			Assert(Progress0.value == this.RawData.progress);
			this.RawData.progress = 1;
			this.Tick();
			Assert(Progress0.value == this.RawData.progress);


			var ToggleBind = this.CN("Toggle")!.GetComponent<ToggleBindComp>();
			var Toggle0 = ToggleBind.target as Toggle;
			Assert(Toggle0.isOn == this.RawData.isToggleCheck);
			this.RawData.isToggleCheck = !this.RawData.isToggleCheck;
			this.Tick();
			Assert(Toggle0.isOn == this.RawData.isToggleCheck);
		}

		public override void TestLazy()
		{
			var label0 = "label";
			var label1 = "hello";
			this.RawData.label = label1;

			this.Tick();
			var VisibleBind = this.CN("Visible")!.GetComponent<ActiveBindComp>();
			var VisibleLable = VisibleBind.GetComponent<Text>();
			var VisibleSubBind = this.CN("Visible")!.CN("Label")!.GetComponent<SimpleBindComp>();
			var VisibleSubLabel = VisibleSubBind.GetComponent<Text>();
			var Visible0 = VisibleBind.gameObject;
			Assert(Visible0.activeSelf == this.RawData.visible);
			Assert(VisibleLable.text == label0);
			Assert(VisibleSubLabel.text == label0);
			this.RawData.visible = true;
			this.Tick();
			Assert(Visible0.activeSelf == this.RawData.visible);
			Assert(VisibleLable.text == this.RawData.label);
			Assert(VisibleSubLabel.text == this.RawData.label);
			this.RawData.visible = false;
			this.RawData.label = label0;
			this.Tick();
			Assert(Visible0.activeSelf == this.RawData.visible);
			Assert(VisibleLable.text == label1);
			Assert(VisibleSubLabel.text == label1);
			this.RawData.visible = true;
			this.Tick();
			Assert(Visible0.activeSelf == this.RawData.visible);
			Assert(VisibleLable.text == this.RawData.label);
			Assert(VisibleSubLabel.text == this.RawData.label);
			this.RawData.label = label0;
			this.Tick();
			Assert(Visible0.activeSelf == this.RawData.visible);
			Assert(VisibleLable.text == this.RawData.label);
			Assert(VisibleSubLabel.text == this.RawData.label);

		}

	}
}
