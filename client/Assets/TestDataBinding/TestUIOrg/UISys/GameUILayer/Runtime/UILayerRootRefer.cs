using System.Threading.Tasks;
using gcc.layer;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UISys.Runtime
{
	[CreateAssetMenu(fileName = "UILayerRootRefer.asset", menuName = "UISys/UILayerRootRefer")]
	public class UILayerRootRefer : GameObjectInstanceRefer
	{
		protected UILayerRoot LayerRootComp;

		internal void SetUILayerRoot(UILayerRoot layerRoot)
		{
			LayerRootComp = layerRoot;
		}

		public LayerRootConfig LayerRootConfig => LayerRootComp.LayerRootConfig;
		public TLayerManager LayerManager => LayerRootComp.LayerRootConfig.LayerManager;
		public Transform LayerRoot => LayerRootConfig.LayerRoot;
		public LayerBundleManager LayerBundleManager => LayerRootConfig.LayerBundleManager;

		public override UILayerRoot Target
		{
			get => LayerRootComp;
			set => LayerRootComp = value;
		}

		[UIAction]
		public virtual async Task<LoadingLayerStatus> PreloadLayer(AssetReferenceT<UILayer> layer,
			AssetReferenceT<UILayerRootRefer> layerRootRef, string uri)
		{
			return LayerManager.PreloadLayer(
				(UILayerUtils.ToOpenLayerParam(layer, layerRootRef, uri, this.LayerRootConfig.LayerRoot)));
		}

		[UIAction]
		public virtual async Task<ILayer> OpenLayer(AssetReferenceT<UILayer> layer,
			AssetReferenceT<UILayerRootRefer> layerRootRef, string uri)
		{
			return await LayerManager.OpenLayer(
				(UILayerUtils.ToOpenLayerParam(layer, layerRootRef, uri, this.LayerRootConfig.LayerRoot)));
		}

		[UIAction]
		public virtual async Task<ILayer> CloseLayer(AssetReferenceT<UILayer> layer, string uri)
		{
			return await LayerManager.CloseLayer((UILayerUtils.ToCloseLayerParam(layer, uri)));
		}

		[UIAction]
		public virtual LoadingLayerStatus FindLayer(AssetReferenceT<UILayer> layer, string uri)
		{
			return LayerManager.FindLayer(UILayerUtils.ToLayerUri(layer, uri));
		}

		[UIAction]
		public virtual Task DestroyLayer(AssetReferenceT<UILayer> layer, string uri)
		{
			return LayerManager.DestroyLayer(UILayerUtils.ToDestroyLayerParam(layer, uri));
		}

		#region LayerBundle

		[UIAction]
		public virtual async Task PushAndOpenBundle(AssetReferenceT<LayerBundleComp> layerBundleRefer)
		{
			var layerBundle = await UILayerUtils.ToLayerBundle(layerBundleRefer);
			await this.LayerBundleManager.PushAndOpen(layerBundle);
		}

		[UIAction]
		public virtual Task PopAndCloseBundle()
		{
			return LayerBundleManager.PopAndClose();
		}

		[UIAction]
		public virtual async Task ReplaceAndOpenBundle(AssetReferenceT<LayerBundleComp> layerBundleRefer)
		{
			var layerBundle = await UILayerUtils.ToLayerBundle(layerBundleRefer);
			await LayerBundleManager.ReplaceAndOpen(layerBundle);
		}

		[UIAction]
		public virtual async Task PopAndCloseToBundle(AssetReferenceT<LayerBundleComp> layerBundleRefer)
		{
			var layerBundle = await UILayerUtils.ToLayerBundle(layerBundleRefer);
			await LayerBundleManager.PopAndCloseTo(layerBundle);
		}

		#endregion
	}
}