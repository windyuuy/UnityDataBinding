
namespace gcc.layer
{
	using boolean = System.Boolean;

	public class CloseLayerParam
	{
		/**
		 * 不播放动画, 立即关闭
		 */
		public boolean CloseInstant;
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
		public CloseLayerParam(string uri, boolean destroyOnClose = false, boolean instant = false)
		{
			this.Uri = uri;
			this.DestroyOnClose = destroyOnClose;
			this.CloseInstant = instant;
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
				this._resUri = LayerUriUtil.WrapUri(value);
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
		 * 如果dat为空, 则复用之前的模型数据
		 */
		public boolean ReuseData = true;

	}

}
