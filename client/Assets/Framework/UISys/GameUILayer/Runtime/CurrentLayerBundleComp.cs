using System.Threading.Tasks;
using gcc.layer;
using UnityEngine;

namespace UISys.Runtime
{
	[AddComponentMenu("UISys/CurrentLayerBundle")]
	public class CurrentLayerBundleComp : LayerBundleCompBase
	{
		public LayerBundleManager.TLayerBundleRefer LayerBundleRefer;

		public override LayerBundle LoadLayerBundle()
		{
			return LayerBundleRefer.GetLayerBundle();
		}
	}
}