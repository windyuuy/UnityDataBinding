using gcc.layer;
using RSG;

public static class UILayerMGExt
{
	public static Promise<LayerModel> ReleaseLayer(this LayerMG self, string uri)
	{
#if UNITY_IOS && !UNITY_EDITOR
		const bool destroyOnClose = true;
#else
		const bool destroyOnClose = false;
#endif
		return self.CloseLayer(uri, destroyOnClose);
	}
	public static Promise<LayerModel> ReleaseLayer(this LayerMG self, LayerModel uri)
	{
#if UNITY_IOS && !UNITY_EDITOR
		const bool destroyOnClose = true;
#else
		const bool destroyOnClose = false;
#endif
		return self.CloseLayer(uri, destroyOnClose);
	}
	
	public static Promise<LayerModel> ReleaseSelf(this UILayer self)
	{
		return self.LayerMG.ReleaseLayer(self.LayerModel);
	}
}