using System;

namespace UI.DataBinding
{
	using number = System.Double;

	public interface ISubDataHub
	{

		number oid { get; set; }

		void setRealDataHub(DataHub realDataHub);
		Object rawObj { get; set; }
		DataHub realDataHub { get; set; }

		void bindDataHost(object data);

	}
}
