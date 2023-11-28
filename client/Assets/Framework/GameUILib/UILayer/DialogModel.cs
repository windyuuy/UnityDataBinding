
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RSG;

namespace gcc.layer
{

    using TLayerUri = System.String;
    using Node = UnityEngine.Transform;
    using boolean = System.Boolean;
    using number = System.Double;
    using TLayerOrder = System.Int32;

    /**
	 * 对话框的状态
	 */
    public enum LayerState
    {
        /**
		 * 待创建
		 */
        BeforeCreate = 0,
        /**
		 * 已经初始化
		 */
        Inited,
        /**
		 * 是否屏蔽
		 */
        Shield,
        /**
		 * 是否显露
		 */
        Exposed,
        /**
		 * 已经销毁
		 */
        Destroyed,
    }

    /**
	 * 内部调用的对话框接口
	 */
    public interface ILayerLifecycleInnerCall
    {
	    void __callOnEnter();
	    void __callDoClose(System.Action resolve, System.Action<Exception> reject);
	    void __callDoOpen(System.Action finished, System.Action<Exception> reject);
	    void __callOnExposed();
	    void __callOnShield();

	    /**
		 * 初次创建调用
		 */
	    void __callOnCreate(Object data = null);

	    Task __callOnOpening();
	    void __callOnOpened();

	    /**
		 * 每次由隐藏变为显示调用
		 */
	    Task __callOnShow();

	    /// <summary>
	    /// 重复显示时调用
	    /// </summary>
	    Task __callOnOverlapShow(ShowLayerParam paras);

	    /**
		 * 每次由显示变为隐藏调用
		 */
	    Task __callOnHide();

	    /**
		 * 开始关闭调用
		 */
	    Task __callOnClosing();

	    /**
		 * 关闭调用
		 */
	    void __callOnBeforeClosed();

	    /**
		 * 关闭调用
		 */
	    void __callOnClosed();

	    /**
		 * 销毁前调用
		 */
	    void __callOnBeforeDestroy();

	    /**
		 * 顶级图层改变时调用
		 */
	    void __callOnCoverChanged();

	    /**
		 * 全局任意图层焦点改变时调用
		 */
	    void __callOnAnyFocusChanged(boolean focus);

	    /**
		 * 释放图层前调用
		 */
	    void __callOnBeforeRelease();
    }
    
    /**
	 * 内部调用的对话框接口
	 */
    public interface ILayerInnerCall
    {
	    ILayerLifecycleInnerCall Lifecycle { get; }

        ILayerRoot LayerRootComp { get; set; }
        Node Node { get; }

        LayerModel LayerModel { get; set; }
        object RawData { get; }

        /**
		 * 显示对话框
		 */
        Promise<LayerModel> Show();
        
        /**
		 * 隐藏对话框
		 */
        // Promise<LayerModel> Hide();
        /**
		 * 关闭对话框
		 */
        Promise<LayerModel> Close(boolean destroyOnClose = false);
    }

    /**
	 * 对话框的模型
	 */
    public class LayerModel
    {
        public virtual void Destroy()
        {
            this.IsValid = false;
            this.Node = null;
            this.Comp = null;
            this.TagFilter = null;
            this.LayerOrderMG = null;
            this.Tags.Clear();
            this.Data = null;
        }

        public LayerState State = LayerState.BeforeCreate;
        /**
		 * 被封面覆盖
		 */
        public boolean BeCovered = false;
        /**
		 * 对话框的根节点
		 */
        public Node Node;

        public ILayerInnerCall Comp;
		public T GetTheComp<T>() where T : class, ILayerInnerCall
		{
			return Comp as T;
		}

		public boolean DestroyOnClose = false;

        /**
		 * 资源是否被释放
		 */
        public boolean IsResReleased = false;

        public boolean IsValid = true;

        /**
		 * 分配的tag过滤器
		 */
        public TagFilter TagFilter;
        /**
		 * 图层管理
		 */
        public LayerOrderMG LayerOrderMG;

        protected List<string> _tags = new List<string>();
        /**
		 * tag列表
		 */
        public virtual List<string> Tags
        {
            get
            {
                return this._tags;
            }
            set
            {
                if (value != null)
                {
                    this._tags.Clear();
                    this._tags.AddRange(value);
                }
                else
                {
                    this._tags.Clear();
                }
                this.TagFilter.UpdateTargetTags(this, value);
            }
        }

        /**
		 * 获取标记层级
		 */
        public virtual int GetTagOrder()
        {
            // return -1;
            var order = this.LayerOrderMG.GetOrder(this.Tags);
            return order;
        }

        public virtual int GetResUriOrder()
        {
	        var order = this.LayerOrderMG.GetOrder(this.ResUri);
	        return order;
        }

        public virtual int GetUriOrder()
        {
	        var order = Math.Max(this.LayerOrderMG.GetOrder(this.Uri),this.LayerOrderMG.GetOrder(this.ResUri));
	        return order;
        }

        /**
		 * 获取图层层级
		 */
        public virtual double GetLayerOrder()
        {
            return this.Node.GetSiblingIndex();
        }

        /**
		 * 模型ID
		 */
        public string Uri;
        /**
		 * 资源uri
		 */
        public string ResUri;
        /**
		 * 图层数据
		 */
        public object Data;
        /**
		 * 是否中断异步展示流程
		 */
        public boolean IsCancelShowing;

        /**
		 * 正在准备显示对话框
		 */
        public boolean IsShowing;
        /**
		 * 对话框是否已经打开
		 */
        public boolean IsOpen;

        /**
		 * 是否封面
		 */
        public boolean IsCover = false;

        /**
		 * show图层的顺序
		 * - 减少对资源异步加载顺序的依赖
		 */
        public TLayerOrder ShowLayerOrder = -1;

        public LayerModel(string uri = null, object data = null)
        {
            this.Uri = uri;
            this.Data = data;
            this.IsCancelShowing = false; ;
            this.IsShowing = false;
            this.IsOpen = false;


            this.onInit();
        }

        protected virtual void onInit()
        {

        }
    }

}
