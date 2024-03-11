using System;

namespace DataBinding.UIBind
{
	public class ContainerItem : ISubDataHub
	{
		public static long OidAcc = 0;
		public long Oid { get; set; } = ContainerItem.OidAcc++;
		public int Index = 0;

		public DataSourceHub RealDataHub { get; set; }
		public Object RawObj { get; set; }

		public void SetRealDataHub(DataSourceHub realDataHub)
		{
			this.RealDataHub = realDataHub;


			if (this.RealDataHub != null)
			{
				this.BindDataHost(this.DataHost);
			}
		}

		public object DataHost;

		public void ObserveData(object data)
		{
			this.DataHost = data;
			if (this.RealDataHub != null)
			{
				this.RealDataHub.ObserveData(data);
			}
		}

		public void UnsetDataHost()
		{
			if (this.RealDataHub != null)
			{
				this.RealDataHub.UnsetDataHost();
			}
		}

		public void BindDataHost(object data)
		{
			if (data == null)
			{
				this.UnsetDataHost();
			}
			else
			{
				this.ObserveData(data);
			}
		}
	}
}