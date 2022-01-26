using UnityEngine;
using System;
using System.Collections.Generic;

namespace UI.DataBinding
{
	[AddComponentMenu("DataDrive/CCActiveBind")]

	public class CCActiveBind : CCDataBindBase
	{
		[InspectorName("可见性")]
		string visible = "";

		protected override bool needAttach()
		{
			if (!string.IsNullOrEmpty(this.visible))
			{
				return this.isAttachCalled && this.enabled && !!this.transform.parent.IsActiveInHierarchy();
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
