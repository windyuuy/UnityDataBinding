using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace gcc.layer
{
	public class LayerBundleManager
	{
		public class TLayerBundleRefer
		{
			public Func<LayerBundle> GetLayerBundle = () => null;
		}

		public readonly TLayerBundleRefer LayerBundleRefer;

		protected readonly Stack<LayerBundle> LayerBundleStack;

		public LayerBundleManager()
		{
			LayerBundleStack = new();
			LayerBundleRefer = new();
			LayerBundleRefer.GetLayerBundle = () => LayerBundleStack.Peek();
		}

		public Task PushAndOpen(LayerBundle layerBundle)
		{
			LayerBundleStack.Push(layerBundle);
			return layerBundle.Open();
		}

		public Task PopAndClose()
		{
			if (LayerBundleStack.Count > 0)
			{
				var layerBundle = LayerBundleStack.Pop();
				return layerBundle.Close();
			}
			else
			{
				Debug.LogException(new InvalidOperationException("Stack empty"));
				return Task.CompletedTask;
			}
		}

		public Task ReplaceAndOpen(LayerBundle layerBundle)
		{
			return Task.WhenAll(
				PopAndClose(),
				PushAndOpen(layerBundle));
		}

		public Task PopAndCloseTo(LayerBundle layerBundle)
		{
			var exist = LayerBundleStack.Any(bundle => bundle == layerBundle);
			if (!exist)
			{
				Debug.LogError($"nolayerbundle: {layerBundle}");
				return Task.CompletedTask;
			}

			var tasks = Enumerable.Empty<Task>();
			while (LayerBundleStack.Peek() != layerBundle)
			{
				tasks = tasks.Append(PopAndClose());
			}

			return Task.WhenAll(tasks);
		}
	}
}