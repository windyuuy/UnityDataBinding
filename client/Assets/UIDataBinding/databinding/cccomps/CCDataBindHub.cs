
using UnityEngine;
using System;
using System.Collections.Generic;


namespace UI.DataBinding
{
	public class CCDataBindHub : CCMyComponent, ICCDataBindHub
	{
		public virtual DataBindHub dataBindHub { get; set; } = new DataBindHub();

		protected override bool needAttach()
		{
			return this.isAttachCalled && this.enabled && !!this.transform.parent.IsActiveInHierarchy();
		}

		public virtual void integrate()
		{
			DataBindHubHelper.onAddDataBindHub(this);
		}

		public virtual void deintegrate()
		{
			this.dataBindHub.clear();
		}

		/**
		 * 集成
		 * - 遍历所有浅层子hub, 设置父节点为自身
		 */
		public virtual void relate()
		{
			DataBindHubHelper.onRelateDataBindHub(this);
		}

		public virtual void derelate()
		{
			DataBindHubHelper.onDerelateDataBindHub(this);
		}

		protected override void onPreload()
		{
			this.integrate();
		}
		protected override void onPreDestroy()
		{
			this.deintegrate();
		}

		protected override void onAttach()
		{
			this.relate();
		}

		protected override void onDeattach()
		{
			this.derelate();
		}

	}
}
