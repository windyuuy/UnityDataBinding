using System;

namespace DataBind.UIBind
{
	public interface ISubDataHub
	{

		long Oid { get; set; }

		void SetRealDataHub(DataSourceHub realDataHub);
		Object RawObj { get; set; }
		DataSourceHub RealDataHub { get; set; }

		void BindDataHost(object data);
		public void UnsetDataHost();

	}
}
