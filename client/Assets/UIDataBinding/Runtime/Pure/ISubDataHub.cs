using System;

namespace DataBinding.UIBind
{
	using number = System.Double;

	public interface ISubDataHub
	{

		number Oid { get; set; }

		void SetRealDataHub(DataSourceHub realDataHub);
		Object RawObj { get; set; }
		DataSourceHub RealDataHub { get; set; }

		void BindDataHost(object data);
		public void UnsetDataHost();

	}
}
