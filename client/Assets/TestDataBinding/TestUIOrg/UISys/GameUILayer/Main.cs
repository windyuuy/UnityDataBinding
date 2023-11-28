
using gcc.layer;
using UI.UISys.GameUILayer.Runtime;

public class Main
{
	public static UILayerRoot UILayerRoot;

	public static LayerMG LayerManager
	{
		get
		{
			return UILayerRoot.LayerManager;
		}
	}
	public static LayerBundle LayerBundle => UILayerRoot.LayerBundle;

}
