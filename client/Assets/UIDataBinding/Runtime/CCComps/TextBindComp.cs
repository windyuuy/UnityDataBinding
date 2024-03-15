using System;
using UnityEngine;
using UnityEngine.UI;

namespace DataBind.UIBind
{
	// TODO: 针对资源的绑定，改为在组件上弱引用资源， 由数据绑定提供索引/键值
	[AddComponentMenu("DataDrive/LabelBind")]
	public class TextBindComp : DataBindBaseComp
	{
		[SerializeField] [Rename("主属性")] protected string key = "";
		[SerializeField] protected Text label;

		[SerializeField] [Rename("忽略undefined值")]
		protected bool ignoreUndefinedValue = true;

		/**
		 * 更新显示状态
		 */
		protected override void OnBindItems()
		{
			if (!string.IsNullOrEmpty(this.key))
			{
				this.CheckLabel();
			}
		}

		// TODO: 完善文本赋值
		protected virtual bool CheckLabel()
		{
			if (label == null)
			{
				label = this.GetComponent<Text>();
			}

			if (label == null)
			{
				return false;
			}

			this.WatchValueChange<string>(this.key, (newValue, oldValue) =>
			{
				if (this.ignoreUndefinedValue)
				{
					if (newValue != null)
					{
						if (label) label.text = $"{newValue}";
					}
				}
				else
				{
					if (label) label.text = $"{newValue}";
				}
			});
			return true;
		}
	}
}