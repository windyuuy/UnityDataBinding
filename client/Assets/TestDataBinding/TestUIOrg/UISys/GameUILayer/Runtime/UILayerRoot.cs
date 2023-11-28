
using gcc.layer;
using UnityEngine;

public class UILayerRoot : BLayerRoot
{
	protected LayerMG layerManager;

	protected LayerBundle layerBundle;
	public override LayerMG LayerManager => layerManager;
	public override LayerBundle LayerBundle => layerBundle;

	void Awake()
	{
		this.Init();
	}

	bool inited = false;
	public UILayerRoot Init()
	{
		if (this.inited)
		{
			return this;
		}
		this.inited = true;

		this.layerManager = new LayerMG();
		var config = new LayerMGConfig();
		config.rootComp = this;
		this.layerManager.LoadMGConfig(config);

		this.layerBundle = new LayerBundle().Init(this.layerManager);

		this.LayerManager.RegisterLayerClass("CCLayerComp", typeof(UILayer));

		// 注册自身
		LayerAPI.LayerRoot = this;

		return this;
	}

}
