using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UI.DataBinding
{
	using number = System.Double;

	[AddComponentMenu("DataDrive/CCSimpleBind")]
	public class CCSimpleBind : CCDataBindBase
	{
		[InspectorName("主属性")][Multiline]
		public string key = "";

		[InspectorName("可见性")]
		public string visible = "";

		[InspectorName("点击响应")]
		public CCSimpleBindClickFuncInfo[] clickTriggers = new CCSimpleBindClickFuncInfo[0];

		[InspectorName("忽略undefined值")]
		public bool ignoreUndefinedValue = true;

		public object target;

		public virtual ISEventCleanInfo2<object, object> watchValueChange<T>(string key, Action<T, T> call)
		{
			return base.watchValueChange<T>(key, (value, old) =>
			{
				if (this.ignoreUndefinedValue)
				{
					if (value != null)
					{
						call((T)value, (T)old);
					}
				}
				else
				{
					call((T)value, (T)old);
				}
			});
		}

		protected override bool needAttach()
		{
			if (!string.IsNullOrEmpty(this.visible))
			{
				// 支持visible设置
				return this.isAttachCalled && this.enabled && !!this.IsActiveInHierarchy();
			}
			else
			{
				return this.isAttachCalled && this.enabledInHierarchy;
			}
		}

		/**
		 * 更新显示状态
		 */
		protected override void onBindItems()
		{
			this.checkVisible();

			if (!string.IsNullOrEmpty(this.key))
			{
				this.checkLabel();

				// 设立优先级
				var ret = this.checkProgressBar() || this.checkSprite();
			}
		}

		public virtual bool checkVisible()
		{
			if (string.IsNullOrEmpty(this.visible))
			{
				return false;
			}
			var node = this.transform;
			if (node == null)
			{
				return false;
			}
			this.watchValueChange<bool>(this.visible, (newValue, oldValue) =>
			{
				if (Utils.isValid(node, true)) node.gameObject.SetActive(newValue);
			});
			return true;
		}

		// TODO: 完善文本赋值
		public virtual bool checkLabel()
		{
			var label = this.GetComponent<Text>();
			if (label == null)
			{
				return false;
			}
			this.target = label;
			this.watchValueChange<string>(this.key, (newValue, oldValue) =>
			{
				if (label) label.text = $"{ newValue}";
			});
			return true;
		}

		public virtual bool checkProgressBar()
		{

			var progressComponent = this.GetComponent<Slider>();
			if (progressComponent == null)
			{
				return false;
			}
			this.target = progressComponent;
			this.watchValueChange<number>(this.key, (newValue, oldValue) =>
			{
				if (progressComponent) progressComponent.value = (float)newValue;
			});
			return true;
		}

		public string spriteTextureUrl;
		public virtual bool checkSprite()
		{
			var sprite = this.GetComponent<Image>();
			if (sprite == null)
			{
				return false;
			}
			this.target = sprite;
			this.watchValueChange<string>(this.key, (newValue, oldValue) =>
			{
				this.spriteTextureUrl = newValue;
				if (sprite != null)
				{
					this.loadImage(newValue, sprite);
				}
			});
			return true;
		}

		protected virtual void loadImage(string url, Image sprite)
		{
			if (url == "")
			{
				sprite.sprite = null;
				return;
			}

			if (!(url is string))
			{
				return;
			}

			if (url.StartsWith("http"))
			{
			}
			else
			{

			}
		}
	}
}
