
using System;
using System.Collections.Generic;

namespace gcc.layer
{

	public interface IShowLayerParams
	{
		public static string Default(string uri)
		{
			return LayerUriUtil.WrapUri(uri);
		}

		protected static Dictionary<string, string> uriDict = new Dictionary<string, string>();
		public static string GetUri(Type type)
		{
			if (uriDict.ContainsKey(type.FullName))
			{
				return uriDict[type.FullName];
			}
			else
			{
				var p0 = Activator.CreateInstance(type);
				if (p0 is IShowLayerParams pp)
				{
					var Uri = pp.Uri;
					if (Uri != null)
					{
						uriDict[type.FullName] = Uri;
						return Uri;
					}
					else
					{
						throw new System.Exception($"Invalid Uri detected for Type<{type.FullName}:IShowLayerParams>");
					}
				}
				else
				{
					throw new System.Exception($"Invalid Uri detected for Type<{type.FullName}>");
				}
			}
		}
		public static string GetUri<T>()
		{
			return GetUri(typeof(T));
		}
		public static void SetUri(string Uri, Type type)
		{
			uriDict[type.FullName] = Uri;
		}

		/// <summary>
		/// 资源加载路径
		/// </summary>
		public string ResUri
		{
			get
			{
				return LayerUriUtil.WrapUri(Uri);
			}
		}
		public string Uri { get; }
	}
}

