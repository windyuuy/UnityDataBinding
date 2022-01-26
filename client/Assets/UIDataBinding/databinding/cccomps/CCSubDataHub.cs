

namespace UI.DataBinding
{
	public class CCSubDataHub : CCMyComponent, ICCSubDataHub
	{
		protected virtual ISubDataHub _subDataHub { get; set; }

		// ccDataHost!: CCDataHost

		public virtual vm.IHost dataHost
		{
			get
			{
				// return this.ccDataHost.dataHost
				return this._subDataHub.realDataHub?.dataHost;
			}
		}

		public virtual ISubDataHub subDataHub
		{
			get
			{
				return this._subDataHub;
			}
		}

		// bindDataHost(dataHost: vm.IHost) {
		// this.ccDataHost.dataHub.setDataHost(dataHost)
		// }

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

		public virtual void integrate()
		{
			DataBindHubHelper.onAddSubDataHub(this);
		}

		public virtual void deintegrate()
		{
			this.derelate();
		}

		public virtual void relate()
		{
		}

		public virtual void derelate()
		{
		}

		public virtual void bindDataHost(object data)
		{
			this._subDataHub.bindDataHost(data);
		}

	}
}
