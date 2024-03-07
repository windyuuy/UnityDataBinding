using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace gcc.layer
{
	public class LoadLayerBundleStatus
	{
		public Task Task;
	}

	[Serializable]
	public class OpenLayerInfo
	{
		public string uri;
		public string resUri;
		public Task<LayerRootConfig> layerRoot;

		public Task<Transform> LoadLayerRoot()
		{
			if (this.layerRoot != null)
			{
				return LoadLayerRootInternal();
			}

			return null;
		}

		protected async Task<Transform> LoadLayerRootInternal()
		{
			var r = await this.layerRoot;
			return r.LayerRoot;
		}

		public OpenLayerParam ToOpenLayerParam()
		{
			return new OpenLayerParam(this.uri, this.resUri,
				LoadLayerRoot());
		}
	}

	public class LayerBundle
	{
		public class OpenLayerStatus
		{
			public OpenLayerParam Config;
			public string Uri => Config.Uri;
			public bool IsOpenInBundle;
		}

		public class SharedLayerStatus
		{
			public string Uri;
			public LayerBundle LayerBundle;
			public int ReferCount = 0;

			public SharedLayerStatus(string uri)
			{
				Uri = uri;
			}
		}

		public string Name { get; set; }

		[NonSerialized] public List<OpenLayerInfo> LayerConfigs = new();

		protected Dictionary<string, OpenLayerStatus> LayerStatus = new();
		private static readonly Dictionary<string, SharedLayerStatus> SharedStatus = new();

		public Task<LayerRootConfig> LayerRootConfig;

		public static Task<Transform> ToLayerRootTask(Task<LayerRootConfig> layerRootRef)
		{
			if (layerRootRef != null)
			{
				async Task<Transform> Load()
				{
					var r = await layerRootRef;
					return r.LayerRoot;
				}

				return Load();
			}

			return null;
		}

		private OpenLayerParam WrapOpenParam(OpenLayerInfo config)
		{
			return new OpenLayerParam(config.uri, config.resUri,
				config.LoadLayerRoot() ?? ToLayerRootTask(LayerRootConfig));
		}

		private OpenLayerParam WrapOpenParam(OpenLayerParam config)
		{
			return new OpenLayerParam(config.Uri, config.ResUri,
				config.LoadLayerRootTask ?? ToLayerRootTask(LayerRootConfig));
		}

		public LoadLayerBundleStatus Preload()
		{
			async Task Load()
			{
				var layerManager = (await LayerRootConfig).LayerManager;
				var tasks = LayerConfigs
					.Select(config =>
						layerManager.PreloadLayer(WrapOpenParam(config))
							.LoadTask);
				var loadTask = Task.WhenAll(tasks);
				await loadTask;
			}

			var result = new LoadLayerBundleStatus
			{
				Task = Load()
			};
			return result;
		}

		public Task Open()
		{
			if (IsOpen)
			{
				return Task.CompletedTask;
			}
			else
			{
				IsPaused = false;
			}

			IsOpen = true;

			var tasks = Enumerable.Empty<Task>();
			foreach (var config in LayerConfigs)
			{
				if (!LayerStatus.ContainsKey(config.uri))
				{
					var task = OpenLayer(WrapOpenParam(config));
					tasks = tasks.Append(task);
				}
			}

			return Task.WhenAll(tasks);
		}

		public Task Close()
		{
			if (!IsOpen)
			{
				return Task.CompletedTask;
			}

			IsOpen = false;

			var tasks = Enumerable.Empty<Task>();
			foreach (var config in LayerStatus.Values.ToArray())
			{
				if (IsLayerOpenInBundle(config.Uri))
				{
					var task = CloseLayer(config.Uri);
					tasks = tasks.Append(task);
				}
			}

			return Task.WhenAll(tasks);
		}

		protected void MarkLayerStatus(OpenLayerParam config)
		{
			var uri = config.Uri;
			var isNew = !this.LayerStatus.TryGetValue(uri, out var status);
			if (isNew)
			{
				status = new OpenLayerStatus();
				this.LayerStatus.Add(uri, status);
			}

			status.Config = config;
			status.IsOpenInBundle = true;

			if (!SharedStatus.TryGetValue(uri, out var sharedStatus))
			{
				sharedStatus = new(uri);
				SharedStatus.Add(uri, sharedStatus);
			}

			sharedStatus.LayerBundle = this;
			if (isNew)
			{
				sharedStatus.ReferCount++;
			}
		}

		public bool IsLayerOpenInBundle(string uri)
		{
			if (this.LayerStatus.TryGetValue(uri, out var status))
			{
				if (status.IsOpenInBundle)
				{
					if (SharedStatus.TryGetValue(uri, out var sharedStatus))
					{
						if (sharedStatus.LayerBundle == this)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		protected void RemoveLayerStatus(string uri)
		{
			if (this.LayerStatus.ContainsKey(uri))
			{
				this.LayerStatus.Remove(uri);

				if (SharedStatus.TryGetValue(uri, out var sharedStatus))
				{
					sharedStatus.ReferCount--;
					if (sharedStatus.ReferCount == 0)
					{
						SharedStatus.Remove(uri);
					}
				}
				else
				{
					throw new Exception("SharedStatus Incorrect, uri info missing");
				}
			}
		}

		protected bool IsOpen = false;
		protected bool IsPaused = false;

		public async void Shield()
		{
			IsPaused = true;

			var layerManager = (await LayerRootConfig).LayerManager;
			foreach (var item in LayerStatus.Values)
			{
				if (IsLayerOpenInBundle(item.Uri))
				{
					layerManager.ShieldLayer(item.Uri);
				}
			}
		}

		public async void Expose()
		{
			IsPaused = false;

			var layerManager = (await LayerRootConfig).LayerManager;
			foreach (var item in LayerStatus.Values)
			{
				var isOpenInBundle = item.IsOpenInBundle;
				if (isOpenInBundle)
				{
					layerManager.OpenLayer(WrapOpenParam(item.Config));
					layerManager.ExposeLayer(item.Uri);
				}
			}
		}

		public async Task<ILayer> OpenLayer(OpenLayerParam config)
		{
			MarkLayerStatus(config);
			if (!IsOpen)
			{
				return null;
			}
			else if (IsPaused)
			{
				return null;
			}
			else
			{
				var layerManager = (await LayerRootConfig).LayerManager;
				return await layerManager.OpenLayer(WrapOpenParam(config));
			}
		}

		public Task<ILayer> CloseLayer(string uri)
		{
			return CloseLayer(new CloseLayerParam(uri));
		}

		public async Task<ILayer> CloseLayer(CloseLayerParam closeLayerParam)
		{
			var isOpenInBundle = IsLayerOpenInBundle(closeLayerParam.Uri);
			RemoveLayerStatus(closeLayerParam.Uri);
			if (isOpenInBundle)
			{
				var layerManager = (await LayerRootConfig).LayerManager;
				return await layerManager.CloseLayer(closeLayerParam);
			}
			else
			{
				return null;
			}
		}
	}
}