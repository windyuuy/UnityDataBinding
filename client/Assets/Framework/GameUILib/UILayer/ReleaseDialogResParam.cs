
namespace gcc.layer
{

	public class ReleaseLayerResParam
	{
		protected string _uri;
		protected string _resUri;

		/**
		 * 
		 * @param uri 图层uri (约定: 图层预制体的资源uri缺省使用 `${路径前缀}/${uri}`, 路径前缀默认为 "prefabs/ui")
		 * @param data 图层模型中的自定义数据
		 * @param resUri 图层预制体的资源uri (约定: 图层预制体的资源uri缺省使用 `${路径前缀}/${uri}`, 路径前缀默认为 "prefabs/ui")
		 * @param tags 图层标签
		 */
		public ReleaseLayerResParam(string uri)
		{
			this.uri = uri;
		}

		public string uri
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

		public string resUri
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

	}

}
