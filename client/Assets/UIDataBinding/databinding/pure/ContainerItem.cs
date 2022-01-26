
using System;
using System.Collections.Generic;

namespace UI.DataBinding
{
	using number = System.Double;

	public class ContainerItem : ISubDataHub
	{
		static number oidAcc = 0;
		public number oid { get; set; } = ContainerItem.oidAcc++;
		public number index = 0;

		public DataHub realDataHub { get; set; }
		public Object rawObj { get; set; }

		public void setRealDataHub(DataHub realDataHub)
		{
			this.realDataHub = realDataHub;


			if (this.realDataHub != null)
			{
				this.bindDataHost(this.dataHost);
			}
		}

		public object dataHost;
		public void observeData(object data)
		{
			this.dataHost = data;
			if (this.realDataHub != null)
			{
				this.realDataHub.observeData(data);
			}
		}

		public void unsetDataHost()
		{
			if (this.realDataHub != null)
			{
				this.realDataHub.unsetDataHost();
			}
		}

		public void bindDataHost(object data)
		{
			if (data == null)
			{
				this.unsetDataHost();
			}
			else
			{
				this.observeData(data);
			}
		}

	}
}
