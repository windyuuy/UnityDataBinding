using System;
using System.Linq;
using gcc.layer;
using UnityEditor;
using UnityEngine;

namespace UISys.Runtime
{
	[AddComponentMenu("UISys/LayerBundle")]
	public class LayerBundleComp : LayerBundleCompBase
	{
		[SerializeField] protected UILayerRootRefer layerRoot;
		[SerializeField] protected OpenLayerConfig[] initLayers;

		[NonSerialized] private LayerBundle _layerBundle;

		public override LayerBundle LayerBundle
		{
			get
			{
				if (_layerBundle == null)
				{
					_layerBundle = new();
					if (layerRoot != null)
					{
						_layerBundle.LayerRoot = layerRoot.LayerRootConfig;
					}

					_layerBundle.LayerConfigs.AddRange(initLayers.Select(info => new OpenLayerInfo()
					{
						uri = UILayerUtils.ToLayerUri(info.layer, info.uri),
						resUri = UILayerUtils.ToLayerResUri(info.layer),
						layerRoot = UILayerUtils.ToLayerRootTask(info.layerRootRef, this.layerRoot.LayerRoot),
					}));
				}

				return _layerBundle;
			}
		}

		private void OnDestroy()
		{
			_layerBundle = null;
		}
	}
}