using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ResourceManager.Trackable.Runtime
{
	public interface IResourceLoader
	{
		public LoadAsyncOp<T> LoadAsync<T>(ResourceKey resKey);

		public Task Release<T>(ResourceKey resKey);

		public LoadAsyncOp<GameObject> InstantiateAsync(ResourceKey resKey, Transform parent = null);

		public LoadAsyncOp<GameObject> InstantiateAsync(ResourceKey resKey, Transform parent, Vector3 position,
			Quaternion rotation);

		public Task ReleaseInstance(LoadAsyncOp<GameObject> op0);
		//
		// public LoadAsyncOp<SceneInstance> LoadSceneAsync(ResourceKey resKey,
		// 	LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100);
		//
		// public LoadAsyncOp<SceneInstance> UnloadSceneAsync(LoadAsyncOp<SceneInstance> handle,
		// 	bool autoReleaseHandle = true);
		//
		// public LoadAsyncOp<SceneInstance> UnloadSceneAsync(ResourceKey resKey, bool autoReleaseHandle = true);
	}
}