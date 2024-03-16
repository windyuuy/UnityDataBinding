using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UISys.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace gcc.layer
{
	[AddComponentMenu("UISys/TransformOrder")]
	[DisallowMultipleComponent]
	public class TransformOrderComp : MonoBehaviour
	{
		[SerializeField] protected TagOrdersConfig config;

		/// <summary>
		/// 只排序active的节点m
		/// </summary>
		[SerializeField] protected bool sortInactive = false;

		public List<string> MainOrders => config.mainOrders;
		public List<string> DependOrders => config.dependOrders;

		public LayerOrderManager LayerOrderManager = new();

		public void OnEnable()
		{
			SetupOrders();
		}

		public void SetupOrders()
		{
			var (mainOrders0, dependOrders0) = config.LoadAll();

			if (mainOrders0.Length > 0)
			{
				foreach (var mainOrder in mainOrders0)
				{
					var orders = mainOrder.Split('<');
					LayerOrderManager.SetupTagOrders(orders);
				}
			}

			if (dependOrders0.Length > 0)
			{
				foreach (var dependOrder in dependOrders0)
				{
					// TODO: 优化性能
					var orders = dependOrder.Split('>');
					var deps = orders[1].Split(',');
					LayerOrderManager.SetTagOrders(orders[0], deps);
				}
			}
		}

		private bool _dirty = false;

		private void OnTransformChildrenChanged()
		{
			SetDirty();
		}

		private void SetDirty()
		{
			if (!this.enabled)
			{
				return;
			}
			
			if (!_dirty)
			{
				_dirty = true;

				ScheduleOnceReSort();
			}
		}

		private void OnTransformChildrenActive()
		{
			SetDirty();
		}

		private void OnTransformChildrenTagsChanged()
		{
			SetDirty();
		}

		protected static readonly List<(string[] Tags, Transform transform, int i)> Nodes = new();

		protected void ScheduleOnceReSort()
		{
			StartCoroutine(DelayReSort());
		}

		protected IEnumerator DelayReSort()
		{
			yield return new WaitForEndOfFrame();
			ReSortOrder();
		}

		public void ReSortOrder()
		{
			if (!this.enabled)
			{
				return;
			}
			
			if (_dirty)
			{
				if (Nodes.Count > 0)
				{
					Nodes.Clear();
				}

				for (var i = 0; i < this.transform.childCount; i++)
				{
					var child = transform.GetChild(i);
					var tagsComp = child.GetComponent<TransformTagsComp>();
					if (tagsComp != null && (sortInactive || tagsComp.isActiveAndEnabled))
					{
						Nodes.Add((tagsComp.Tags, tagsComp.transform, i));
					}
				}

				var layersCount = Nodes.Count;
				if (layersCount > 0)
				{
					if (layersCount >= 2)
					{
						// var names = Layers.Select(layer => layer.name).ToArray();
						var orders = Nodes.Select(layer => layer.i).ToArray();
						Nodes.Sort((a, b) =>
						{
							var aO = LayerOrderManager.GetOrder(a.Tags);
							var bO = LayerOrderManager.GetOrder(b.Tags);
							if (aO == bO)
							{
								var aI = a.i;
								var bI = b.i;
								return aI - bI;
							}
							else
							{
								return bO - aO;
							}
						});

						// Debug.Log(
						// 	$"Sort: {string.Concat(names)} -> {string.Concat(Layers.Select(l => l.transform.name))}");

						// var index0 = Layers[0].transform.GetSiblingIndex();
						// for (var i = 1; i < Layers.Count; i++)
						// {
						// 	if (Layers[i - 1].transform.GetSiblingIndex() < Layers[i].transform.GetSiblingIndex())
						// 	{
						// 		Layers[i - 1].transform.SetSiblingIndex(Layers[i].transform.GetSiblingIndex());
						// 	}
						// }
						//
						//
						// Layers[^1].transform.SetSiblingIndex(index0);

						// for (var i = Layers.Count-1; i >=0 ; i--)
						for (var i = 0; i < layersCount; i++)
						{
							var child = Nodes[i].transform;
							if (child.GetSiblingIndex() != orders[i])
							{
								child.SetSiblingIndex(orders[i]);
							}
						}
					}

					Nodes.Clear();
				}

				_dirty = false;

				this.SendMessage("OnTransformChildrenReSort", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}