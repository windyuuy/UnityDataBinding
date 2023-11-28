
using UnityEngine;

namespace gcc.layer
{
	public abstract class BLayerRoot : MonoBehaviour, ILayerRoot
	{
		public abstract LayerMG LayerManager { get; }
		public abstract LayerBundle LayerBundle { get; }
	}
}
