using UnityEngine;
using UnityEngine.Serialization;

namespace gcc.layer
{
	public class LayerBundleConfig:MonoBehaviour
	{
		[SerializeField]
		protected LayerBundle layerBundle;

		public LayerBundle LayerBundle => layerBundle;
	}
}
