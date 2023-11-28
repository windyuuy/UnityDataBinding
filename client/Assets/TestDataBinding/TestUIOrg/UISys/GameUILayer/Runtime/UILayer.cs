using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gcc.layer;
using RSG;
using UI.UISys.GameUILayer.Runtime.Input;
using UnityEngine;
using UnityEngine.Serialization;
using Node = UnityEngine.Transform;
using Prefab = UnityEngine.GameObject;
using console = UnityEngine.Debug;
using number = System.Double;
using boolean = System.Boolean;
using Object = UnityEngine.Object;

//public enum LayerTagEnum
//{
//    Back,
//    Normal,
//    Barrier,
//    Pop,
//    Transfer,
//    DSureDialog,

//    /// <summary>
//    /// 信息通知界面
//    /// </summary>
//    Info
//}

[DisallowMultipleComponent]
public class UILayer : MonoBehaviour, ILayerInnerCall, ILayerRoot
{
    [SerializeField][HideInInspector] protected string[] layerTags = new string[0];
    
    /// <summary>
    /// 是否全屏遮盖下方界面
    /// </summary>
    [SerializeField][HideInInspector] protected bool isCoverDefault;

    /// <summary>
    /// 是否允许被cover遮盖
    /// </summary>
    [SerializeField][HideInInspector] protected bool allowCoverred = true;

    protected ILayerRoot layerRoot;
    private LayerMG layerMG => layerRoot.LayerManager;
    protected virtual LayerBundle layerBundle => layerRoot.LayerBundle;

    public virtual LayerMG LayerMG => layerMG;

    public virtual bool IsRecoverable => true;
    
    /// <summary>
    /// 是否全屏界面
    /// </summary>
    public virtual bool IsUICover=>this.layerTags.Contains("UICover");

    /**
	 * 组合一些附加的业务
	 */
    protected virtual ILayerDelegate SideEffect { get; set; }

    public virtual ILayerRoot LayerRootComp
    {
        get => layerRoot;
        set => layerRoot = value;
    }

    public virtual Node Node
    {
        get { return this.transform; }
    }

    public virtual LayerMG LayerManager
    {
        get => LayerMG;
    }

    //#region layer bundle manager
    public virtual LayerBundle LayerBundle
    {
        get => layerBundle;
    }

    private UILayerLifecycleDelegate lifecycleDelegate;
    public ILayerLifecycleInnerCall Lifecycle => lifecycleDelegate;

    public UILayer() : base()
    {
        this.__initConstructor();
    }

    private void __initConstructor()
    {
        this.initLifecycle();
#if UNITY_EDITOR
	    this.initDefaultProps();
#endif
        this.doCreateLayerModel();
    }

    protected virtual void doCreateLayerModel()
    {
        this.LayerModel = new LayerModel();
    }

    protected virtual void initLifecycle()
    {
	    this.lifecycleDelegate = new UILayerLifecycleDelegate
	    {
		    Layer = this,
	    };
    }
    protected virtual void initDefaultProps()
    {
	    this.isCoverDefault = false;
    }

    /**
	 * 构建图层束
	 */
    public virtual void SetupOneBundle(string name, LayerBundleInputItem[] items)
    {
        this.layerBundle.SetupOneBundle(name, items);
    }

    /**
	 * 批量构建图层束
	 */
    public virtual void SetupBundles(Dictionary<string, LayerBundleInputItem[]> map)
    {
        this.layerBundle.SetupBundles(map);
    }

    /**
	 * 展示图层束
	 */
    public virtual IPromise<IEnumerable<T>> ShowBundle<T>(string name) where T : LayerModel
    {
        return this.layerBundle.ShowBundle<T>(name);
    }

    /**
	 * 关闭图层束
	 */
    public virtual IPromise<IEnumerable<T>> CloseBundle<T>(string name) where T : LayerModel
    {
        return this.layerBundle.CloseBundle<T>(name);
    }

    /**
	 * 隐藏图层束
	 */
    // public virtual IPromise<IEnumerable<T>> HideBundle<T>(string name) where T : LayerModel
    // {
    //     return this.layerBundle.HideBundle<T>(name);
    // }

    /**
	 * 预加载图层束
	 */
    public virtual IPromise<IEnumerable<T>> PreloadBundle<T>(string name) where T : LayerModel
    {
        return this.layerBundle.PreloadBundle<T>(name);
    }
    //#endregion

    //#region lifecycle
    /**
	 * 显示对话框
	 * @param uri
	 * @returns
	 */
    public virtual Promise<LayerModel> ShowLayer(string uri)
    {
        return LayerMG.ShowLayer(uri);
    }

    public virtual Promise<LayerModel> ShowLayer(ShowLayerParam uri)
    {
        return LayerMG.ShowLayer(uri);
    }
    public virtual Promise<LayerModel> ShowLayer(string uri, object data)
    {
        return LayerMG.ShowLayer(uri,data);
    }

    /**
	 * 寻找对应资源ID的对话框
	 * @param resUri
	 * @returns
	 */
    public virtual LayerModel[] FindLayersByResUri(string resUri)
    {
        return LayerMG.FindLayersByResUri(resUri);
    }

    /**
	 * 寻找所有同类对话框
	 * @param resUri
	 * @returns
	 */
    public virtual LayerModel[] FindSameKindLayers()
    {
        return this.FindLayersByResUri(this.LayerModel.ResUri);
    }

    /**
	 * 创建对话框
	 * @param uri
	 * @returns
	 */
    public virtual IPromise<LayerModel> CreateLayer(string uri)
    {
        return LayerMG.CreateLayer(uri);
    }

    public virtual IPromise<LayerModel> CreateLayer(ShowLayerParam uri)
    {
        return LayerMG.CreateLayer(uri);
    }

    /**
	 * 预加载对话框
	 * @param uri
	 * @returns
	 */
    public virtual Promise<LayerModel> PreloadLayer(string uri)
    {
        return LayerMG.PreloadLayer(uri);
    }

    public virtual Promise<LayerModel> PreloadLayer(ShowLayerParam uri)
    {
        return LayerMG.PreloadLayer(uri);
    }

    /**
	 * 关闭对话框
	 * @param uri
	 * @returns
	 */
    public virtual Promise<LayerModel> CloseLayer(string uri)
    {
        return LayerMG.CloseLayer(uri);
    }

    /**
	 * 关闭所有对话框
	 * @param uri
	 * @returns
	 */
    public virtual IPromise<IEnumerable<LayerModel>> CloseAllLayers()
    {
        return LayerMG.CloseAllLayers();
    }

    /**
	 * 强制关闭并销毁对话框
	 * @returns
	 */
    public virtual IPromise<LayerModel> DestroyLayer(string uri)
    {
        return LayerMG.DestroyLayer(uri);
    }

    /**
	 * 对话框数据模型
	 */
    protected LayerModel _layerModel;

    public virtual LayerModel LayerModel
    {
        get => _layerModel;
        set => _layerModel = value;
    }

    /// <summary>
    /// 是否遮挡封面
    /// </summary>
    /// <value></value>
    public virtual bool IsCover
    {
        get => LayerModel.IsCover;
        set => LayerModel.IsCover = value;
    }

    public virtual bool IsOpen
    {
	    get => LayerModel.IsOpen; 
    }

    /**
	 * 对话框模型中的自定义数据
	 */
    public virtual object RawData
    {
        get { return this.LayerModel.Data; }
    }

    public virtual T GetRawData<T>() where T : class
    {
        return RawData as T;
    }

    /**
	 * 设置对话框tag
	 * @param tags 
	 */
    public virtual void SetTags(IEnumerable<string> tags)
    {
        this.LayerModel.Tags = new List<string>(tags);
    }

    /**
	 * 设置对话框tag
	 * @param tags
	 */
    public virtual void SetTag(string tag)
    {
        this.SetTags(new string[] { tag });
    }

    /**
	 * 初次创建调用
	 */
    internal virtual void onCreate(object data = null)
    {
        if (this.layerTags.Length > 0)
        {
            SetTags(this.layerTags);
        }
        this.IsCover = isCoverDefault;

        UIInputMG.Shared.TryRegister(this);
        this.integrateDataBind();
        this.onInit(data);
    }

    /**
	 * 创建对话框完成时调用
	 * @param data
	 */
    protected virtual void onInit(object data = null)
    {
    }

    internal virtual void __callOnEnter()
    {
        this.onEnter();
    }

    /**
	 * 进入初次显示状态
	 */
    protected virtual void onEnter()
    {
    }

    /**
	 * 异步关闭
	 */
    internal virtual void __callDoClose(System.Action resolve, System.Action<Exception> reject)
    {
        this.doCloseAsync().Then(resolve, (reason) => reject(reason));
    }

    protected virtual Promise doCloseAsync()
    {
        return new Promise((resolve, reject) => { this.PlayCloseAnimation(() => { resolve(); }); });
    }


    /**
	 * 播放关闭动画
	 */
    internal virtual void __callDoOpen(System.Action resolve, System.Action<Exception> reject)
    {
        this.doOpenAsync().Then(resolve, (reason) => reject(reason));
    }

    protected virtual IPromise doOpenAsync()
    {
        var task1 = new Promise((resolve, reject) => { this.PlayOpenAnimation(() => { resolve(); }); });
        // var task2 = new Promise((resolve, reject) =>
        // {
        // 	if (this.audioConfigName)
        // 	{
        // 		this.loadAudioConfig(this.audioConfigName, (config) =>
        // 		{
        // 			resolve();
        // 		});
        // 	}
        // 	else
        // 	{
        // 		resolve();
        // 	}
        // });
        return Promise.All(new Promise[]
        {
            task1,
            // task2,
        });
    }

    /**
	 * 播放关闭动画
	 */
    public virtual void PlayCloseAnimation(System.Action finished)
    {
        finished();
    }

    /**
	 * 播放关闭动画
	 */
    public virtual void PlayOpenAnimation(System.Action finished)
    {
        finished();
    }

    private void _pauseLayer()
    {
        console.LogWarning("[ui] dialog pauseLayer:" + this.name);

        // 停止渲染被遮蔽的对话框
        // {
        // 	this._lastOpacity = this.node._uiProps.localOpacity

        // 	var renders = this.node.getComponentsInChildren(cc.Renderable2D)
        // 	renders.forEach(sk => {
        // 		(sk as any)["_enabledLast"] = sk.enabled
        // 		sk.enabled = false
        // 	})

        // 	this.node._uiProps.localOpacity = 0
        // }

        if (allowCoverred)
        {
			this.gameObject.SetActive(false);
        }
    }

    private bool _isResumed()
    {
        if (this.LayerModel.BeCovered)
        {
            return false;
        }

        return this.gameObject.activeSelf;
    }

    private void _resumeLayer()
    {
        // 重新启用渲染被显示的对话框
        // if (this._lastOpacity !== undefined) {
        console.LogWarning("[ui] dialog resumeLayer:" + this.name);

        // 	this.node._uiProps.localOpacity = this._lastOpacity
        // 	this._lastOpacity = undefined

        // 	syncOpacity(this.node)

        // 	var renders = this.node.getComponentsInChildren(cc.Renderable2D)
        // 	renders.forEach(sk => {
        // 		if ((sk as any)["_enabledLast"] !== undefined) {
        // 			if (sk.enabled) {
        // 				console.warn("意外的enabled:", sk)
        // 				sk.onRestore()
        // 			} else {
        // 				sk.enabled = (sk as any)["_enabledLast"]
        // 			}
        // 		}
        // 	})

        // }

        this.gameObject.SetActive(true);
    }

    internal virtual void __callOnExposed()
    {
        this._resumeLayer();

        this.SideEffect?.onExposed();
        this.onExposed();
    }

    /**
	 * 图层暴露
	 */
    // protected onExposed?(): void
    internal virtual void __callOnShield()
    {
        this._pauseLayer();

        this.SideEffect?.onShield();
        this.onShield();
    }

    /**
	 * 图层被遮挡屏蔽
	 */
    protected virtual void onShield()
    {
        console.LogWarning("[ui] dialog onShield:" + this.name);
    }

    /**
	 * 显示对话框
	 */
    public virtual Promise<LayerModel> Show()
    {
        return this.layerMG.ShowLayer(new ShowLayerParam(this.LayerModel.Uri));
    }

    internal virtual async Task __callOnShow()
    {
        if (!this.LayerModel.BeCovered && _isResumed()==false)
        {
            this._resumeLayer();
        }

        this.gameObject.SendMessage(ICCReuse.Reuse, SendMessageOptions.DontRequireReceiver);
        this.SideEffect?.onShow();

        console.LogWarning("[ui] dialog onShow:" + this.name);

        this.onShow();
        await this.onShowAsync();
    }

    /**
	 * 每次由隐藏变为显示调用
	 */
    protected virtual void onShow()
    {
    }

    /**
	 * 每次由隐藏变为显示调用
	 */
    protected virtual Task onShowAsync()
    {
	    return Task.CompletedTask;
    }

    internal virtual async Task __callOnOverlapShow(ShowLayerParam paras)
    {
	    this.onOverlapShow(paras);
	    await this.onOverlapShowAsync(paras);
    }

    /// <summary>
    /// 每次重复调用显示时, 调用
    /// </summary>
    protected virtual void onOverlapShow(ShowLayerParam paras)
    {

    }

    /// <summary>
    /// 每次重复调用显示时, 调用
    /// </summary>
    protected virtual Task onOverlapShowAsync(ShowLayerParam paras)
    {
	    return Task.CompletedTask;
    }

    /**
	 * 强制隐藏而不关闭对话框
	 */
    // public virtual Promise<LayerModel> Hide()
    // {
    //     return this.layerMG.HideLayer(this.LayerModel.Uri);
    // }

    internal virtual async Task __callOnHide()
    {
        this.SideEffect?.onHide();
        this.onHide();
        await this.onHideAsync();
    }

    /**
	 * 每次由显示变为隐藏调用
	 */
    protected virtual void onHide()
    {
    }

    /**
	 * 每次由显示变为隐藏调用
	 */
    protected virtual Task onHideAsync()
    {
	    return Task.CompletedTask;
    }

    /**
	 * 关闭对话框
	 */
    public virtual Promise<LayerModel> Close(boolean destroyOnClose = false)
    {
        return this.layerMG.CloseLayer(this.LayerModel, destroyOnClose);
    }

    internal virtual async Task __callOnClosing()
    {
        this.SideEffect?.onClosing();
        await this.onClosingAsync();
    }

    protected virtual Task onClosingAsync()
    {
	    return Task.CompletedTask;
    }

    internal virtual void __callOnClosed()
    {
        // console.warn("closeLayer:", this.dialogModel.uri)
        this.SideEffect?.onClosed();
        this.stopAllMusics();
        this.onClosed();
    }

    internal virtual void __callOnBeforeClosed()
    {
        this.SideEffect?.onBeforeClosed();
        this.onBeforeClosed();
    }

    protected virtual void onBeforeClosed()
    {
    }

    /**
	 * 关闭调用
	 */
    protected virtual void onClosed()
    {
    }

    internal virtual async Task __callOnOpening()
    {
        // console.warn("recorddialog", this.layerBundle.recordBundleName, this.dialogModel.uri)
        // 记录打开的对话框
        if (this is not UIScene)
        {
	        this.layerBundle.AddRecordItem(this.LayerModel.Uri);
        }
        this.SideEffect?.onOpening();
        this.onOpening();
        await this.onOpeningAsync();
    }

    protected virtual void onOpening()
    {
    }

    protected virtual Task onOpeningAsync()
    {
	    return Task.CompletedTask;
    }

    internal virtual void __callOnOpened()
    {
        this.SideEffect?.onOpened();
        this.onOpened();
    }

    // protected virtual onOpened?(): void

    internal void __callOnCoverChanged()
    {
        this.onCoverChanged();
    }

    /**
	 * 顶级图层改变时调用
	 */
    protected virtual void onCoverChanged()
    {
    }

    internal virtual void __callOnAnyFocusChanged(bool focus)
    {
        this.onAnyFocusChanged(focus);
    }

    /**
	 * 焦点改变时调用
	 */
    protected virtual void onAnyFocusChanged(bool focus)
    {
    }

    internal virtual void __callOnBeforeDestroy()
    {
        this.onBeforeDestroy();
        this.clearCoroutines();
        this._clearNodePool();
    }

    /**
	 * 调用对话框destroy之前调用
	 */
    protected virtual void onBeforeDestroy()
    {
    }

	/**
	 * 强制关闭并销毁对话框自身
	 * @returns
	 */
	public virtual IPromise<LayerModel> DestroySelf()
    {
        return LayerMG.DestroyLayer(this.LayerModel);
    }

    //#region

    //#region node pool
    /**
	 * 清理使用的节点
	 */
    private void _clearNodePool()
    {
    }

    /**
	 * 强制销毁静态节点池
	 */
    protected virtual void DestroyPoolMap()
    {
    }
    //#endregion

    //#region data host
    protected virtual void integrateDataBind()
    {
    }

    /**
	 * 观测数据
	 * @param data
	 */
    public virtual void observeData(Object data, bool updateChildren = true)
    {
    }
    //#endregion

    // #region event binding
    public virtual void DoClose()
    {
        this.Close();
    }
    // #endregion

    //#region audio
    protected string audioConfigName = "";

    protected virtual void onOpened()
    {
    }

    protected virtual void onExposed()
    {
		console.LogWarning("[ui] dialog onExposed:" + this.name);
    }

    //#endregion


    //#region UILog
    protected lang.libs.Log _logger;

    // 使用get避免没人用浪费内存
    /**
	 * 对话框日志管理
	 */
    public virtual lang.libs.Log Logger
    {
        get
        {
            return this._logger ?? (
                this._logger = Logs.UILog
                    .clone()
                    .appendTags(new string[]
                    {
                        "dialog",
                        this.name
                    })
            );
        }
    }

    /**
	 * 打印普通日志
	 * @param args 
	 */
    public virtual void Log(params object[] args)
    {
        this.Logger.log(args);
    }

    // debug(...args: any[]) {
    // 	this.logger.debug(...args)
    // }
    // info(...args: any[]) {
    // 	this.logger.info(...args)
    // }
    /**
	 * 打印警告日志
	 * @param args 
	 */
    public virtual void Warn(params object[] args)
    {
        this.Logger.warn(args);
    }

    /**
	 * 打印错误日志
	 * @param args 
	 */
    public virtual void Error(params object[] args)
    {
        this.Logger.error(args);
    }

    /**
     * 获取图层层级
     */
    public virtual int GetLayerOrder()
    {
        return this.Node.GetSiblingIndex();
    }

    /// <summary>
    /// 是否最上层图层
    /// </summary>
    /// <returns></returns>
    public virtual bool IsTopLayer()
    {
        var r=this.layerMG.GetOpenLayerOrderRange();
        var order = this.GetLayerOrder();
        return r.End.Value == order;
    }

    /**
	 * 播放游戏结算界面音效
	 * @param key
	 */
    // public virtual void PlayGameResultEffect(string key)
    // {
    //     // AudioManager.playCommonEffect(key, this.node)
    // }

    //#endregion

    #region Coroutine

    protected virtual void clearCoroutines()
    {
    }

    #endregion

    #region Audios

    protected virtual void stopAllMusics()
    {
    }

    internal virtual void __callOnBeforeRelease()
    {
    }

    #endregion

    #region HandleInput

    public virtual bool EnableOnKeyback
    {
	    get
	    {
		    if (this is IWithKeyback handler)
		    {
			    var ret=UIInputMG.Shared.IsOnKeybackEnabled(handler);
			    return ret;
		    }
		    return false;
	    }
	    set
	    {
		    if (this is IWithKeyback handler)
		    {
			    if (value)
			    {
				    UIInputMG.Shared.RegisterOnKeyback(handler);
			    }
			    else
			    {
				    UIInputMG.Shared.UnregisterOnKeyback(handler);
			    }
		    }
	    }
    }

    #endregion
}