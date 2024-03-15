using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Console = EngineAdapter.Diagnostics.Console;

namespace DataBind.UIBind
{
	using TButtonBindCallback=System.Object;

	[AddComponentMenu("DataDrive/ButtonBind")]
	public class ButtonBindComp : DataBindBaseComp
	{
		[Rename("可交互")]
		public string kInteractive = "";

		[Rename("变灰")]
		public string kToGray = "";

		[Rename("点击响应")]
		public SimpleBindClickFuncInfo[] clickTriggers = Array.Empty<SimpleBindClickFuncInfo>();

		/**
		 * 更新显示状态
		 */
		protected override void OnBindItems()
		{
			this.CheckInteractive();
			this.CheckToGray();
			this.CheckClick();
		}

		public virtual bool CheckInteractive()
		{
			if (string.IsNullOrEmpty(this.kInteractive))
			{
				return false;
			}
			var node = this.transform;
			if (node == null)
			{
				return false;
			}
			this.WatchValueChange<bool>(this.kInteractive, (newValue, oldValue) =>
			{
				if (Utils.IsValid(node, true)) this.SetInteractive((bool)newValue);
			});
			return true;
		}

		protected virtual void SetInteractive(bool b)
		{
			var button = this.GetComponent<Button>();
			if (button)
			{
				button.interactable = b;
			}
		}

		protected virtual bool CheckToGray()
		{
			if (string.IsNullOrEmpty(this.kToGray))
			{
				return false;
			}
			var node = this.transform;
			if (node == null)
			{
				return false;
			}
			this.WatchValueChange<bool>(this.kToGray, (newValue, oldValue) =>
			{
				if (Utils.IsValid(node, true)) this.SetToGray((bool)newValue);


			});
			return true;
		}

		[HideInInspector]
		public bool isGray = false;
		protected virtual void SetToGray(bool b)
		{
			this.isGray = b;
			var button = this.GetComponent<Button>();
			var sprite = this.GetComponent<Image>();
			if (button != null && sprite != null)
			{
				if (b)
				{
					sprite.color = button.colors.disabledColor;
				}
				else
				{
					sprite.color = new Color32(1, 1, 1, 1);
				}
			}
		}

		[HideInInspector]
		public Button target = null;

		protected Action<Button> clickEventHandler;
		protected readonly List<TButtonBindCallback> clickHandleFuncs = new List<TButtonBindCallback>();

		public bool CheckClick()
		{
			if (this.clickTriggers.Length == 0)
			{
				return false;
			}
			var node = this.transform;
			if (node == null)
			{
				return false;
			}
			this.target = this.GetComponent<Button>();
			if (this.clickEventHandler == null)
			{
				var handler = this.clickEventHandler = (Button target) =>
				{
					this.target = target;
					var funcs = this.clickHandleFuncs.ToArray();
					for (var index = 0; index < funcs.Length; index++)
					{
						var clickHandleFunc = funcs[index];
						if (clickHandleFunc is System.Action<ButtonBindComp, int> uiCaller)
						{
							uiCaller(this, index);
						}
						else if (clickHandleFunc is System.Action call)
						{
							call();
						}
						else if (clickHandleFunc is VM.MemberMethod memberMethod)
						{
							if (memberMethod.GetParametersCount() == 0)
							{
								memberMethod.Invoke(Array.Empty<object>());
							}
							else
							{
								memberMethod.Invoke(new object[]
								{
									this, index,
								});
							}
						}
						else
						{
							Console.Error($"call invalid func type of { this.clickTriggers[index]} -> { clickHandleFunc},", clickHandleFunc);
						}
					}
				};
				this.target.onClick.AddListener(() =>
				{
					handler(this.target);
				});
			}
			this.clickHandleFuncs.Clear();
			for (var i = 0; i < this.clickTriggers.Length; i++)
			{
				var index = i;
				var clickTrigger = this.clickTriggers[index];
				if (string.IsNullOrEmpty(clickTrigger.callExpr))
				{
					continue;
				}

				void OnSetValue(TButtonBindCallback value)
				{
					if (value != null)
					{
						if (Utils.IsValid(this))
						{
							while (this.clickHandleFuncs.Count <= index)
							{
								this.clickHandleFuncs.Add(null);
							}

							this.clickHandleFuncs[index] = value;
						}
					}
					else
					{
						Console.Warn("点击响应需要返回函数类型, 当前返回值: ", value, ", type:", value.GetType());
					}
				}

				this.WatchValueChange<TButtonBindCallback>(clickTrigger.callExpr, (newValue, oldValue) =>
				{
					OnSetValue((TButtonBindCallback)newValue);
				});
			}
			return true;
		}

	}
}
