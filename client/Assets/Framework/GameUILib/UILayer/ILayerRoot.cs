using UnityEngine;

namespace gcc.layer
{
	public interface ILayerRoot
	{
		TLayerManager LayerManager { get; }

		//#region layer bundle manager
		LayerBundleManager LayerBundleManager { get; }

		Transform LayerRoot { get; }
	}
}