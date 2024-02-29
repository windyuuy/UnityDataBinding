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
		public Task<Transform> layerRoot;

		public OpenLayerParam ToOpenLayerParam()
		{
			return new OpenLayerParam(this.uri, this.resUri,
				this.layerRoot);
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

		public LayerRootConfig LayerRoot;
		public TLayerManager LayerManager => LayerRoot.LayerManager;

		private OpenLayerParam WrapOpenParam(OpenLayerInfo config)
		{
			return new OpenLayerParam(config.uri, config.resUri,
				config.layerRoot ?? Task.FromResult(LayerRoot.LayerRoot));
		}

		private OpenLayerParam WrapOpenParam(OpenLayerParam config)
		{
			return new OpenLayerParam(config.Uri, config.ResUri,
				config.LoadLayerRootTask ?? Task.FromResult(LayerRoot.LayerRoot));
		}

		public LoadLayerBundleStatus Preload()
		{
			var tasks = LayerConfigs
				.Select(config =>
					LayerManager.PreloadLayer(WrapOpenParam(config))
						.LoadTask);
			var loadTask = Task.WhenAll(tasks);
			var result = new LoadLayerBundleStatus
			{
				Task = loadTask
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

		public void Shield()
		{
			IsPaused = true;

			foreach (var item in LayerStatus.Values)
			{
				if (IsLayerOpenInBundle(item.Uri))
				{
					LayerManager.ShieldLayer(item.Uri);
				}
			}
		}

		public void Expose()
		{
			IsPaused = false;

			foreach (var item in LayerStatus.Values)
			{
				var isOpenInBundle = item.IsOpenInBundle;
				if (isOpenInBundle)
				{
					LayerManager.OpenLayer(WrapOpenParam(item.Config));
					LayerManager.ExposeLayer(item.Uri);
				}
			}
		}

		public Task<ILayer> OpenLayer(OpenLayerParam config)
		{
			MarkLayerStatus(config);
			if (!IsOpen)
			{
				return Task.FromResult<ILayer>(null);
			}
			else if (IsPaused)
			{
				return Task.FromResult<ILayer>(null);
			}
			else
			{
				return LayerManager.OpenLayer(WrapOpenParam(config));
			}
		}

		public Task<ILayer> CloseLayer(string uri)
		{
			return CloseLayer(new CloseLayerParam(uri));
		}

		public Task<ILayer> CloseLayer(CloseLayerParam closeLayerParam)
		{
			var isOpenInBundle = IsLayerOpenInBundle(closeLayerParam.Uri);
			RemoveLayerStatus(closeLayerParam.Uri);
			if (isOpenInBundle)
			{
				return LayerManager.CloseLayer(closeLayerParam);
			}
			else
			{
				return Task.FromResult<ILayer>(null);
			}
		}
	}
}