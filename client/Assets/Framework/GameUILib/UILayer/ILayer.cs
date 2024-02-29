using System.Threading.Tasks;
using gcc.layer;
using UnityEngine;

namespace gcc.layer
{
	public interface ILayer
	{
		string Uri { get; set; }
		string ResUri { get; set; }
		ILayerLifecycleInnerCall Lifecycle { get; }
		Task OpeningTask { get; set; }

		// ReSharper disable once InconsistentNaming
		GameObject gameObject { get; }

		// ReSharper disable once InconsistentNaming
		Transform transform { get; }
	}
}
