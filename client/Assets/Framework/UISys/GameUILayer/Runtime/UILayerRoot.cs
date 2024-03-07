using System;
using System.Threading.Tasks;
using gcc.layer;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UISys.Runtime
{
	public class UILayerRoot : MonoBehaviour
	{
		[SerializeField] protected AssetReferenceT<UILayerRootRefer> layerRootRefer;
		[SerializeField] protected CurrentLayerBundleComp currentLayerBundleRefer;
		[NonSerialized] public LayerRootConfig LayerRootConfig;

		void Awake()
		{
			this.Init();
		}

		private bool _inited = false;

		public void Init()
		{
			if (this._inited)
			{
				return;
			}

			this._inited = true;

			this.LayerRootConfig = new(this.transform);
			this.LayerRootConfig.LayerManager.RegisterLayerClass(typeof(UILayer));

			UpdateUILayerRootRefer();

			UpdateLayerBundleRefer();
		}

		private void UpdateLayerBundleRefer()
		{
			if (currentLayerBundleRefer != null)
			{
				currentLayerBundleRefer.LayerBundleRefer = this.LayerRootConfig.LayerBundleManager.LayerBundleRefer;
			}
		}

		private async Task UpdateUILayerRootRefer()
		{
			if (layerRootRefer != null)
			{
				var layerRoot = await layerRootRefer.LoadAssetAsync().Task;
				layerRoot.Target = this;
			}
			else
			{
				Debug.LogError("UILayerRoot.uiLayerRootRefer cannot be null");
			}
		}

		private void OnDestroy()
		{
			if (layerRootRefer.IsValid())
			{
				layerRootRefer.ReleaseAsset();
			}
		}
	}
}