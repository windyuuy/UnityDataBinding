using gcc.layer;
using UISys.Runtime;
using UnityEngine;

namespace UISys.Runtime
{
	[AddComponentMenu("UISys/CurrentLayerBundle")]
	public class CurrentLayerBundleComp: LayerBundleCompBase
	{
		public LayerBundleManager.TLayerBundleRefer LayerBundleRefer;
		public override LayerBundle LayerBundle => LayerBundleRefer.GetLayerBundle();
	}
}