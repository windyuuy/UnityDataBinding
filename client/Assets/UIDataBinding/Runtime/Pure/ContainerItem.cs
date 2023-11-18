
using System;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	using number = System.Double;

	public class ContainerItem : ISubDataHub
	{
		static number _oidAcc = 0;
		public number Oid { get; set; } = ContainerItem._oidAcc++;
		public number index = 0;

		public DataSourceHub RealDataHub { get; set; }
		public Object RawObj { get; set; }

		public void SetRealDataHub(DataSourceHub realDataHub)
		{
			this.RealDataHub = realDataHub;


			if (this.RealDataHub != null)
			{
				this.BindDataHost(this.dataHost);
			}
		}

		public object dataHost;
		public void ObserveData(object data)
		{
			this.dataHost = data;
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
