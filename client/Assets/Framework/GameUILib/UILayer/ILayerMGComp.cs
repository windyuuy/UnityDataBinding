
namespace gcc.layer
{

	using Node = UnityEngine.Transform;
	using Prefab = UnityEngine.GameObject;

	public interface ILayerMGComp
	{
		Node LayerRoot { get; set; }
		Prefab ModalPrefab { get; set; }
		Prefab ToastPrefab { get; set; }
		IUILoading LoadingView { get; set; }
	}
}
