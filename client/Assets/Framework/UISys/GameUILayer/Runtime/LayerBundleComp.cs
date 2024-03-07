using System;
using System.Linq;
using System.Threading.Tasks;
using gcc.layer;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UISys.Runtime
{
	[AddComponentMenu("UISys/LayerBundle")]
	public class LayerBundleComp : LayerBundleCompBase
	{
		[SerializeField] protected AssetReferenceT<UILayerRootRefer> layerRootRefer;
		[SerializeField] protected OpenLayerConfig[] initLayers;

		[NonSerialized] private LayerBundle _layerBundle;

		public Task<LayerRootConfig> LoadLayerRootConfig()
		{
			return UILayerUtils.ToLayerRootConfigTask(layerRootRefer);
		}

		public override LayerBundle LoadLayerBundle()
		{
			Debug.Assert(layerRootRefer.RuntimeKeyIsValid());
			if (_layerBundle == null)
			{
				_layerBundle = new();
				var  layerRootConfig=LoadLayerRootConfig();
				_layerBundle.LayerRootConfig = layerRootConfig;
				_layerBundle.LayerConfigs.AddRange(initLayers.Select(info => new OpenLayerInfo()
				{
					uri = UILayerUtils.ToLayerUri(info.layer, info.uri),
					resUri = UILayerUtils.ToLayerResUri(info.layer),
					layerRoot = UILayerUtils.ToLayerRootConfigTask(info.layerRootRef, layerRootConfig),
				}));
			}

			return _layerBundle;
		}

		private void OnDestroy()
		{
			if (layerRootRefer != null && layerRootRefer.IsValid())
			{
				layerRootRefer.ReleaseAsset();
			}
			_layerBundle = null;
		}
	}
}