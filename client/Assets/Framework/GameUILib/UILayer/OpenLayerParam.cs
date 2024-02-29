using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace gcc.layer
{
	public struct OpenLayerParam
	{
		[NonSerialized] public Task<Transform> LoadLayerRootTask;

		public string Uri;

		public string ResUri;

		[NonSerialized] public object Data;

		/**
		 *
		 * @param uri 图层uri (约定: 图层预制体的资源uri缺省使用 `${路径前缀}/${uri}`, 路径前缀默认为 "prefabs/ui")
		 * @param data 图层模型中的自定义数据
		 * @param resUri 图层预制体的资源uri (约定: 图层预制体的资源uri缺省使用 `${路径前缀}/${uri}`, 路径前缀默认为 "prefabs/ui")
		 * @param tags 图层标签
		 */
		public OpenLayerParam(string uri, string resUri = null, Task<Transform> loadLayerRootTask = null,
			object data = null) : this()
		{
			this.LoadLayerRootTask = loadLayerRootTask;
			this.Data = data;
			SetUri(uri, resUri);
		}

		internal OpenLayerParam(string uri) : this()
		{
			SetUri(uri, null);
		}

		private void SetUri(string uri, string resUri)
		{
			this.Uri = uri;
			this.ResUri = string.IsNullOrEmpty(resUri) ? LayerUriUtil.WrapUri(uri) : resUri;
		}
	}
}