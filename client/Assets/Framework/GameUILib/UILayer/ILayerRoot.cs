

namespace gcc.layer
{
	public interface ILayerRoot
	{
		LayerMG LayerManager { get; }

		//#region layer bundle manager
		LayerBundle LayerBundle { get; }
	}
}
