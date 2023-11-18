
using UnityEngine;
using System;
using System.Collections.Generic;


namespace DataBinding.UIBind
{
	public class CCDataBindHub : CCMyComponent, ICCDataBindHub
	{
		public virtual DataBindHub DataBindHub { get; set; } = new DataBindHub();

		protected override bool NeedAttach()
		{
			return this.isAttachCalled && this.enabled && this.transform.parent.IsActiveInHierarchy();
		}

		public virtual void Integrate()
		{
			DataBindHubHelper.OnAddDataBindHub(this);
		}

		public virtual void Deintegrate()
		{
			this.DataBindHub.Clear();
		}

		/**
		 * 集成
		 * - 遍历所有浅层子hub, 设置父节点为自身
		 */
		public virtual void Relate()
		{
			DataBindHubHelper.OnRelateDataBindHub(this);
		}

		public virtual void Derelate()
		{
			DataBindHubHelper.OnDerelateDataBindHub(this);
		}

		protected override void OnPreload()
		{
			this.Integrate();
		}
		protected override void OnPreDestroy()
		{
			this.Deintegrate();
		}

		protected override void OnAttach()
		{
			this.Relate();
		}

		protected override void OnDeattach()
		{
			this.Derelate();
		}

	}
}
