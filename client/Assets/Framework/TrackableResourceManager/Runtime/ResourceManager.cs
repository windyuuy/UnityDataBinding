using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ResourceManager.Trackable.Runtime
{
	public class ModuleResourceAccessKeys
	{
		public static readonly ResourceKey CheckInBgImage = new ResourceKey("@eeeeeeeeee:CheckInBgImage");
	}

	internal class ResourceManager
	{
		public static readonly ResourceManager Inst = new();

		public Task<T> LoadAsync<T>(string resUri)
		{
			return Addressables.LoadAssetAsync<T>(resUri).Task;
		}

		public void Unload<T>(T obj)
		{
			Addressables.Release<T>(obj);
		}

		public Task<GameObject> InstantiateAsync<T>(string resUri)
		{
			return Addressables.InstantiateAsync(resUri).Task;
		}

		public void ReleaseInstance(GameObject obj)
		{
			Addressables.ReleaseInstance(obj);
		}
	}

	public struct LoadAsyncOp<T>
	{
		internal Task<AsyncOperationHandle<T>> LoadOpTask { get; set; }

		public Task<T> Task { get; internal set; }
	}
}