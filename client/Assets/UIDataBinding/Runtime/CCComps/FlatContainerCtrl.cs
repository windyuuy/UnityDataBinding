using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataBinding.UIBind
{
	[RequireComponent(typeof(CCContainerBind))]
	public class FlatContainerCtrl : MonoBehaviour
	{
		/**
		 * 如果子节点没有添加 DialogChild, 那么强制为所有子节点添加 DialogChild
		 */
		[Rename("自动收容子节点")] public bool bindChildren = true;

		[SerializeField] protected RectTransform _container = null;

		protected List<Transform> unallocNodes;

		protected List<(Transform, int, Vector3)> PendingResetPostion = new();

		protected Transform AddPendingResetLocationAction(Transform child, int index, Vector3 localPosition)
		{
			PendingResetPostion.Add((child, index, localPosition));
			return child;
		}
		protected Transform AddPendingResetLocationAction(Transform child, int index)
		{
			if (index < _container.childCount)
			{
				var  localPosition = _container.GetChild(index).localPosition;
				AddPendingResetLocationAction(child, index, localPosition);
			}
			return child;
		}

		protected void ApplyResetPosition()
		{
			if (PendingResetPostion.Count > 0)
			{
				foreach (var item in PendingResetPostion)
				{
					item.Item1.localPosition = item.Item3;
					// item.Item1.SetSiblingIndex(item.Item2);
				}
				PendingResetPostion.Clear();
			}
		}

		protected virtual void Awake()
		{
		}

		protected virtual void InitContainer()
		{
			if (this._container == null)
			{
				this._container = (RectTransform)this.transform;
			}

			Debug.Assert(this._container != null, "this._container!=null");

			if (this._container != null)
			{
				if (this.bindChildren)
				{
					this._container.ForEachChildren(child =>
					{
						var ccDataHost = child.GetOrAddComponent<CCDataHost>();
						ccDataHost.Integrate();
					});
				}

				unallocNodes = this._container.GetChildren().ToList();
			}
		}

		protected List<object> OldList;

		protected virtual void OnDataChanged(System.Collections.IList dataSources)
		{
			if (OldList == null)
			{
				if (dataSources.Count == 0)
				{
					return;
				}
				else
				{
					OldList = new List<object>(dataSources.Count);
				}
			}

			for (var i = OldList.Count - 1; i >= 0; i--)
			{
				var oldData = OldList[i];

				var newIndex = dataSources.IndexOf(oldData);
				if (newIndex < 0)
				{
					OldList.RemoveAt(i);
					this.OnRemoveItem(oldData, i);
				}
			 }

			for (var i = 0; i < dataSources.Count; i++)
			{
				if (OldList.Count <= i)
				{
					OldList.Add(dataSources[i]);
					this.OnAddItem(dataSources[i], i);
				}
				else if (OldList[i] != dataSources[i])
				{
					var dataSource = dataSources[i];
					var oldIndex = -1;
					for (var j = i + 1; j < OldList.Count; j++)
					{
						if (OldList[j] == dataSource)
						{
							oldIndex = j;
							break;
						}
					}

					if (oldIndex < 0)
					{
						OldList.Insert(i, dataSources[i]);
						this.OnAddItem(dataSources[i], i);
					}
					else
					{
						// move is usually fast, 暂时不考虑最优解
						OldList.RemoveAt(oldIndex);
						OldList.Insert(i, dataSources[i]);
						this.OnMoveItem(dataSources[i], i, oldIndex);
					}
				}
				else
				{
					// same
					UpdateDataBind(_container.GetChild(i), dataSources[i], i);
				}
			}

			for (int i = OldList.Count - 1; i >= dataSources.Count; i--)
			{
				OldList.RemoveAt(i);
				this.OnRemoveItem(OldList[i], i);
			}

#if DEVELOPMENT_BUILD || UNITY_EDITOR
			// check result
			Debug.Assert(OldList.Count == dataSources.Count,
				$"result length unmatched: old({OldList.Count})!=new({dataSources.Count})");
			for (var i = 0; i < OldList.Count; i++)
			{
				Debug.Assert(OldList[i] == dataSources[i], $"result unmatched: {i}");
			}
#endif

			this.OnUpdateDone(dataSources, this.OldList.Count);
		}

		protected virtual Transform GetTemplateNode(int index)
		{
			if (TemplateNode == null)
			{
				if (_container.childCount > 0)
				{
					TemplateNode = _container.GetChild(0);
					return TemplateNode;
				}
			}

			return TemplateNode;
		}

		protected Transform TemplateNode;
		protected IEnumerable<CCContainerItem> ItemHubIter;

		public virtual Transform CreateNewNode(int index)
		{
			var tempNode = GetTemplateNode(index);
			if (tempNode != null)
			{
				var child = GameObject.Instantiate(tempNode.gameObject, _container).transform;
				child.gameObject.name = tempNode.gameObject.name;
				return child;
			}

			return null;
		}

		protected virtual void UpdateCurrentDataBind(Transform child, int index)
		{
			UpdateDataBind(child, OldList[index], index);
		}

		protected virtual void PrepareDataBind(Transform child)
		{
			var ccItem = child.GetComponent<CCContainerItem>();
			if (ccItem == null)
			{
				ccItem = child.gameObject.AddComponent<CCContainerItem>();
				ccItem.Integrate();
			}
		}

		protected virtual void UpdateDataBind(Transform child, object dataSource, int index)
		{
			var ccItem = child.GetComponent<CCContainerItem>();
			if (ccItem == null)
			{
				ccItem = child.gameObject.AddComponent<CCContainerItem>();
				ccItem.Integrate();
			}

			if (ccItem.DataHost != dataSource || ccItem.ContainerItem.Index != index)
			{
				ccItem.ContainerItem.Index = index;
				var itemHost = dataSource;
				var itemHost1 = VM.Utils.ImplementStdHost(itemHost);
				ccItem.BindDataHost(itemHost1, $"N|{ccItem.ContainerItem.Index}");
			}
		}

		public virtual Transform RequireNewNode(int index)
		{
			Transform child;
			if (unallocNodes != null && unallocNodes.Count > 0)
			{
				// child = GetTemplateNode(index);
				child = unallocNodes[0];
				// TemplateNode = child;
				unallocNodes.RemoveAt(0);
			}
			else
			{
				child = CreateNewNode(index);
			}

			AddPendingResetLocationAction(child, index);
			child.SetSiblingIndex(index);
			return child;
		}

		protected virtual void OnAddItem(object dataSource, int index)
		{
			var child = this.RequireNewNode(index);

			UpdateDataBind(child, dataSource, index);
		}

		protected virtual void OnMoveItem(object oldData, int newIndex, int oldIndex)
		{
			var child = _container.GetChild(oldIndex);
			AddPendingResetLocationAction(child, newIndex);
			child.SetSiblingIndex(newIndex);

			UpdateDataBind(child, oldData, newIndex);
		}

		protected virtual void OnRemoveItem(object oldData, int i)
		{
			var childCount = _container.childCount;
			Debug.Assert(childCount > i);
			var child = _container.GetChild(i);

			// var ccItem = child.GetComponent<CCContainerItem>();
			// if (ccItem != null)
			// {
			// 	ccItem.BindDataHost(null, $"N|{ccItem.ContainerItem.Index}");
			// }

			RecycleNode(child);
		}

		protected virtual void RecycleNode(Transform child)
		{
			child.SetSiblingIndex(child.parent.childCount);
		}

		protected virtual void UnuseNode(Transform child)
		{
			if (_container.childCount == 1)
			{
				TemplateNode = child;
			}
			else
			{
#if UNITY_EDITOR
				child.parent = null;
#endif
				child.gameObject.DestorySafe();
			}
		}

		protected virtual void OnUpdateDone(System.Collections.IList dataSources, int oldListCount)
		{
			ApplyResetPosition();
			
			if (unallocNodes != null)
			{
				foreach (var unallocNode in unallocNodes)
				{
					this.UnuseNode(unallocNode);
				}

				unallocNodes = null;
			}

			var childCount = _container.childCount;
			for (var i = childCount - 1; i >= oldListCount; i--)
			{
				var child = _container.GetChild(i);
				this.UnuseNode(child);
			}

			if (TemplateNode == null || TemplateNode.GetSiblingIndex() >= oldListCount)
			{
				TemplateNode = null;
			}
		}
	}
}