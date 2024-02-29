using UnityEngine;

namespace gcc.layer
{
	public class LayerRootConfig : ILayerRoot
	{
		public TLayerManager LayerManager { get; } = new();
		public LayerBundleManager LayerBundleManager { get; } = new();
		public Transform LayerRoot { get;}

		public LayerRootConfig(Transform layerRoot)
		{
			LayerRoot = layerRoot;
		}
	}
}