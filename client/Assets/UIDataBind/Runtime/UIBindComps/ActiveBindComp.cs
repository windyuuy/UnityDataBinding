using UnityEngine;
using System;
using System.Collections.Generic;
using DataBind.UIBind.Meta;
using UnityEngine.Serialization;

namespace DataBind.UIBind
{
	[AddComponentMenu("DataDrive/ActiveBind")]
	public class ActiveBindComp : DataBindBaseComp
	{
		[FormerlySerializedAs("visible")] [Rename("可见性")]
		[UIBindKey(typeof(object))]
		public string key = "";

		protected override bool NeedAttach()
		{
			if (!string.IsNullOrEmpty(this.key))
			{
				if(this.IsAttachCalled && this.enabled)
                {
					var p = this.transform.parent;
					return p != null && p.IsActiveInHierarchy();
				}
				return false;
			}
			else
			{
				return this.IsAttachCalled && this.EnabledInHierarchy;
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
			if (string.IsNullOrEmpty(this.key))
			{
				return false;
			}
			var node = this.transform;
			if (node == null)
			{
				return false;
			}
			this.WatchValueChange<bool>(this.key, (object newValue, object oldValue) =>
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
