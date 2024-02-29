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
		public abstract LayerBundle LayerBundle { get; }

		[UIAction]
		public LoadLayerBundleStatus Preload()
		{
			return LayerBundle.Preload();
		}

		[UIAction]
		public Task Open()
		{
			return LayerBundle.Open();
		}

		[UIAction]
		public Task Close()
		{
			return LayerBundle.Close();
		}

		[UIAction]
		public void Shield()
		{
			LayerBundle.Shield();
		}

		[UIAction]
		public void Expose()
		{
			LayerBundle.Expose();
		}

		[UIAction]
		public Task<ILayer> OpenLayer(AssetReferenceT<UILayer> layer,
			AssetReferenceT<UILayerRootRefer> layerRootRef, string uri)
		{
			return LayerBundle.OpenLayer(UILayerUtils.ToOpenLayerParam(layer, layerRootRef, uri));
		}

		[UIAction]
		public Task<ILayer> CloseLayer(AssetReferenceT<UILayer> layer, string uri)
		{
			return LayerBundle.CloseLayer(UILayerUtils.ToCloseLayerParam(layer, uri));
		}

		[UIAction]
		public async Task PushAndOpen(AssetReferenceT<UILayerRootRefer> layerRootRef)
		{
			var layerRoot = await layerRootRef.LoadAssetAsync<UILayerRootRefer>().Task;
			await layerRoot.LayerBundleManager.PushAndOpen(this.LayerBundle);
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