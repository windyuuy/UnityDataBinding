
namespace gcc.layer
{

	using boolean = System.Boolean;

	public class ShowLayerParam
	{
		/**
		 * 不播放动画, 立即打开
		 */
		public boolean OpenInstant;
		/**
		 * 关闭时直接销毁
		 */
		public boolean DestroyOnClose;
		protected string _uri;
		protected object _data;
		protected string[] _tags;
		protected string _resUri;
		public string[] Tags
		{
			get
			{
				return this._tags;
			}
			set
			{
				this._tags = value;
			}
		}

		/**
		 * 
		 * @param uri 图层uri (约定: 图层预制体的资源uri缺省使用 `${路径前缀}/${uri}`, 路径前缀默认为 "prefabs/ui")
		 * @param data 图层模型中的自定义数据
		 * @param resUri 图层预制体的资源uri (约定: 图层预制体的资源uri缺省使用 `${路径前缀}/${uri}`, 路径前缀默认为 "prefabs/ui")
		 * @param tags 图层标签
		 */
		public ShowLayerParam(string uri, object data = null, string resUri = null, boolean reuseData = false, string[] tags = null)
		{
			this.Uri = uri;
			this.Data = data;
			this.ResUri = resUri ?? uri;
			this.ReuseData = reuseData;
			this.Tags = tags;


			this.onInit();
		}

		internal ShowLayerParam(string uri, bool reuseData)
		{
			this.Uri = uri;
			this.ReuseData = reuseData;
		}

		protected void onInit()
		{

		}

		public string Uri
		{
			get
			{
				return this._uri ?? "";
			}
			set
			{
				this._uri = value;
			}
		}

		public string ResUri
		{
			get
			{
				return this._resUri;
			}
			set
			{
				this._resUri = value.StartsWith("Assets/") ? value : LayerUriUtil.WrapUri(value);
			}
		}

		public object Data
		{
			get
			{
				return this._data;
			}
			set
			{
				this._data = value;
			}
		}

		/**
		 * true: 如果data为空, 则复用之前的模型数据
		 * false: 始终更新data
		 */
		public boolean ReuseData;

		/**
		 * 隐藏loading界面
		 */
		public boolean? HideLoading;

	}

}
