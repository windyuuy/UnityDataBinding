using System;
using gcc.layer;
using UnityEngine;
using UnityEngine.Serialization;

namespace UISys.Runtime
{
	public class UILayerRoot : MonoBehaviour
	{
		[SerializeField] protected UILayerRootRefer uiLayerRootRefer;
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

		private void UpdateUILayerRootRefer()
		{
			if (uiLayerRootRefer != null)
			{
				uiLayerRootRefer.Target = this;
			}
			else
			{
				Debug.LogError("UILayerRoot.uiLayerRootRefer cannot be null");
			}
		}
	}
}