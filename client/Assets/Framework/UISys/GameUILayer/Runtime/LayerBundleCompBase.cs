using System;
using System.Threading.Tasks;
using gcc.layer;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UISys.Runtime
{
	[Serializable]
	public class OpenLayerConfig
	{
		public AssetReferenceGameObject layer;
		public AssetReferenceT<UILayerRootRefer> layerRootRef;
		public string uri;
	}

	public abstract class LayerBundleCompBase : MonoBehaviour
	{
		public abstract LayerBundle LoadLayerBundle();

		[UIAction]
		public LoadLayerBundleStatus Preload()
		{
			var layerBundle = LoadLayerBundle();
			return layerBundle.Preload();
		}

		[UIAction]
		public Task Open()
		{
			var layerBundle = LoadLayerBundle();
			return layerBundle.Open();
		}

		[UIAction]
		public Task Close()
		{
			var layerBundle = LoadLayerBundle();
			return layerBundle.Close();
		}

		[UIAction]
		public void Shield()
		{
			var layerBundle = LoadLayerBundle();
			layerBundle.Shield();
		}

		[UIAction]
		public void Expose()
		{
			var layerBundle = LoadLayerBundle();
			layerBundle.Expose();
		}

		[UIAction]
		public Task<ILayer> OpenLayer(AssetReferenceT<UILayer> layer,
			AssetReferenceT<UILayerRootRefer> layerRootRef, string uri)
		{
			var layerBundle = LoadLayerBundle();
			return layerBundle.OpenLayer(UILayerUtils.ToOpenLayerParam(layer, layerRootRef, uri));
		}

		[UIAction]
		public Task<ILayer> CloseLayer(AssetReferenceT<UILayer> layer, string uri)
		{
			var layerBundle = LoadLayerBundle();
			return layerBundle.CloseLayer(UILayerUtils.ToCloseLayerParam(layer, uri));
		}

		[UIAction]
		public async Task PushAndOpen(AssetReferenceT<UILayerRootRefer> layerRootRef)
		{
			var layerBundle = LoadLayerBundle();
			var layerRoot = await layerRootRef.LoadAssetAsync<UILayerRootRefer>().Task;
			await layerRoot.LayerBundleManager.PushAndOpen(layerBundle);
		}

		[UIAction]
		public async Task PopAndClose(AssetReferenceT<UILayerRootRefer> layerRootRef)
		{
			if (layerRootRef != null)
			{
				var layerRoot = await layerRootRef.LoadAssetAsync<UILayerRootRefer>().Task;
				await layerRoot.LayerBundleManager.PopAndClose();
			}
			else
			{
				Debug.LogException(
					new InvalidOperationException("LayerBundleCompBase:PopAndClose layerRootRef cannot be null"));
			}
		}
	}
}