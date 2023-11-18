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

		protected override bool NeedAttach()
		{
			if (!string.IsNullOrEmpty(this.visible))
			{
				if(this.isAttachCalled && this.enabled)
                {
					var p = this.transform.parent;
					return p != null && p.IsActiveInHierarchy();
				}
				return false;
			}
			else
			{
				return this.isAttachCalled && this.EnabledInHierarchy;
			}
		}

		/**
		 * 更新显示状态
		 */
		protected override void OnBindItems()
		{
			this.CheckVisible();
		}

		public virtual bool CheckVisible()
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
			this.WatchValueChange<bool>(this.visible, (object newValue, object oldValue) =>
			{
				if (Utils.IsValid(node, true))
				{
					node.gameObject.SetActive(Utils.IsTrue(newValue));
				}
			});
			return true;
		}

	}
}
