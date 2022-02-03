using UnityEngine;
using System;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	[AddComponentMenu("DataDrive/CCActiveBind")]

	public class CCActiveBind : CCDataBindBase
	{
		[Rename("可见性")]
		public string visible = "";

		protected override bool needAttach()
		{
			if (!string.IsNullOrEmpty(this.visible))
			{
				if(this.isAttachCalled && this.enabled)
                {
					var p = this.transform.parent;
					return p != null ? p.IsActiveInHierarchy() : false;
				}
				return false;
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
			this.watchValueChange<bool>(this.visible, (object newValue, object oldValue) =>
			{
				if (Utils.isValid(node, true))
				{
					node.gameObject.SetActive(Utils.IsTrue(newValue));
				}
			});
			return true;
		}

	}
}
