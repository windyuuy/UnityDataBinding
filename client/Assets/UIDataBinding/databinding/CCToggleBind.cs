
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.DataBinding
{
	[AddComponentMenu("DataDrive/CCToggleBind")]
	public class CCToggleBind : CCDataBindBase
	{
		[InspectorName("是否选中")]
		public string kIsChecked = "";

		/**
		 * 更新显示状态
		 */
		protected override void onBindItems()
		{
			this.checkIsChecked();
		}

		public virtual bool checkIsChecked()
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
			this.watchValueChange<bool>(this.kIsChecked, (newValue, oldValue) =>
			{
				if (Utils.isValid(node, true)) this.setIsChecked((bool)newValue);
			});
			return true;
		}

		public virtual void setIsChecked(bool newValue)
		{
			var toggle = this.GetComponent<Toggle>();
			if (toggle)
			{
				toggle.isOn = newValue;
			}
		}

	}
}
