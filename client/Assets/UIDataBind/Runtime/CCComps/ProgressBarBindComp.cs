using System;
using DataBind.UIBind.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace DataBind.UIBind
{
	[AddComponentMenu("DataDrive/ProgressBarBind")]
	public class ProgressBarBindComp : DataBindBaseComp
	{
		[UIBindKey(typeof(float))]
		[SerializeField] [Rename("主属性")] public string key = "";

		[SerializeField] [Rename("忽略undefined值")]
		public bool ignoreUndefinedValue = true;

		[SerializeField] protected Slider target;

		/**
		 * 更新显示状态
		 */
		protected override void OnBindItems()
		{
			if (!string.IsNullOrEmpty(this.key))
			{
				// 设立优先级
				var ret = this.CheckProgressBar();
			}
		}

		public virtual bool CheckProgressBar()
		{
			if (target == null)
			{
				target = GetComponent<Slider>();
			}

			if (target == null)
			{
				return false;
			}

			this.WatchValueChange<float>(this.key, (newValue, oldValue) =>
			{
				if (newValue is float fValue)
				{
					if (target)
					{
						target.value = fValue;
					}
				}
				else if (!ignoreUndefinedValue)
				{
					target.value = 0;
				}
			});
			return true;
		}
	}
}