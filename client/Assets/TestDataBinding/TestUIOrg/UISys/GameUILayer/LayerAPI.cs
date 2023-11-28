
using gcc.layer;

public class LayerAPI
{
	public static ILayerRoot LayerRoot { get; set; }
	public static LayerMG LayerManager => LayerRoot.LayerManager;

	public static LayerBundle LayerBundle => LayerRoot.LayerBundle;
}
