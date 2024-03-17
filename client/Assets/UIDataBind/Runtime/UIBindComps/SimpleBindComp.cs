using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using DataBind.UIBind.Meta;

namespace DataBind.UIBind
{
	// TODO: 针对资源的绑定，改为在组件上弱引用资源， 由数据绑定提供索引/键值
	[AddComponentMenu("DataDrive/SimpleBind")]
	public sealed class SimpleBindComp : DataBindBaseComp
	{
		[UIBindKey(typeof(object))]
		[Rename("主属性")] public string key = "";

		[Rename("忽略undefined值")] public bool ignoreUndefinedValue = true;

		[HideInInspector] public object target;
		//
		// public ISEventCleanInfo2<object, object> WatchValueChange2<T>(string key0, Action<T, T> call)
		// {
		// 	return base.WatchValueChange<T>(key0, OnValueChanged);
		// }
		//
		// protected void OnValueChanged(object value, object old)
		// {
		// 	if (this.ignoreUndefinedValue)
		// 	{
		// 		if (value != null)
		// 		{
		// 			call((T)value, (T)(old ?? default(T)));
		// 		}
		// 	}
		// 	else
		// 	{
		// 		call((T)(value ?? default(T)), (T)(old ?? default(T)));
		// 	}
		// }

		/**
		 * 更新显示状态
		 */
		protected override void OnBindItems()
		{
			if (!string.IsNullOrEmpty(this.key))
			{
				this.CheckLabel();

				// 设立优先级
				var ret = this.CheckProgressBar() || this.CheckSprite();
			}
		}

		protected Text Label;
		// TODO: 完善文本赋值
		public bool CheckLabel()
		{
			if (Label == null)
			{
				Label = this.GetComponent<Text>();
			}
			if (Label == null)
			{
				return false;
			}

			this.target = Label;
			this.WatchValueChange<string>(this.key, OnTextChanged);
			return true;
		}

		protected void OnTextChanged(object newValue,object oldValue)
		{
			var newValue2 = ConvValueType<string>(newValue);
			if (Label) Label.text = $"{newValue2}";
		}

		protected Slider ProgressComponent;
		public bool CheckProgressBar()
		{
			if (ProgressComponent == null)
			{
				ProgressComponent = this.GetComponent<Slider>();
			}
			if (ProgressComponent == null)
			{
				return false;
			}

			this.target = ProgressComponent;

			this.WatchValueChange<float>(this.key, OnFloatChanged);
			return true;
		}

		protected void OnFloatChanged(object newValue, object oldValue)
		{
			var newValue2 = ConvValueType<float>(newValue);
			if (ProgressComponent != null)
			{
				ProgressComponent.value = newValue2;
			}
		}

		[HideInInspector] public string spriteTextureUrl;

		protected Image Sprite;
		public bool CheckSprite()
		{
			if (Sprite == null)
			{
				Sprite = this.GetComponent<Image>();
			}
			if (Sprite == null)
			{
				return false;
			}

			this.target = Sprite;

			this.WatchValueChange<string>(this.key, OnUrlChanged);
			return true;
		}

		void OnUrlChanged(object newValue, object oldValue)
		{
			var newValue2 = ConvValueType<string>(newValue)??"";
			if (Sprite != null && this.spriteTextureUrl != newValue2)
			{
				this.spriteTextureUrl = newValue2;
				this.LoadImage(newValue2, Sprite);
			}
		}

		private T ConvValueType<T>(object newValue)
		{
			return ignoreUndefinedValue ? (newValue==null ? default(T):(T)newValue) : (T)newValue;
		}

		protected void LoadImage(string url, Image sprite)
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