using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Node = UnityEngine.Transform;
using Prefab = UnityEngine.GameObject;
using console = UnityEngine.Debug;
using number = System.Double;
using boolean = System.Boolean;

/**
* 对话框基类
*/
public class UIScene : UILayer
{
    protected override void initDefaultProps()
    {
        this.isCoverDefault = true;
    }

    [SerializeField] protected bool needRecoverLastScene = true;

    /// <summary>
    /// 自动恢复上一场景图层
    /// </summary>
    public virtual bool NeedRecoverLastScene
    {
        get => needRecoverLastScene;
        set => needRecoverLastScene = value;
    }

    [SerializeField] protected bool keepLayers = true;
    
    /// <summary>
    /// 切换场景时保持场景图层打开状态
    /// </summary>
    public virtual bool KeepLayers
    {
        get => keepLayers;
        set => keepLayers = value;
    }
    
    /**
	 * 当前场景
	 */
    public static List<UIScene> currentScenes = new List<UIScene>();
    /**
	 * 获取当前场景
	 */
    public static UIScene GetCurrentScene()
    {
        return UIScene.currentScenes.FirstOrDefault();
    }

    protected SceneDelegate _sideEffect = new SceneDelegate();
    internal virtual SceneDelegate SceneSideEffect => _sideEffect;
    protected override ILayerDelegate SideEffect
    {
        get
        {
            return _sideEffect;
        }
        set
        {
            _sideEffect = value as SceneDelegate;
        }
    }

    internal override void onCreate(object data = null)
    {
        this.LayerModel.IsCover = true;
        this._sideEffect.init(this, this.layerBundle);
        base.onCreate(data);
    }

    public virtual string dynamicBundleName
    {
        get
        {
            var bundleName = this.LayerModel.Uri;
            return $"{bundleName}$DynBundle";
        }
    }

    public virtual void DoKeepLayers()
    {
        if (KeepLayers)
        {
            onKeepLayers();
        }
    }
    public virtual void onKeepLayers()
    {
        var recordBundle = this.layerBundle.ExpandBundle(this.layerBundle.RecordBundleName);
        foreach (var uri in recordBundle)
        {
            var isOpen = this.LayerManager.IsOpen(uri);
            this.SceneSideEffect.SceneBundleState[uri] = isOpen;
        }
    }

}
