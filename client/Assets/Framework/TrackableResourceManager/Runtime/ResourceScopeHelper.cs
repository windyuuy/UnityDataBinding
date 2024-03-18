using System.Threading.Tasks;
using UnityEngine;

namespace TrackableResourceManager.Runtime
{
	public interface IWithResourceSet
	{
		ResourceSet Token { get; }
	}
	
	public static class ResourceScopeHelper
	{
		public static LoadAsyncOp<T> LoadAsync<T>(this IWithResourceSet wrap, ResourceKey resKey)
		{
			return wrap.Token.LoadAsync<T>(resKey);
		}

		public static Task Release<T>(this IWithResourceSet wrap, ResourceKey resKey)
		{
			return wrap.Token.Release<T>(resKey);
		}

		public static LoadAsyncOp<GameObject> InstantiateAsync(this IWithResourceSet wrap, ResourceKey resKey, Transform parent = null)
		{
			return wrap.Token.InstantiateAsync(resKey, parent);
		}

		public static LoadAsyncOp<GameObject> InstantiateAsync(this IWithResourceSet wrap, ResourceKey resKey, Transform parent, Vector3 position,
			Quaternion rotation)
		{
			return wrap.Token.InstantiateAsync(resKey, parent, position, rotation);
		}

		public static Task ReleaseInstance(this IWithResourceSet wrap, LoadAsyncOp<GameObject> op0)
		{
			return wrap.Token.ReleaseInstance(op0);
		}

	}
}