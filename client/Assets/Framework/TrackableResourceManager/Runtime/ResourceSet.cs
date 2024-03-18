using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace TrackableResourceManager.Runtime
{
	public class ResourceSet : IDisposable, IResourceLoader
	{
		protected struct CacheKey
		{
			public readonly string Key;
			public readonly Type T;

			public CacheKey(string cacheKey, Type t)
			{
				Key = cacheKey;
				T = t;
			}

			public override int GetHashCode()
			{
				return Key.GetHashCode() ^ T.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				if (obj is CacheKey cacheKey)
					return this.Key == cacheKey.Key && this.T == cacheKey.T;
				return false;
			}
		}

		protected readonly Dictionary<CacheKey, IEnumerator> Cached = new();
		protected readonly List<IEnumerator> CachedInstance = new();

		public bool IsAllLoaded()
		{
			foreach (var keyValuePair in Cached)
			{
				if (keyValuePair.Value.MoveNext())
				{
					return false;
				}
			}
			
			foreach (var enumerator in CachedInstance)
			{
				if (enumerator.MoveNext())
				{
					return false;
				}
			}

			return true;
		}

		public async Task<string> LoadKeyUri(ResourceKey key)
		{
			return key.Key;
		}

		private static LoadAsyncOp<T> WrapLoadOp<T>(Func<Task<AsyncOperationHandle<T>>> loadOp)
		{
			LoadAsyncOp<T> op0 = new();

			async Task<T> Load()
			{
				var task = loadOp();
				op0.LoadOpTask = task;
				var op = await task;
				var opResult = await op.Task;
				return opResult;
			}

			op0.Task = Load();
			return op0;
		}

		#region with key

		public LoadAsyncOp<T> LoadAsync<T>(ResourceKey resKey)
		{
			async Task<AsyncOperationHandle<T>> LoadOp()
			{
				var uri = await LoadKeyUri(resKey);
				var op = LoadAsync<T>(uri);
				return op;
			}

			var op0 = WrapLoadOp(LoadOp);
			return op0;
		}

		public async Task Release<T>(ResourceKey resKey)
		{
			var uri = await LoadKeyUri(resKey);
			Release<T>(uri);
		}

		public LoadAsyncOp<GameObject> InstantiateAsync(ResourceKey resKey, Transform parent = null)
		{
			async Task<AsyncOperationHandle<GameObject>> LoadOp()
			{
				var uri = await LoadKeyUri(resKey);
				var op = InstantiateAsync(uri, parent);
				return op;
			}

			var op0 = WrapLoadOp(LoadOp);
			return op0;
		}

		public LoadAsyncOp<GameObject> InstantiateAsync(ResourceKey resKey, Transform parent, Vector3 position,
			Quaternion rotation)
		{
			async Task<AsyncOperationHandle<GameObject>> LoadOp()
			{
				var uri = await LoadKeyUri(resKey);
				var op = InstantiateAsync(uri, parent, position, rotation);
				return op;
			}

			var op0 = WrapLoadOp(LoadOp);
			return op0;
		}

		public async Task ReleaseInstance(LoadAsyncOp<GameObject> op0)
		{
			var op = await op0.LoadOpTask;
			ReleaseInstance(op);
		}

		#endregion

		#region scene manager
		
		internal LoadAsyncOp<SceneInstance> LoadSceneAsync(ResourceKey resKey,
			LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			async Task<AsyncOperationHandle<SceneInstance>> LoadOp()
			{
				var uri = await LoadKeyUri(resKey);
				var op = LoadSceneAsync(uri, loadMode, activateOnLoad, priority);
				return op;
			}

			var op0 = WrapLoadOp(LoadOp);
			return op0;
		}

		internal LoadAsyncOp<SceneInstance> UnloadSceneAsync(LoadAsyncOp<SceneInstance> handle,
			bool autoReleaseHandle = true)
		{
			async Task<AsyncOperationHandle<SceneInstance>> LoadOp()
			{
				var op1 = await handle.LoadOpTask;
				var op = UnloadSceneAsync(op1, autoReleaseHandle);
				return op;
			}

			var op0 = WrapLoadOp(LoadOp);
			return op0;
		}

		internal LoadAsyncOp<SceneInstance> UnloadSceneAsync(ResourceKey resKey,
			bool autoReleaseHandle = true)
		{
			async Task<AsyncOperationHandle<SceneInstance>> LoadOp()
			{
				var uri = await LoadKeyUri(resKey);
				var op = UnloadSceneAsync(uri, autoReleaseHandle);
				return op;
			}

			var op0 = WrapLoadOp(LoadOp);
			return op0;
		}

		#endregion

		#region with string

		protected AsyncOperationHandle<T> LoadAsync<T>(string resUri)
		{
			var key = new CacheKey(resUri, typeof(T));
			if (!Cached.TryGetValue(key, out var op))
			{
				op = Addressables.LoadAssetAsync<T>(resUri);
				Cached.Add(key, op);
			}

			return (AsyncOperationHandle<T>)op;
		}

		protected void Release<T>(string resUri)
		{
			var key = new CacheKey(resUri, typeof(T));
			if (Cached.Remove(key, out var op))
			{
				Addressables.Release(op);
			}
		}

		protected void Release<T>(AsyncOperationHandle<T> op)
		{
			bool isFind = false;
			CacheKey key = new();
			foreach (var keyValuePair in Cached)
			{
				if (op.Equals(keyValuePair.Value))
				{
					isFind = true;
					key = keyValuePair.Key;
					break;
				}
			}

			if (isFind)
			{
				Cached.Remove(key);
				Addressables.Release(op);
			}
		}

		protected AsyncOperationHandle<GameObject> InstantiateAsync(string resUri, Transform parent = null)
		{
			var op = Addressables.InstantiateAsync(resUri, parent);
			CachedInstance.Add(op);
			return op;
		}

		protected AsyncOperationHandle<GameObject> InstantiateAsync(string resUri, Transform parent,
			Vector3 position,
			Quaternion rotation)
		{
			var op = Addressables.InstantiateAsync(resUri, position, rotation, parent);
			CachedInstance.Add(op);
			return op;
		}

		protected void ReleaseInstance(AsyncOperationHandle<GameObject> op)
		{
			CachedInstance.Remove(op);
			Addressables.ReleaseInstance(op);
		}

		protected AsyncOperationHandle<SceneInstance> LoadSceneAsync(string resUri,
			LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			var key = new CacheKey(resUri, typeof(SceneInstance));
			if (!Cached.TryGetValue(key, out var op))
			{
				op = Addressables.LoadSceneAsync(resUri, loadMode, activateOnLoad, priority);
				Cached.Add(key, op);
			}

			return (AsyncOperationHandle<SceneInstance>)op;
		}

		protected AsyncOperationHandle<SceneInstance> UnloadSceneAsync(string resUri,
			bool autoReleaseHandle = true)
		{
			var key = new CacheKey(resUri, typeof(SceneInstance));
			if (Cached.Remove(key, out var op))
			{
				return Addressables.UnloadSceneAsync((AsyncOperationHandle<SceneInstance>)op, autoReleaseHandle);
			}
			else
			{
				throw new InvalidOperationException($"cannot UnloadSceneAsync not exist: {key}");
			}
		}

		protected AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> handle,
			bool autoReleaseHandle = true)
		{
			bool isFind = false;
			CacheKey key = new();
			foreach (var keyValuePair in Cached)
			{
				if (handle.Equals(keyValuePair.Value))
				{
					isFind = true;
					key = keyValuePair.Key;
					break;
				}
			}

			if (isFind)
			{
				Cached.Remove(key);
				return Addressables.UnloadSceneAsync(handle, autoReleaseHandle);
			}
			else
			{
				throw new InvalidOperationException($"cannot UnloadSceneAsync not exist: {key}");
			}
		}

		#endregion

		private bool _isDisposed = false;
		public bool IsDisposed => _isDisposed;

		public void Dispose()
		{
			if (_isDisposed)
			{
				return;
			}

			_isDisposed = true;

			foreach (var asyncOperationHandle in this.CachedInstance)
			{
				Addressables.ReleaseInstance((AsyncOperationHandle<GameObject>)asyncOperationHandle);
			}

			CachedInstance.Clear();

			foreach (var keyValuePair in this.Cached)
			{
				Addressables.Release(keyValuePair.Value.Current);
			}

			Cached.Clear();

		}

		~ResourceSet()
		{
			Dispose();
		}
	}
}