using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataBind.UIBind
{
	[AddComponentMenu("DataDrive/ToggleBind")]
	public class ToggleBindComp : DataBindBaseComp
	{
		[Rename("是否选中")] [SerializeField] public string kIsChecked = "";

		[SerializeField] public Toggle target;

		/**
		 * 更新显示状态
		 */
		protected override void OnBindItems()
		{
			this.CheckIsChecked();
		}

		public virtual bool CheckIsChecked()
		{
			if (string.IsNullOrEmpty(this.kIsChecked))
			{
				return false;
			}

			var node = this.transform;
			if (node == null)
			{
				return false;
			}

			this.WatchValueChange<bool>(this.kIsChecked, (newValue, oldValue) =>
			{
				if (Utils.IsValid(node, true)) this.SetIsChecked((bool)newValue);
			});
			return true;
		}

		public virtual void SetIsChecked(bool newValue)
		{
			if (target == null)
			{
				target = this.GetComponent<Toggle>();
			}

			if (target != null)
			{
				target.isOn = newValue;
			}
		}
	}
}