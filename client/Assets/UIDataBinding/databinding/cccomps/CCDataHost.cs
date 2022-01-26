
using UnityEngine;
using System;
using System.Collections.Generic;

namespace UI.DataBinding
{
	public class CCDataHost : CCMyComponent, ICCDataHost
	{

		public virtual vm.IHost dataHost
		{
			get
			{
				return this.dataHub.dataHost;
			}
		}

		public virtual DataHub dataHub { get; set; } = new DataHub();

		public override void onAfterAttach()
		{
			this.onAttach();
		}
		public override void onAfterDeattach()
		{
			this.onDeattach();
		}
		protected override void onEnable()
		{
			this.dataHub.running = true;
		}
		protected override void onDisable()
		{
			this.dataHub.running = false;
		}

		protected virtual void observeData(object data)
		{
			this.dataHub.observeData(data);
		}

		protected virtual void setDataHost(vm.IHost dataHost)
		{
			this.dataHub.setDataHost(dataHost);
		}

		protected virtual void unsetDataHost()
		{
			this.dataHub.unsetDataHost();
		}

		protected override void __preload()
		{
			this.integrate();
		}
		protected override void onPreDestroy()
		{
			this.derelate();
			DataBindHubHelper.onRemoveDataHub(this);
		}

		protected override void onAttach()
		{
			this.relate();
		}

		protected override void onDeattach()
		{
			this.derelate();
		}

		/**
		 * 集成
		 * - 遍历所有浅层子hub, 设置父节点为自身
		 */
		public virtual void integrate()
		{
			this.dataHub.rawObj = this;
			DataBindHubHelper.onAddDataHub(this);
		}

		public virtual void relate()
		{
			DataBindHubHelper.onRelateDataHub(this);
		}

		public virtual void derelate()
		{
			DataBindHubHelper.onDerelateDataHub(this);
		}

	}
}
