
using DataBinding;

namespace DataBinding.UIBind
{
	public class CCSubDataHub : CCMyComponent, ICCSubDataHub
	{
		protected virtual ISubDataHub subDataHub { get; set; }

		// ccDataHost!: CCDataHost

		public virtual IStdHost DataHost
		{
			get
			{
				// return this.ccDataHost.dataHost
				return this.subDataHub.RealDataHub?.dataHost;
			}
		}

		public virtual ISubDataHub SubDataHub
		{
			get
			{
				return this.subDataHub;
			}
		}

		// bindDataHost(dataHost: IStdHost) {
		// this.ccDataHost.dataHub.setDataHost(dataHost)
		// }

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

		public virtual void Integrate()
		{
			DataBindHubHelper.OnAddSubDataHub(this);
		}

		public virtual void Deintegrate()
		{
			this.Derelate();
		}

		public virtual void Relate()
		{
		}

		public virtual void Derelate()
		{
		}

		public virtual void BindDataHost(object data)
		{
			this.subDataHub.BindDataHost(data);
		}

	}
}
