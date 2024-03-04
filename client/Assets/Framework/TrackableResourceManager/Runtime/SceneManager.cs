using System.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ResourceManager.Trackable.Runtime
{
	public class SceneManager
	{
		private readonly ResourceSet _resourceSet = new();


		public LoadAsyncOp<SceneInstance> LoadSceneAsync(ResourceKey key,
			LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			return _resourceSet.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
		}

		public LoadAsyncOp<SceneInstance> UnloadSceneAsync(LoadAsyncOp<SceneInstance> handle,
			bool autoReleaseHandle = true)
		{
			return _resourceSet.UnloadSceneAsync(handle, autoReleaseHandle);
		}

		protected LoadAsyncOp<SceneInstance> UnloadSceneAsync(ResourceKey key,
			bool autoReleaseHandle = true)
		{
			return _resourceSet.UnloadSceneAsync(key, autoReleaseHandle);
		}
	}
}