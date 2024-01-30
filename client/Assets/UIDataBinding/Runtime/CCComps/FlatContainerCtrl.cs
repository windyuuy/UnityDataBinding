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

		protected List<Transform> unallocNodes;

		protected List<(int, Vector3)> PendingResetPostion = new();

		protected void AddPendingResetLocationAction(int index, Vector3 localPosition)
		{
			PendingResetPostion.Add((index, localPosition));
		}

		protected virtual void ApplyAllResetPosition()
		{
			Profiler.BeginSample($"ApplyAllResetPosition: {PendingResetPostion.Count}");
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
			Profiler.EndSample();
		}

		protected virtual void ClearPendingResetPosition()
		{
			PendingResetPostion.Clear();
		}

		protected virtual void ApplyResetPosition(Transform child, int index, ref Vector3 localPosition)
		{
			if (child.localPosition != localPosition)
			{
				child.localPosition = localPosition;
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
			foreach (var o in UpdateItemsByData(dataSources, 0, Math.Max(OldList?.Count??0, dataSources.Count), 0, dataSources.Count))
			{
				
			}
			yield return null;
		}

		protected bool IsUpdatingItems = false;
		private static readonly float InVisiblePosV = Mathf.Floor(float.MaxValue * 0.5f);
		protected static Vector3 InVisiblePos = new Vector3(InVisiblePosV,InVisiblePosV,0);
		protected virtual IEnumerable UpdateItemsByData(IList dataSources, int oldStart, int oldEnd, int start, int end)
		{
			IsUpdatingItems = true;


			if ((OldList == null || OldList.Count >= end)
			    && (start >= end || dataSources == null || dataSources.Count <= start))
			{
				yield break;
			}

			if (dataSources == null)
			{
				dataSources = EmptyList;
			}

			if (OldList == null)
			{
				OldList = new List<object>(dataSources.Count);
				TempList = new List<object>(dataSources.Count);
			}

			var dataSourcesStart = Math.Min(dataSources.Count, start);
			var dataSourcesEnd = Math.Min(dataSources.Count, end);
			var dataSourcesCount = dataSourcesEnd - dataSourcesStart;

			Debug.Assert(OldList.Count >= oldStart, "OldList.Count > oldStart");
			Debug.Assert(oldEnd >= oldStart, "oldEnd > oldStart");
			// Debug.Assert(OldList.Count >= oldEnd, "OldList.Count > oldEnd");
			var oldListStart0 = oldStart;
			var oldListEnd0 = Math.Min(oldEnd, OldList.Count);
			var oldListCount0 = oldListEnd0 - oldListStart0;
			var oldListEnd = oldListEnd0;
			var oldListLen0 = OldList.Count;
			Debug.Assert(oldListStart0 == dataSourcesStart, "oldListStart == start");
			if (oldListStart0 != dataSourcesStart)
			{
				throw new ArgumentOutOfRangeException();
			}

			TempList.Clear();
			TempList.AddRange(OldList.Skip(oldListStart0).Take(oldListCount0));

			// yield return null;
			Profiler.BeginSample("AddPendingResetLocationAction");
			// delay update locations
			// if (OldList.Count < dataSources.Count)
			ClearPendingResetPosition();
			{
				var i = dataSourcesStart;
				for (; i < Math.Min(dataSourcesEnd, oldListEnd); i++)
				{
					if (OldList[i] != dataSources[i])
					{
						var child = _container.GetChild(i);
						this.AddPendingResetLocationAction(i, child.localPosition);
					}
				}
				
				for (; i < dataSourcesEnd; i++)
				{
					this.AddPendingResetLocationAction(i, InVisiblePos);
				}
			}
			Profiler.EndSample();
			if (dataSourcesCount > 512)
			{
				yield return null;
			}

			Profiler.BeginSample("UpdateDataItems");
			// remove first
			for (var i = oldListEnd - 1; i >= oldListStart0; i--)
			{
				var oldData = OldList[i];

				var newIndex = dataSources.IndexOf(oldData);
				if (newIndex < 0)
				{
					OldList.RemoveAt(i);
					oldListEnd--;
					this.OnRemoveItem(oldData, i);
				}
			}

			for (var i = dataSourcesStart; i < dataSourcesEnd; i++)
			{
				// if (i % 200 == 0)
				// {
				// 	yield return null;
				// }

				var dataSource = dataSources[i];
				if (oldListEnd <= i)
				{
					OldList.Add(dataSource);
					oldListEnd++;
					this.OnAddItem(dataSource, i);
				}
				else if (OldList[i] != dataSources[i])
				{
					var oldIndex = -1;
					for (var j = i + 1; j < oldListEnd; j++)
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
						oldListEnd++;
						this.OnAddItem(dataSource, i);
					}
					else
					{
						// move is usually fast, 暂时不考虑最优解
						OldList.RemoveAt(oldIndex);
						OldList.Insert(i, dataSource);
						// oldListEnd += 0;
						var isIndexMoved = TempList.Count <= i || TempList[i] != dataSource;
						this.OnMoveItem(dataSource, i, oldIndex, isIndexMoved);
					}
				}
				else
				{
					// same
					var isIndexMoved = TempList.Count <= i || TempList[i] != dataSource;
					if (isIndexMoved)
					{
						this.OnMoveItem(dataSource, i, i, isIndexMoved);
					}
				}
			}

			// remove duplicate at end
			for (int i = oldListEnd - 1; i >= dataSourcesEnd; i--)
			{
				var oldData = OldList[i];
				OldList.RemoveAt(i);
				oldListEnd--;
				this.OnRemoveItem(oldData, i);
			}

			Profiler.EndSample();

			Debug.Assert(OldList.Count >= oldListEnd);
			Debug.Assert(oldListEnd == oldListStart0 + dataSourcesCount);
			Debug.Assert(OldList.Count - oldListLen0 == dataSourcesCount - (oldListEnd0 - oldListStart0));


			if (needCheck)
			{
				Profiler.BeginSample("CheckResult");
				// check result
				Debug.Assert(oldListEnd == dataSourcesEnd,
					$"result length unmatched: old({oldListEnd})!=new({dataSourcesEnd})");
				for (var i = 0; i < Math.Min(oldListEnd, dataSources.Count); i++)
				{
					Debug.Assert(OldList[i] == dataSources[i], $"result unmatched: {i}");
				}

				Profiler.EndSample();
			}
			
			if (oldListEnd0 < dataSourcesEnd)
			{
				this.MarkLayoutDirty();
			}

			// yield return null;
			this.OnUpdateDone(dataSources, oldListCount0, dataSourcesStart, dataSourcesEnd);

			TempList.Clear();

			IsUpdatingItems = false;
		}

		public bool needCheck = false;

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

		protected virtual void OnUpdateDone(IList dataSources, int oldListCount0, int start, int end)
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
			for (var i = childCount - 1; i >= end; i--)
			{
				var child = _container.GetChild(i);
				this.UnuseNode(child);
			}

			if (TemplateNode == null || TemplateNode.GetSiblingIndex() >= end)
			{
				TemplateNode = null;
			}

			Profiler.EndSample();

			ApplyAllResetPosition();
		}
	}
}