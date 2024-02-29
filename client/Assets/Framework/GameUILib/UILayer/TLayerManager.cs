using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Node = UnityEngine.Transform;

namespace gcc.layer
{
	[Flags]
	public enum LayerState
	{
		None = 0,
		Loading = 1,
		Init = 1 << 1,
		Loaded = 1 << 2,
		Opening = 1 << 3,
		Opened = 1 << 4,
		Closing = 1 << 5,
		Closed = 1 << 6,
		Disposing = 1 << 7,
		Disposed = 1 << 8,

		MainFlagArea = (1 << 9) - 1,
		MainFlagClear = ~(MainFlagArea),

		Expose = 1 << 10,
		Shield = ~Expose,
	}

	public static class LayerStateExt
	{
		public static bool ContainFlag(this LayerState state, LayerState flag)
		{
			return (state & flag) == flag;
		}

		public static bool NotContainFlag(this LayerState state, LayerState flag)
		{
			return !ContainFlag(state, flag);
		}

		public static void SetMainFlag(ref this LayerState state, LayerState flag)
		{
			state &= LayerState.MainFlagClear;
			state |= flag;
		}
	}

	public class LoadingLayerStatus
	{
		public readonly string Uri;

		public LoadingLayerStatus(string uri)
		{
			this.Uri = uri;
		}

		public LayerState State = LayerState.None;
		public LayerState TargetState = LayerState.None;

		internal Task<ILayer> PendingTask;
		internal Task<ILayer> LoadTask;

		public ILayer Layer;

		protected async Task<ILayer> LinkTask1(Task<ILayer> task1, Func<Task<ILayer>> action)
		{
			await task1;
			return await action();
		}

		protected Task<ILayer> LinkTask(Task<ILayer> task1, Func<Task<ILayer>> action)
		{
			if (task1 != null && !task1.IsCompleted)
			{
				return LinkTask1(task1, action);
			}

			return action();
		}

		internal Task<ILayer> AppendTask(Func<Task<ILayer>> action)
		{
			return PendingTask = LinkTask(PendingTask, action);
		}
	}

	public class TLayerManager
	{
		#region Public

		public virtual LoadingLayerStatus PreloadLayer(string uri)
		{
			return PreloadLayer(new OpenLayerParam(uri));
		}

		public virtual LoadingLayerStatus PreloadLayer(OpenLayerParam paras)
		{
			var status = GetOrCreateLayer(paras.Uri, paras.ResUri, paras.LoadLayerRootTask);
			status.TargetState.SetMainFlag(LayerState.Loaded);
			return status;
		}

		public virtual Task<ILayer> OpenLayer(OpenLayerParam paras)
		{
			var status = GetOrCreateLayer(paras.Uri, paras.ResUri, paras.LoadLayerRootTask);
			var task = status.PendingTask;

			async Task<ILayer> Load()
			{
				var layer = status.Layer;
				if (status.State.ContainFlag(LayerState.Opening) || status.State.ContainFlag(LayerState.Opened))
				{
					return layer;
				}

				status.State.SetMainFlag(LayerState.Opening);
				var layerRoot = paras.LoadLayerRootTask.Result;
				if (layer.transform.parent != layerRoot)
				{
					layer.transform.SetParent(layerRoot, false);
				}

				await layer.Lifecycle.__callOnPrepare();
				layer.Lifecycle.__callOnReady();
				layer.gameObject.SetActive(true);
				var openingTask = layer.Lifecycle.__callOnOpening();
				layer.OpeningTask = openingTask;
				await openingTask;
				status.State.SetMainFlag(LayerState.Opened);
				layer.Lifecycle.__callOnOpened();
				return layer;
			}

			_ = status.AppendTask(Load);

			status.TargetState.SetMainFlag(LayerState.Opened);

			ExposeLayer(status);

			return task;
		}

		public virtual Task<ILayer> CloseLayer(string uri)
		{
			return CloseLayer(new CloseLayerParam(uri));
		}

		public virtual Task<ILayer> CloseLayer(CloseLayerParam paras)
		{
			if (GetLoadingStatus(paras.Uri, out var status))
			{
				var ret = CloseLayerInternal(status);
				return ret;
			}
			else
			{
				return null;
			}
		}

		internal Task<ILayer> CloseLayerInternal(LoadingLayerStatus status)
		{
			_ = ShieldLayer(status);

			async Task<ILayer> Close()
			{
				var layer = status.Layer;
				if (status.State.ContainFlag(LayerState.Closing) || status.State.ContainFlag(LayerState.Closed))
				{
					return layer;
				}

				status.State.SetMainFlag(LayerState.Closing);
				await layer.Lifecycle.__callOnClosing();
				status.State.SetMainFlag(LayerState.Closed);
				layer.Lifecycle.__callOnClosed();
				layer.gameObject.SetActive(false);
				return layer;
			}

			var task = status.AppendTask(Close);
			status.TargetState.SetMainFlag(LayerState.Closed);
			return task;
		}

		public virtual LoadingLayerStatus FindLayer(string uri)
		{
			if (GetLoadingStatus(uri, out var status))
			{
				return status;
			}

			return null;
		}

		private void DestroySafe(string uri, ILayer layer)
		{
			var isValid = layer != null && layer.gameObject != null && layer.gameObject.transform != null;
			if (isValid)
			{
				var transform = layer.gameObject.transform;
				if (transform.parent != null && transform.GetSiblingIndex() >= transform.parent.childCount)
				{
					isValid = false;
				}
			}

			if (isValid)
			{
				Debug.Log("do destroy dialog:" + uri);
				layer.gameObject.SetActive(false);
				respool.MyNodePool.Put(layer.gameObject.transform, true);
			}
		}

		public virtual Task DestroyLayer(DestroyLayerParam paras)
		{
			var uri = paras.Uri;
			if (GetLoadingStatus(uri, out var status))
			{
				_ = CloseLayerInternal(status);

				Task<ILayer> Destroy()
				{
					var layer = status.Layer;
					if (status.State.ContainFlag(LayerState.Disposed) || status.State.ContainFlag(LayerState.Disposing))
					{
						return Task.FromResult(layer);
					}

					status.State.SetMainFlag(LayerState.Disposing);
					layer.Lifecycle.__callOnDispose();
					status.State.SetMainFlag(LayerState.Disposed);
					DestroySafe(uri, layer);

					return Task.FromResult(layer);
				}

				var task = status.AppendTask(Destroy);

				status.TargetState.SetMainFlag(LayerState.Disposed);

				return task;
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region Internal

		protected System.Type LayerClass;

		public void RegisterLayerClass(System.Type cls)
		{
			this.LayerClass = cls;
		}

		public System.Type GetLayerClass()
		{
			return this.LayerClass;
		}

		public virtual Task<ILayer> ExposeLayer(string uri)
		{
			if (GetLoadingStatus(uri, out var status))
			{
				return ExposeLayer(status);
			}
			else
			{
				Debug.LogError("invalid uri not load");
				return Task.FromResult<ILayer>(null);
			}
		}

		public Task<ILayer> ExposeLayer(LoadingLayerStatus status)
		{
			Task<ILayer> Expose()
			{
				var layer = status.Layer;
				if (status.State.NotContainFlag(LayerState.Opened))
				{
					return Task.FromResult(layer);
				}

				if (status.State.ContainFlag(LayerState.Expose))
				{
					return Task.FromResult(layer);
				}

				status.State |= LayerState.Expose;

				layer.Lifecycle.__callOnExpose();
				return Task.FromResult(layer);
			}

			var task = status.AppendTask(Expose);
			status.TargetState |= LayerState.Expose;
			return task;
		}

		public virtual Task<ILayer> ShieldLayer(string uri)
		{
			if (GetLoadingStatus(uri, out var status))
			{
				return ShieldLayer(status);
			}
			else
			{
				Debug.LogError("invalid uri not load");
				return Task.FromResult<ILayer>(null);
			}
		}

		public virtual bool IsLayerBeingExpose(string uri)
		{
			if (GetLoadingStatus(uri, out var status))
			{
				return (status.TargetState & LayerState.Expose) != 0;
			}

			return false;
		}

		public virtual Task<ILayer> ShieldLayer(LoadingLayerStatus status)
		{
			Task<ILayer> Shield()
			{
				var layer = status.Layer;
				if (status.State.NotContainFlag(LayerState.Opened))
				{
					return Task.FromResult(layer);
				}

				if (status.State.NotContainFlag(LayerState.Expose))
				{
					return Task.FromResult(layer);
				}

				status.State &= LayerState.Shield;

				layer.Lifecycle.__callOnShield();
				return Task.FromResult(layer);
			}

			var task = status.AppendTask(Shield);
			status.TargetState &= LayerState.Shield;
			return task;
		}


		protected virtual LoadingLayerStatus GetOrCreateLayer(string uri, string resUri,
			Task<Transform> loadLayerRootTask)
		{
			if (!GetOrCreateLoadingStatus(uri, out var status))
			{
				async Task<ILayer> Load()
				{
					status.State.SetMainFlag(LayerState.Loading);
					var layerRoot = await loadLayerRootTask;
					var loadingTask = CreateLayerInstance(uri, resUri, layerRoot, status);
					status.LoadTask = loadingTask;
					var layer = await loadingTask;
					await layer.Lifecycle.__callOnCreate();
					status.State.SetMainFlag(LayerState.Loaded);
					return layer;
				}

				status.AppendTask(Load);
			}

			return status;
		}

		protected readonly Dictionary<string, LoadingLayerStatus> LoadingTasks = new();

		protected bool GetLoadingStatus(string uri, out LoadingLayerStatus status)
		{
			return LoadingTasks.TryGetValue(uri, out status);
		}

		protected bool GetOrCreateLoadingStatus(string uri, out LoadingLayerStatus status)
		{
			Debug.Assert(uri != null, "uri!=null");
			if (!LoadingTasks.TryGetValue(uri, out var result))
			{
				result = new(uri);
				LoadingTasks.Add(uri, result);
				status = result;
				return false;
			}
			else
			{
				status = result;
				return true;
			}
		}

		protected virtual async Task<ILayer> CreateLayerInstance(string uri, string resUri, Node layerRoot,
			LoadingLayerStatus status)
		{
			var node = await respool.MyNodePool.LoadAsync(resUri, layerRoot);
			var comp = node.GetComponent(GetLayerClass());
			Debug.Assert(comp is ILayer, "comp is ILayer");
			var result = (ILayer)comp;
			result.Uri = uri;
			result.ResUri = resUri;
			status.Layer = result;
			status.State.SetMainFlag(LayerState.Init);
			return result;
		}

		#endregion
	}
}