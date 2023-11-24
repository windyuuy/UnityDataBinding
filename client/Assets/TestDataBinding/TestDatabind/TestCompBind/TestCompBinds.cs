using Game.Diagnostics.IO;
using UnityEngine;
using UnityEngine.UI;

namespace DataBinding.UIBind.Tests.TestCompBinds
{
	using TButtonBindCallback = System.Action<DataBinding.UIBind.CCButtonBind, double>;
	using number = System.Double;

	public class TRawData:IStdHost
	{
		public bool enabled { get; set; } = false;
		public bool gray { get; set; } = false;
		public TButtonBindCallback doClick { get; set; } = (CCButtonBind binder, double index) =>
		{
			Console.Log("click", binder.name);
		};
		public string label { get; set; } = "hello";
		public string spriteUrl { get; set; } = "doge_cartoon";
		public bool visible { get; set; } = false;
		public number progress { get; set; } = 0;
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
				doClick = (CCButtonBind binder, double index) =>
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


			var BtnBind = this.CN("Button")!.GetComponent<CCButtonBind>();
			var Btn = BtnBind.target!;
			this.RawData.doClick = (CCButtonBind binder, double index) =>
			{
				Console.Log("click2", binder.name);
			}; ;
			this.RawData.enabled = true;
			this.Tick();
			Assert(Btn.interactable == this.RawData.enabled);
			this.RawData.gray = true;
			this.Tick();
			Assert(BtnBind.isGray == this.RawData.gray);


			var LabelBind = this.CN("Label")!.GetComponent<CCSimpleBind>();
			var Label0 = LabelBind.target as Text;
			Assert(Label0.text == this.RawData.label);
			this.RawData.label = "jkkjkfje";
			this.Tick();
			Assert(Label0.text == this.RawData.label);
			this.RawData.label = label0;
			this.Tick();
			Assert(Label0.text == this.RawData.label);


			var SpriteBind = this.CN("Sprite")!.GetComponent<CCSimpleBind>();
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
			var VisibleBind = this.CN("Visible")!.GetComponent<CCActiveBind>();
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


			var ProgressBind = this.CN("ProgressBar")!.GetComponent<CCSimpleBind>();
			var Progress0 = ProgressBind.target as Slider;
			Assert(Progress0.value == this.RawData.progress);
			this.RawData.progress = 1;
			this.Tick();
			Assert(Progress0.value == this.RawData.progress);


			var ToggleBind = this.CN("Toggle")!.GetComponent<CCToggleBind>();
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
			var VisibleBind = this.CN("Visible")!.GetComponent<CCActiveBind>();
			var VisibleLable = VisibleBind.GetComponent<Text>();
			var VisibleSubBind = this.CN("Visible")!.CN("Label")!.GetComponent<CCSimpleBind>();
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
