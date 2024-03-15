
using DataBind;
using UnityEngine;

namespace DataBind.UIBind
{
	// [AddComponentMenu("DataDrive/SubDataHub")]
	public abstract class SubDataHubComp : MyComponent, ICCSubDataHub
	{
		protected virtual ISubDataHub SubDataHub { get; set; }
		public virtual string SubKey { get; set; }

		// ccDataHost!: DataHostComp

		public virtual IStdHost DataHost
		{
			get
			{
				// return this.ccDataHost.dataHost
				return this.SubDataHub.RealDataHub?.DataHost;
			}
		}

		public virtual ISubDataHub DataHub
		{
			get
			{
				return this.SubDataHub;
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

		public virtual void BindDataHost(object data, string subKey)
		{
			 this.SubKey = subKey;
			this.SubDataHub.BindDataHost(data);
		}

		public virtual void UnsetDataHost()
		{
			this.SubDataHub.UnsetDataHost();
		}

	}
}
