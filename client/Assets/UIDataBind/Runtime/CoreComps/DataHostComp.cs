using DataBind;
using UnityEngine;

namespace DataBind.UIBind
{
	[AddComponentMenu("DataDrive/DataHost")]
	public class DataHostComp : MyComponent, ICCDataHost
	{

		public virtual IStdHost DataHost
		{
			get
			{
				return this.DataHub.DataHost;
			}
		}

		public virtual DataSourceHub DataHub { get; set; } = new DataSourceHub();

		public override void OnRequireAttach()
		{
			this.OnAttach();
		}
		public override void OnRequireDeattach()
		{
			this.OnDeattach();
		}
		protected override void OnEnable()
		{
			// isAttachCalled 如果为false，则可能因为没有设置父节点
			base.OnEnable();
			this.DataHub.Running = true;
		}
		protected override void OnDisable()
		{
			this.DataHub.Running = false;
			base.OnDisable();
		}

		public virtual void ObserveData(object data)
		{
			this.DataHub.ObserveData(data);
		}

		protected virtual void SetDataHost(IStdHost dataHost)
		{
			this.DataHub.SetDataHost(dataHost);
		}

		protected virtual void UnsetDataHost()
		{
			this.DataHub.UnsetDataHost();
		}

		protected override void OnPreload()
		{
			this.Integrate();
		}
		protected override void OnPreDestroy()
		{
			this.Derelate();
			DataBindHubHelper.OnRemoveDataHub(this);
		}

		protected override void OnAttach()
		{
			this.Relate();
		}

		protected override void OnDeattach()
		{
			this.Derelate();
		}

		/**
		 * 集成
		 * - 遍历所有浅层子hub, 设置父节点为自身
		 */
		public virtual void Integrate()
		{
			this.DataHub.RawObj = this;
			DataBindHubHelper.OnAddDataHub(this);
		}

		public virtual void Relate()
		{
			DataBindHubHelper.OnRelateDataHub(this);
		}

		public virtual void Derelate()
		{
			DataBindHubHelper.OnDerelateDataHub(this);
		}

	}
}
