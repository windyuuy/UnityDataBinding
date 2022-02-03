using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	using number = System.Double;

	[AddComponentMenu("DataDrive/CCSimpleBind")]
	public class CCSimpleBind : CCDataBindBase
	{
		[Rename("主属性")]
		public string key = "";

		[Rename("忽略undefined值")]
		public bool ignoreUndefinedValue = true;

		[HideInInspector]
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

		/**
		 * 更新显示状态
		 */
		protected override void onBindItems()
		{
			if (!string.IsNullOrEmpty(this.key))
			{
				this.checkLabel();

				// 设立优先级
				var ret = this.checkProgressBar() || this.checkSprite();
			}
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
			this.watchValueChange<number?>(this.key, (newValue, oldValue) =>
			{
				if (progressComponent) progressComponent.value = (float)newValue;
			});
			return true;
		}

		[HideInInspector]
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
				if (sprite != null && this.spriteTextureUrl != newValue)
				{
					this.spriteTextureUrl = newValue;
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
				throw new NotImplementedException();
			}
			else
			{
				Resources.LoadAsync<Sprite>(url).completed += (ret) =>
				{
					var ret2 = ret as ResourceRequest;
					Sprite sprite = ret2.asset as Sprite;

					var image = this.target as Image;
					if (image != null)
					{
						image.sprite = sprite;
					}
				};
			}
		}
	}
}
