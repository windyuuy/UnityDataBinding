using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace UISys.Runtime
{
	[CreateAssetMenu(fileName = "TagOrdersConfig.asset", menuName = "UISys/TagOrdersConfig")]
	public class TagOrdersConfig : ScriptableObject
	{
		public PresetTagsConfig presetTagsConfig;
		
		public List<string> mainOrders;
		public List<string> dependOrders;
		public TagOrdersConfig[] extendConfigs;

		// protected string[] MainOrders0;
		// protected string[] DependOrders0;
		//
		//
		// protected bool IsLoaded = false;

		// public void Load()
		// {
		// 	if (!IsLoaded)
		// 	{
		// 		IsLoaded = true;
		//
		// 		if (MainOrders0 == null)
		// 		{
		// 			MainOrders0 = mainOrders.ToArray();
		// 		}
		//
		// 		if (DependOrders0 == null)
		// 		{
		// 			DependOrders0 = dependOrders.ToArray();
		// 		}
		//
		// 		var mainOrdersE = Enumerable.Empty<string>();
		// 		mainOrdersE = mainOrdersE.Concat(MainOrders0);
		//
		// 		var dependOrdersE = Enumerable.Empty<string>();
		// 		dependOrdersE = dependOrdersE.Concat(DependOrders0);
		//
		// 		for (var i = 0; i < extendConfigs.Length; i++)
		// 		{
		// 			var config = extendConfigs[i];
		//
		// 			mainOrdersE = mainOrdersE.Concat(config.mainOrders);
		// 			dependOrdersE = dependOrdersE.Concat(config.dependOrders);
		// 		}
		//
		// 		mainOrders = mainOrdersE.ToList();
		// 		dependOrders = dependOrdersE.ToList();
		// 	}
		// }


		public (string[] mainOrders0, string[] dependOrders0) LoadAll()
		{
			var mainOrders0 = mainOrders.ToArray();
			var dependOrders0 = dependOrders.ToArray();

			var mainOrdersE = Enumerable.Empty<string>();
			mainOrdersE = mainOrdersE.Concat(mainOrders0);

			var dependOrdersE = Enumerable.Empty<string>();
			dependOrdersE = dependOrdersE.Concat(dependOrders0);

			for (var i = 0; i < extendConfigs.Length; i++)
			{
				var config = extendConfigs[i];

				if (config != null)
				{
					mainOrdersE = mainOrdersE.Concat(config.mainOrders);
					dependOrdersE = dependOrdersE.Concat(config.dependOrders);
				}
			}

			mainOrders0 = mainOrdersE.ToArray();
			dependOrders0 = dependOrdersE.ToArray();

			return (mainOrders0, dependOrders0);
		}
	}
}