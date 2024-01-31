using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace DataBinding.UIBind.RecycleContainer
{
	public class LocalNodeQueue
	{
		private Func<int, Transform> _creator;

		public LocalNodeQueue(Func<int, Transform> creator)
		{
			_creator = creator;
		}

		protected virtual Queue<Transform> Pool { get; } = new Queue<Transform>();

		public virtual Transform Get(int index)
		{
			if (Pool.Count > 0)
			{
				Profiler.BeginSample("Pool.First");
				var child2 = Pool.Dequeue();
				Profiler.EndSample();
				// Profiler.BeginSample("SetSiblingIndex1");
				// child2.SetSiblingIndex(index);
				// Profiler.EndSample();
				Profiler.BeginSample("gameObject.SetActive");
				if (!child2.gameObject.activeSelf)
				{
					child2.gameObject.SetActive(true);
				}
				Profiler.EndSample();

				return child2;
			}
			else
			{
				var child = _creator(index);
				if (!child.gameObject.activeSelf)
				{
					child.gameObject.SetActive(true);
				}

				return child;
			}
		}

		// public Transform PopRaw()
		// {
		// 	var child2 = Pool.First();
		// 	Pool.Remove(child2);
		// 	return child2;
		// }
		//
		// public Transform Peek()
		// {
		// 	return Pool.First();
		// }

		public virtual IEnumerable<Transform> PeekAll()
		{
			return Pool;
		}

		public virtual void Recycle(Transform node)
		{
			node.SetSiblingIndex(node.parent.childCount);
			Pool.Enqueue(node);
		}

		public virtual bool Contains(Transform node)
		{
			return Pool.Contains(node);
		}

		public virtual int Count => Pool.Count;
	}

	public class LocalNodePool
	{
		private Func<int, Transform> _creator;

		public LocalNodePool(Func<int, Transform> creator)
		{
			_creator = creator;
		}

		protected virtual HashSet<Transform> Pool { get; } = new HashSet<Transform>();

		public virtual Transform Get(int index)
		{
			if (Pool.Count > 0)
			{
				Profiler.BeginSample("Pool.First");
				var child2 = Pool.First();
				Pool.Remove(child2);
				Profiler.EndSample();
				Profiler.BeginSample("gameObject.SetActive");
				if (!child2.gameObject.activeSelf)
				{
					child2.gameObject.SetActive(true);
				}
				Profiler.EndSample();

				return child2;
			}
			else
			{
				var child = _creator(index);
				if (!child.gameObject.activeSelf)
				{
					child.gameObject.SetActive(true);
				}

				return child;
			}
		}

		// public Transform PopRaw()
		// {
		// 	var child2 = Pool.First();
		// 	Pool.Remove(child2);
		// 	return child2;
		// }
		//
		// public Transform Peek()
		// {
		// 	return Pool.First();
		// }

		public virtual IEnumerable<Transform> PeekAll()
		{
			return Pool;
		}

		public virtual void Recycle(Transform node)
		{
			node.SetSiblingIndex(node.parent.childCount);
			Pool.Add(node);
		}

		public virtual bool Contains(Transform node)
		{
			return Pool.Contains(node);
		}

		public virtual bool Remove(Transform node)
		{
			Debug.Assert(Pool.Count > 0, "Pool.Count > 0");

			return Pool.Remove(node);
		}

		public virtual int Count => Pool.Count;
	}

	/// <summary>
	/// 使用双端池，优化节点吞吐表现
	/// </summary>
	public class LocalNodeLinearPool : LocalNodePool
	{
		public LocalNodeLinearPool(Func<int, Transform> creator) : base(creator)
		{
		}

		protected readonly Stack<Transform> BackPool = new();

		public virtual void RecycleBack(Transform node)
		{
			node.SetSiblingIndex(node.parent.childCount);
			BackPool.Push(node);
		}

		public bool ContainsBack(Transform node)
		{
			return BackPool.Contains(node);
		}

		public override Transform Get(int index)
		{
			if (BackPool.Count > 0)
			{
				var child2 = BackPool.Pop();
				if (!child2.gameObject.activeSelf)
				{
					child2.gameObject.SetActive(true);
				}

				return child2;
			}

			if (Pool.Count > 0)
			{
				return base.Get(index);
			}

			return base.Get(index);
		}

		public override IEnumerable<Transform> PeekAll()
		{
			foreach (var transform in BackPool)
			{
				yield return transform;
			}
			
			foreach (var transform in Pool)
			{
				yield return transform;
			}
		}

		public override int Count => Pool.Count + BackPool.Count;
	}
}