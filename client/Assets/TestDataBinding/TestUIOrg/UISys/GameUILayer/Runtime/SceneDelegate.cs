using System.Collections.Generic;
using System.Linq;
using gcc.layer;

/**
 * 处理场景附加业务
 */
public class SceneDelegate : ILayerDelegate
{
	public UIScene scene;
	public LayerBundle layerBundle;
	public Dictionary<string, bool> SceneBundleState = new();

	public SceneDelegate init(UIScene scene, LayerBundle layerBundle)
	{
		this.scene = scene;
		this.layerBundle = layerBundle;
		return this;
	}
	public virtual void onOpening()
	{
		var currentScenes = UIScene.currentScenes;
		if (currentScenes.Count > 0)
		{
			var curScene = UIScene.currentScenes[0];
			if (curScene!=null)
			{
				curScene.DoKeepLayers();
			}
		}

		this.layerBundle.SetRecordBundle(this.scene.dynamicBundleName);
		currentScenes.Remove(this.scene);
		currentScenes.Insert(0, this.scene);
	}
	public virtual void onOpened(){
        
	}
	public virtual void onClosing()
	{
	}

	protected Queue<(string, bool)> temp1 = new();
	public virtual void onBeforeClosed()
	{
		// console.warn("pop dialog recordbundle")
		// this.layerBundle.CloseRecordBundle<LayerModel>();
		var recordBundle = this.layerBundle.ExpandBundle(this.layerBundle.RecordBundleName);
		var curBundle=this.layerBundle.GetRecordBundle();
		var currentScenes = UIScene.currentScenes;
		currentScenes.Remove(this.scene);
		UIScene curScene=null;
		if (currentScenes.Count > 0)
		{
			curScene = currentScenes[0];
			if (curScene != null)
			{
				this.layerBundle.SetRecordBundle(curScene.dynamicBundleName);
			}
		}
		else
		{
			this.layerBundle.SetRecordBundle(gcc.layer.LayerBundle.DefaultBundleName);
		}

		if (curScene == null || scene.NeedRecoverLastScene==false)
		{
			curBundle.Clear();
			foreach (var layer in recordBundle)
			{
				this.layerBundle.LayerMG.CloseLayer(layer);
			}
		}
		else
		{
			var recordBundleState = curScene.SceneSideEffect.SceneBundleState;
			var i1 = recordBundleState.Select(state => (state.Key, state.Value));
			var i2 = recordBundle
				.Where(layer => recordBundleState.ContainsKey(layer))
				.Where(layer=>layer!=scene.LayerModel.Uri)
				.Select(layer => (layer, false));
			var recoverList = i2.Concat(i1);
			temp1.Clear();
			foreach (var ele in recoverList)
			{
				temp1.Enqueue(ele);
			}
			curBundle.Clear();
			recordBundleState.Clear();
			foreach (var (layer,isOpen) in temp1)
			{
				if (!isOpen)
				{
					this.layerBundle.LayerMG.CloseLayer(layer);
				}
				else
				{
					if (!this.layerBundle.LayerMG.IsOpen(layer))
					{
						var isRecoverable = true;
						var layerModel = layerBundle.LayerMG.FindLayer(layer);
						if (layerModel != null)
						{
							var comp = layerModel.GetTheComp<UILayer>();
							if (!comp.IsRecoverable)
							{
								isRecoverable = false;
							}
						}

						if (isRecoverable)
						{
							this.layerBundle.LayerMG.ShowLayer(new ShowLayerParam(layer,null,layer,true));
						}
					}
				}
			}
		}
	}

	public virtual void onClosed()
	{
	}
	public virtual void onShow()
	{
	}
	public virtual void onHide()
	{
	}

	public virtual void onExposed()
	{
	}
	public virtual void onShield()
	{
	}

}