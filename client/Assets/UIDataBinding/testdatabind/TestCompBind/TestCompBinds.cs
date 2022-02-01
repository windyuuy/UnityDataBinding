using UnityEngine;
using UnityEngine.UI;

namespace UI.DataBinding.Tests.TestCompBinds
{
	using number = System.Double;

	class TRawData
	{
		public bool enabled = false;
		public bool gray = false;
		public System.Action<Component> doClick = (Component target) =>
		{
			console.log("click", target.name);
		};
		public string label = "hello";
		public string spriteUrl = "textures/ui/common/pack/pop_btn_closed";
		public bool visible = false;
		public number progress = 0;
		public bool isToggleCheck = false;
	}

	public class TestCompBinds : TestBase
	{

		TRawData rawData => (TRawData)_rawData;

		protected override void initTestData()
		{
			this._rawData = new TRawData()
			{
				enabled = false,
				gray = false,
				doClick = (Component target) =>
				{
					console.log("click", target);
				},
				label = "hello",
				spriteUrl = "textures/ui/common/pack/pop_btn_closed",
				visible = false,
				progress = 0,
				isToggleCheck = false,
			};
		}

		public override void test()
		{
			var label0 = "label";
			// var label1 = "hello";


			var BtnBind = this.cn("Button")!.GetComponent<CCButtonBind>();
			var Btn = BtnBind.target!;
			this.rawData.enabled = true;
			this.tick();
			assert(Btn.interactable == this.rawData.enabled);
			this.rawData.gray = true;
			this.tick();
			assert(BtnBind.isGray == this.rawData.gray);


			var LabelBind = this.cn("Label")!.GetComponent<CCSimpleBind>();
			var Label0 = LabelBind.target as Text;
			assert(Label0.text == this.rawData.label);
			this.rawData.label = "jkkjkfje";
			this.tick();
			assert(Label0.text == this.rawData.label);
			this.rawData.label = label0;
			this.tick();
			assert(Label0.text == this.rawData.label);


			var SpriteBind = this.cn("Sprite")!.GetComponent<CCSimpleBind>();
			var Sprite0 = SpriteBind.target as Image;
			assert(SpriteBind.spriteTextureUrl == this.rawData.spriteUrl);
			this.rawData.spriteUrl = "textures/ui/common/pack/pop_btn_closed";
			this.tick();
			assert(SpriteBind.spriteTextureUrl == this.rawData.spriteUrl);
			this.rawData.spriteUrl = "";
			this.tick();
			assert(Sprite0.sprite == null);
			this.rawData.spriteUrl = "textures/ui/common/pack/pop_btn_closed";
			this.tick();
			assert(SpriteBind.spriteTextureUrl == this.rawData.spriteUrl);


			this.tick();
			var VisibleBind = this.cn("Visible")!.GetComponent<CCActiveBind>();
			var Visible0 = VisibleBind.gameObject;
			assert(Visible0.activeSelf == this.rawData.visible);
			this.rawData.visible = true;
			this.tick();
			assert(Visible0.activeSelf == this.rawData.visible);
			this.rawData.visible = false;
			this.tick();
			// @ts-ignore;
			assert(Visible0.activeSelf == this.rawData.visible);
			this.rawData.visible = true;
			this.tick();
			assert(Visible0.activeSelf == this.rawData.visible);
			this.rawData.visible = false;
			this.tick();
			assert(Visible0.activeSelf == this.rawData.visible);


			var ProgressBind = this.cn("ProgressBar")!.GetComponent<CCSimpleBind>();
			var Progress0 = ProgressBind.target as Slider;
			assert(Progress0.value == this.rawData.progress);
			this.rawData.progress = 1;
			this.tick();
			assert(Progress0.value == this.rawData.progress);


			var ToggleBind = this.cn("Toggle")!.GetComponent<CCSimpleBind>();
			var Toggle0 = ToggleBind.target as Toggle;
			assert(Toggle0.isOn == this.rawData.isToggleCheck);
			this.rawData.isToggleCheck = !this.rawData.isToggleCheck;
			this.tick();
			assert(Toggle0.isOn == this.rawData.isToggleCheck);
		}

		public override void testLazy()
		{
			var label0 = "label";
			var label1 = "hello";
			this.rawData.label = label1;

			this.tick();
			var VisibleBind = this.cn("Visible")!.GetComponent<CCActiveBind>();
			var VisibleLable = VisibleBind.GetComponent<Text>();
			var VisibleSubBind = this.cn("Visible")!.cn("Label")!.GetComponent<CCSimpleBind>();
			var VisibleSubLabel = VisibleSubBind.GetComponent<Text>();
			var Visible0 = VisibleBind.gameObject;
			assert(Visible0.activeSelf == this.rawData.visible);
			assert(VisibleLable.text == label0);
			assert(VisibleSubLabel.text == label0);
			this.rawData.visible = true;
			this.tick();
			assert(Visible0.activeSelf == this.rawData.visible);
			assert(VisibleLable.text == this.rawData.label);
			assert(VisibleSubLabel.text == this.rawData.label);
			this.rawData.visible = false;
			this.rawData.label = label0;
			this.tick();
			// @ts-ignore
			assert(Visible0.activeSelf == this.rawData.visible);
			// @ts-ignore
			assert(VisibleLable.text == label1);
			assert(VisibleSubLabel.text == label1);
			this.rawData.visible = true;
			this.tick();
			assert(Visible0.activeSelf == this.rawData.visible);
			assert(VisibleLable.text == this.rawData.label);
			assert(VisibleSubLabel.text == this.rawData.label);
			this.rawData.label = label0;
			this.tick();
			assert(Visible0.activeSelf == this.rawData.visible);
			assert(VisibleLable.text == this.rawData.label);
			assert(VisibleSubLabel.text == this.rawData.label);

		}

	}
}
