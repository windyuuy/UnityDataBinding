using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gcc.layer;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace UISys.Runtime
{
	public static class UILayerUtils
	{
		public static OpenLayerParam ToOpenLayerParam(AssetReferenceT<UILayer> layer,
			AssetReferenceT<UILayerRootRefer> layerRootRef, string uri, Transform layerRootDefault = null)
		{
			if (layer.RuntimeKeyIsValid())
			{
				var resUri = layer.RuntimeKey.ToString();
				uri = ToLayerUri(layer, uri);

				var loadLayerRootTask = ToLayerRootTask(layerRootRef, layerRootDefault);

				return new OpenLayerParam(uri, resUri, loadLayerRootTask);
			}
			else
			{
				throw new ArgumentException("layer asset cannot be null");
			}
		}

		public static Task<Transform> ToLayerRootTask(AssetReferenceT<UILayerRootRefer> layerRootRef,
			Transform layerRootDefault)
		{
			Task<Transform> loadLayerRootTask;
			if (layerRootRef != null)
			{
				loadLayerRootTask = ToLayerRootTask(layerRootRef);
			}
			else if (layerRootDefault != null)
			{
				loadLayerRootTask = Task.FromResult(layerRootDefault);
			}
			else
			{
				loadLayerRootTask = null;
			}

			return loadLayerRootTask;
		}

		public static Task<Transform> ToLayerRootTask(AssetReferenceT<UILayerRootRefer> layerRootRef)
		{
			if (layerRootRef != null && layerRootRef.RuntimeKeyIsValid())
			{
				return ToLayerRootTaskInternal(layerRootRef);
			}

			return null;
		}

		private static async Task<Transform> ToLayerRootTaskInternal(AssetReferenceT<UILayerRootRefer> layerRootRef)
		{
			var layerRootAsset0 = await Addressables.LoadAssetAsync<Object>(layerRootRef).Task;
			var layerRootAsset = (UILayerRootRefer)layerRootAsset0;
			var layerRoot = layerRootAsset.LayerRootConfig.LayerRoot;
			return layerRoot;
		}

		public static CloseLayerParam ToCloseLayerParam(AssetReferenceT<UILayer> layer, string uri)
		{
			if (layer != null && layer.RuntimeKeyIsValid())
			{
				uri = ToLayerUri(layer, uri);
				return new CloseLayerParam(uri);
			}
			else
			{
				throw new ArgumentException("layer asset cannot be null");
			}
		}

		public static DestroyLayerParam ToDestroyLayerParam(AssetReferenceT<UILayer> layer, string uri)
		{
			if (layer != null && layer.RuntimeKeyIsValid())
			{
				uri = ToLayerUri(layer, uri);
				return new DestroyLayerParam(uri);
			}
			else
			{
				throw new ArgumentException("layer asset cannot be null");
			}
		}

		private static List<IResourceLocation> _sharedLocations = new();

		public static string ToLayerUri(AssetReference layer)
		{
			string uri;
			foreach (var resourceLocator in Addressables.ResourceLocators)
			{
				// TODO: 优化性能
				resourceLocator.Locate(layer.RuntimeKey, typeof(Object), out var locations);
				{
					if (locations == null)
					{
						continue;
					}

					if (locations.Count > 0)
					{
						foreach (var resourceLocation in locations)
						{
							uri = LayerUriUtil.ExtractUri(resourceLocation.InternalId);
							if (!string.IsNullOrEmpty(uri))
							{
								return uri;
							}
						}
					}
				}
			}

			if (layer != null && layer.RuntimeKeyIsValid())
			{
				var resUri = layer.RuntimeKey.ToString();
				uri = LayerUriUtil.ExtractUri(resUri);
				if (!string.IsNullOrEmpty(uri))
				{
					return uri;
				}
			}

			throw new ArgumentException("layer asset cannot be null");
		}

		public static string ToLayerUri(AssetReference layer, string uri)
		{
			return string.IsNullOrEmpty(uri) ? ToLayerUri(layer) : uri;
		}

		public static string ToLayerResUri(AssetReference layer)
		{
			return layer.RuntimeKey.ToString();
		}

		public static async Task<LayerBundleComp> ToLayerBundleComp(AssetReferenceT<LayerBundleComp> layerBundleRefer)
		{
			var go = await Addressables.LoadAssetAsync<GameObject>(layerBundleRefer).Task;
			return go.GetComponent<LayerBundleComp>();
		}

		public static async Task<LayerBundle> ToLayerBundle(AssetReferenceT<LayerBundleComp> layerBundleRefer)
		{
			var layerBundleComp = await ToLayerBundleComp(layerBundleRefer);
			return layerBundleComp.LayerBundle;
		}

		/// <summary>
		/// delay release to improve performance
		/// </summary>
		/// <param name="uiActionSelf"></param>
		public static async ValueTask DelayRelease(AssetReference uiActionSelf)
		{
			if (uiActionSelf != null && uiActionSelf.IsValid())
			{
				await AsyncUtils.WaitForEndOfFrame();
				uiActionSelf.ReleaseAsset();
			}
		}
		
		public static async Task<Object> LoadAsset(AssetReference uiActionSelf)
		{
			return uiActionSelf.Asset != null
				? uiActionSelf.Asset
				: await uiActionSelf.LoadAssetAsync<Object>().Task;
		}
	}
}