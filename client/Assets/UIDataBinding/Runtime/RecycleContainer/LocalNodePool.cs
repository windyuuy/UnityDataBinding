using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIDataBinding.Runtime.RecycleContainer
{
	public class LocalNodePool
	{
		private Func<int, Transform> _creator;
		public LocalNodePool(Func<int, Transform> creator)
		{
			_creator = creator;
		}
		protected HashSet<Transform> Pool = new();

		public Transform Get(int index)
		{
			if (Pool.Count > 0)
			{
				var child2 = Pool.First();
				Pool.Remove(child2);
				if (!child2.gameObject.activeSelf)
				{
					child2.gameObject.SetActive(true);
				}
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

		public Transform PopRaw()
		{
			var child2 = Pool.First();
			Pool.Remove(child2);
			return child2;
		}

		public Transform Peek()
		{
			return Pool.First();
		}

		public HashSet<Transform> PeekAll()
		{
			return Pool;
		}

		public void Recycle(Transform node)
		{
			node.SetSiblingIndex(node.parent.childCount);
			Pool.Add(node);
		}

		public bool Exist(Transform node)
		{
			return Pool.Contains(node);
		}

		public void Remove(Transform node)
		{
			Pool.Remove(node);
		}

		public int Count => Pool.Count;
	}
}