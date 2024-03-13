using System;
using System.Collections;
using UnityEngine;

namespace TrackableResourceManager.Runtime
{
	public readonly struct ResourceScope : IDisposable
	{
		private readonly ResourceSet _resourceSet;
		
		public static ResourceScope New => new ResourceScope(null);

		private ResourceScope(ResourceSet set = null)
		{
			_resourceSet = set ?? new();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			DelayCheck();
#endif
		}

		public void DelayCheck()
		{
			LoomMG.SharedLoom.StartCoroutine(DelayCheckCo());
		}

		private IEnumerator DelayCheckCo()
		{
			yield return new WaitForEndOfFrame();
			Debug.Assert(_resourceSet.IsDisposed, "_resourceSet.IsDisposed");
		}

		public void Dispose()
		{
			_resourceSet.Dispose();
		}
	}
}