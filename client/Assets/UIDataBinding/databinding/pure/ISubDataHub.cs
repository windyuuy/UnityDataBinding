using System;

namespace DataBinding.UIBind
{
	using number = System.Double;

	public interface ISubDataHub
	{

		number oid { get; set; }

		void setRealDataHub(DataSourceHub realDataHub);
		Object rawObj { get; set; }
		DataSourceHub realDataHub { get; set; }

		void bindDataHost(object data);

	}
}
