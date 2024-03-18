﻿using System;
using System.Collections;

namespace TrackableResourceManager.Runtime
{
	public readonly struct DelayResourceScope : IDisposable
	{
		private readonly ResourceSet _resourceSet;

		public DelayResourceScope(ResourceSet set = null)
		{
			_resourceSet = set ?? new();
		}

		private void DelayDispose(IEnumerator delayer = null)
		{
			LoomMG.SharedLoom.StartCoroutine(DelayDisposeCo(delayer));
		}

		private IEnumerator DelayDisposeCo(IEnumerator delayer)
		{
			yield return delayer;
			_resourceSet.Dispose();
		}

		public void Dispose()
		{
			DelayDispose();
		}
	}
}