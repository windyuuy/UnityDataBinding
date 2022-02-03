
using UnityEngine;
using System;
using System.Collections.Generic;
using DataBinding;

namespace DataBinding.UIBind
{
	public class CCDataHost : CCMyComponent, ICCDataHost
	{

		public virtual IStdHost dataHost
		{
			get
			{
				return this.dataHub.dataHost;
			}
		}

		public virtual DataSourceHub dataHub { get; set; } = new DataSourceHub();

		public override void onRequireAttach()
		{
			this.onAttach();
		}
		public override void onRequireDeattach()
		{
			this.onDeattach();
		}
		protected override void OnEnable()
		{
			this.dataHub.running = true;
		}
		protected override void OnDisable()
		{
			this.dataHub.running = false;
		}

		public virtual void observeData(object data)
		{
			this.dataHub.observeData(data);
		}

		protected virtual void setDataHost(IStdHost dataHost)
		{
			this.dataHub.setDataHost(dataHost);
		}

		protected virtual void unsetDataHost()
		{
			this.dataHub.unsetDataHost();
		}

		protected override void onPreload()
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
