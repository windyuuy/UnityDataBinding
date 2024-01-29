using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

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

		protected System.Collections.Generic.List<Transform> unallocNodes;

		protected System.Collections.Generic.List<(int, Vector3)> PendingResetPostion = new();

		protected void AddPendingResetLocationAction(int index, Vector3 localPosition)
		{
			PendingResetPostion.Add((index, localPosition));
		}

		protected virtual void ApplyAllResetPosition()
		{
			if (PendingResetPostion.Count > 0)
			{
				foreach (var (index, vector3) in PendingResetPostion)
				{
					var child = _container.GetChild(index);
					var localPosition = vector3;
					ApplyResetPosition(child, index, ref localPosition);
				}
				PendingResetPostion.Clear();
			}
		}

		protected virtual void ClearPendingResetPosition()
		{
			PendingResetPostion.Clear();
		}

		protected virtual void ApplyResetPosition(Transform child, int index, ref Vector3 localPosition)
		{
			child.localPosition = localPosition;
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

		protected int LayoutDirtyFrameCount = 0;
		public virtual void MarkLayoutDirty()
		{
			this.LayoutDirtyFrameCount = Time.frameCount;
		}

		public virtual bool IsLayoutDirty()
		{
			return LayoutDirtyFrameCount == Time.renderedFrameCount;
		}

		protected List<object> OldList;
		protected List<object> TempList;
		protected static readonly List<object> EmptyList = new();

		protected Coroutine CoUpdateItemsByTicks;
		protected virtual void OnDataChanged(System.Collections.IList dataSources)
		{
			if (CoUpdateItemsByTicks != null)
			{
				StopCoroutine(CoUpdateItemsByTicks);
				CoUpdateItemsByTicks = null;
			}
			CoUpdateItemsByTicks = StartCoroutine(UpdateItemsByTicks(dataSources));
		}

		protected virtual IEnumerator UpdateItemsByTicks(IList dataSources)
		{
			yield return UpdateItemsByData(dataSources);
			yield return null;
		}

		protected bool IsUpdatingItems = false;
		protected virtual IEnumerator UpdateItemsByData(IList dataSources)
		{
			IsUpdatingItems = true;
			
			if (OldList == null || OldList.Count == 0)
			{
				if (dataSources==null || dataSources.Count == 0)
				{
					// return;
					yield break;
				}
				else
				{
					OldList = new List<object>(dataSources.Count);
					TempList = new List<object>(dataSources.Count);
				}
			}
			
			if (dataSources == null)
			{
				dataSources = EmptyList;
			}

			var dataSourcesCount = dataSources.Count;
			var oldListCount = OldList.Count;
			
			TempList.Clear();
			TempList.AddRange(OldList);

			// yield return null;
			Profiler.BeginSample("AddPendingResetLocationAction");
			// delay update locations
			// if (OldList.Count < dataSources.Count)
			ClearPendingResetPosition();
			{
				var i = 0;
				for (; i < Math.Min(dataSourcesCount, OldList.Count); i++)
				{
					if (OldList[i] != dataSources[i])
					{
						var child = _container.GetChild(i);
						this.AddPendingResetLocationAction(i, child.localPosition);
					}
				}

				for (; i < dataSourcesCount; i++)
				{
					var inVisiblePos = float.MaxValue * 0.5f;
					this.AddPendingResetLocationAction(i, new Vector3(inVisiblePos,inVisiblePos,0));
				}
			}
			Profiler.EndSample();
			yield return null;

			Profiler.BeginSample("UpdateDataItems");
			// remove first
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

			for (var i = 0; i < dataSourcesCount; i++)
			{
				// if (i % 200 == 0)
				// {
				// 	yield return null;
				// }
				
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
						OldList.Insert(i, dataSource);
						this.OnAddItem(dataSource, i);
					}
					else
					{
						// move is usually fast, 暂时不考虑最优解
						OldList.RemoveAt(oldIndex);
						OldList.Insert(i, dataSource);
						var isIndexMoved = TempList.Count <= i || TempList[i] != dataSource;
						this.OnMoveItem(dataSource, i, oldIndex, isIndexMoved);
					}
				}
				else
				{
					var dataSource = dataSources[i];
					// same
					var isIndexMoved = TempList.Count <= i || TempList[i] != dataSource;
					if (isIndexMoved)
					{
						this.OnMoveItem(dataSource, i, i, isIndexMoved);
					}
				}
			}

			// remove duplicate at end
			for (int i = OldList.Count - 1; i >= dataSourcesCount; i--)
			{
				var oldData = OldList[i];
				OldList.RemoveAt(i);
				this.OnRemoveItem(oldData, i);
			}
			Profiler.EndSample();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
			Profiler.BeginSample("CheckResult");
			// check result
			Debug.Assert(OldList.Count == dataSourcesCount,
				$"result length unmatched: old({OldList.Count})!=new({dataSourcesCount})");
			for (var i = 0; i < Math.Min(OldList.Count, dataSources.Count); i++)
			{
				Debug.Assert(OldList[i] == dataSources[i], $"result unmatched: {i}");
			}
			Profiler.EndSample();
#endif
			// yield return null;
			this.OnUpdateDone(dataSources, oldListCount, TempList);
			
			TempList.Clear();

			if (oldListCount < dataSourcesCount)
			{
				this.MarkLayoutDirty();
			}
			
			IsUpdatingItems = false;
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

			child.SetSiblingIndex(index);
			return child;
		}

		protected virtual void OnAddItem(object dataSource, int index)
		{
			Profiler.BeginSample("OnAddItem");
			var child = this.RequireNewNode(index);
			Profiler.EndSample();
			Profiler.BeginSample("OnAddItem-UpdateDataBind");
			UpdateDataBind(child, dataSource, index);
			Profiler.EndSample();
		}

		protected virtual void OnMoveItem(object oldData, int newIndex, int oldIndex, bool isIndexMoved)
		{
			var child = _container.GetChild(oldIndex);

			OnMoveNode(child, newIndex, oldIndex, isIndexMoved);

			if (isIndexMoved)
			{
				UpdateDataBind(child, oldData, newIndex);
			}
		}

		protected virtual void OnMoveNode(Transform child, int newIndex, int oldIndex, bool isIndexMoved)
		{
			if (oldIndex != newIndex)
			{
				child.SetSiblingIndex(newIndex);
			}
		}

		protected virtual void OnRemoveItem(object oldData, int i)
		{
			var childCount = _container.childCount;
			Debug.Assert(childCount > i);
			var child = _container.GetChild(i);

			var ccItem = child.GetComponent<CCContainerItem>();
			if (ccItem != null)
			{
				ccItem.UnsetDataHost();
			}

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
				child.SetParent(null);
#endif
				child.gameObject.DestorySafe();
			}
		}

		protected virtual void OnUpdateDone(IList dataSources, int oldListCount, System.Collections.Generic.List<object> oldList)
		{
			Profiler.BeginSample("UnuseNode");
			if (unallocNodes != null)
			{
				foreach (var unallocNode in unallocNodes)
				{
					this.UnuseNode(unallocNode);
				}

				unallocNodes = null;
			}

			var childCount = _container.childCount;
			for (var i = childCount - 1; i >= dataSources.Count; i--)
			{
				var child = _container.GetChild(i);
				this.UnuseNode(child);
			}

			if (TemplateNode == null || TemplateNode.GetSiblingIndex() >= dataSources.Count)
			{
				TemplateNode = null;
			}
			Profiler.EndSample();
			
			Profiler.BeginSample("ApplyAllResetPosition");
			ApplyAllResetPosition();
			Profiler.EndSample();
		}
	}
}