using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DataBinding.UIBind;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// ReSharper disable All

namespace UIDataBinding.Runtime.RecycleContainer
{
	public partial class RecycleContainerCtrl : FlatContainerCtrl
	{
		[SerializeField] private ScrollRect scrollRect;

		/// <summary>
		/// Cached reference to the scrollRect's transform
		/// </summary>
		private RectTransform _scrollRectTransform;

// #if UNITY_EDITOR
		[SerializeField] private bool enableDebugView;
// #endif

		private bool _isAwake = false;

		protected override void Awake()
		{
			HandleDataChangedAfterAwake?.Invoke();

			_isAwake = true;
		}

		protected override void InitContainer()
		{
			InitLayoutRebuilder();

			Pool = new LocalNodeLinearPool(GenFromTemplate);
			StandPool = new(GenPlacer);
			LentPool = new((_) => throw new Exception("cannot implement for this lent pool"));
			if (this.scrollRect == null)
			{
				this.scrollRect = this.GetComponent<ScrollRect>();
			}

			if (this.scrollRect != null)
			{
				if (this._container == null)
				{
					this._container = this.scrollRect.content;
				}

				if (this._scrollRectTransform == null)
				{
					this._scrollRectTransform = this.scrollRect.GetComponent<RectTransform>();
				}
			}
			else
			{
				Debug.LogError($"scroll rect invalid, please set it");
			}

			base.InitContainer();
		}

		protected bool IsDataDirty = false;
		protected Action HandleDataChangedAfterAwake;

		protected override void OnDataChanged(IList dataSources)
		{
			if (dataSources.Count > 0)
			{
				IsDataDirty = true;

				if (!_isAwake)
				{
					HandleDataChangedAfterAwake = () => { HandleDataChangedOnInit(dataSources); };
				}
				else
				{
					HandleDataChangedOnInit(dataSources);
				}
			}
		}

		protected void HandleDataChangedOnInit(IList dataSources)
		{
			CalcPreviewCount();
			base.OnDataChanged(dataSources);

			if (IsDataDirty)
			{
				IsDataDirty = false;
				StartCoroutine(DelayStart());
			}
		}

		IEnumerator DelayStart()
		{
			yield return new WaitForEndOfFrame();

			this._ScrollRect_OnValueChanged(this.scrollRect.normalizedPosition);
		}

		void OnEnable()
		{
			// when the scroller is enabled, add a listener to the onValueChanged handler
			scrollRect.onValueChanged.AddListener(_ScrollRect_OnValueChanged);
		}

		void OnDisable()
		{
			// when the scroller is disabled, remove the listener
			scrollRect.onValueChanged.RemoveListener(_ScrollRect_OnValueChanged);
		}

		/// <summary>
		/// 可滚动长度
		/// </summary>
		public float GetScrollSizeY()
		{
			return Mathf.Max(_container.rect.height - _scrollRectTransform.rect.height, 0);
		}

		/// <summary>
		/// 可滚动长度
		/// </summary>
		public float GetScrollSizeX()
		{
			return Mathf.Max(_container.rect.width - _scrollRectTransform.rect.width, 0);
		}

		private Vector2 _scrollVal;

		/// <summary>
		/// The scrollers position
		/// </summary>    
		private Vector2 _scrollPosV;
		private Vector2 _scrollPosH;
		
		private Rect _scrollRectRange;

		private void UpdateScrollRectStatus(Vector2 val)
		{
			_scrollVal = val;

			var rect = _scrollRectTransform.rect;
			// var offset = _scrollRectTransform.position - min3 - _root.transform.position;
			if (scrollRect.vertical)
			{
				var scrollSizeY = GetScrollSizeY();
				var scrollBeginY = (1f - val.y) * scrollSizeY;
				// scrollBeginY = Mathf.Clamp(scrollBeginY, 0, scrollSizeY);
				if (scrollBeginY.CompareTo(_scrollPosV.x) != 0)
				{
					var scrollEndY = scrollBeginY - rect.height;
					_scrollPosV.x = scrollBeginY;
					_scrollPosV.y = scrollEndY;
					// Debug.Log($"_scrollBeginY: {scrollBeginY}, scrollEndY:{scrollEndY}, scrollSizeY:{scrollSizeY}");
				}
			}

			if (scrollRect.horizontal)
			{
				var scrollSizeX = GetScrollSizeX();
				var scrollBeginX = val.x * scrollSizeX;
				// scrollBeginX = Mathf.Clamp(scrollBeginX, 0, scrollSizeX);
				if (scrollBeginX.CompareTo(_scrollPosH.x) != 0)
				{
					var scrollEndX = scrollBeginX + rect.width;
					_scrollPosH.x = scrollBeginX;
					_scrollPosH.y = scrollEndX;
					// Debug.Log($"_scrollBeginX: {scrollBeginX}, scrollEndX:{scrollEndX}, scrollSizeX:{scrollSizeX}");
				}
			}

			var size = _scrollRectTransform.sizeDelta;
			_scrollRectRange = new Rect(_scrollPosH.x, -_scrollPosV.x - size.y, size.x, size.y);
		}

		private int _childCountInit;

		/// <summary>
		/// Handler for when the scroller changes value
		/// </summary>
		/// <param name="val">The scroll rect's value</param>
		private void _ScrollRect_OnValueChanged(Vector2 val)
		{
			UpdateContainerState(val);
		}

		protected void UpdateContainerState(Vector2 val)
		{
			NeedDelayUpdateContainer = false;
			
			UpdateScrollRectStatus(val);

			_childCountInit = OldList.Count;
			if (needCheck)
			{
				// var childCountInit = _container.childCount;
				// for (int i = _container.childCount - 1; i >= 0; i--)
				// {
				// 	if (_container.GetChild(i).gameObject.activeSelf)
				// 	{
				// 		break;
				// 	}
				//
				// 	childCountInit--;
				// }
				var childCountInit = 0;
				for (var i = 0; i < _container.childCount; i++)
				{
					if (!_container.GetChild(i).gameObject.activeSelf)
					{
						break;
					}
				
					childCountInit++;
				}

				Debug.Assert(_childCountInit == childCountInit);
			}

			if (!_gridIter.IsPrecision)
			{
				DetectRectInit(_scrollRectRange.center);
			}

			UpdateContainerParams(_gridIterNext);
			if (this.IsGracefullyDirty())
			{
				// TODO: 完善会闪一下的问题
				this.MarkLayoutDirty();
			}
			UpdateRectByScroll(_scrollRectRange.center);
			UpdateElementViewState();

			Profiler.BeginSample("_ScrollRect_OnValueChanged2");
			this.HandleScollDone();
			Profiler.EndSample();
		}

		public bool IsGracefullyDirty()
		{
			if (_gridIter.ScrollRectRange.width != _gridIterNext.ScrollRectRange.width ||
			    _gridIter.ScrollRectRange.height != _gridIterNext.ScrollRectRange.height 
			    )
			{
				_gridIter.ScrollRectRange = _gridIterNext.ScrollRectRange;
				return true;
			}

			return false;
		}

		private void UpdateElementViewState()
		{
			var isIn = false;
			_gridIterNext.IterAcc = 0;
			_gridIterNext.IterIndex = 1;
			var iterInto = _gridIterNext.GetForwardIter(() => isIn).GetEnumerator();
			var iterIntoRet0 = iterInto.MoveNext();
			_gridIterNext.IterIndex = 2;
			var iterOut = _gridIterNext.GetBackwardIter( () => isIn).GetEnumerator();
			var iterOutRet0 = iterOut.MoveNext();

			var iInto = iterIntoRet0 ? iterInto.Current : _childCountInit;

			int IterIntoContainer(bool suff)
			{
				if (0 <= iInto && iInto < _childCountInit)
				{
					var child = this._container.GetChild(iInto);
					var isInL = IsInContainer(child);
					if (IsVirtualNode(child) && isInL)
					{
						if (Pool.Count == 0 && suff)
						{
							return 1;
						}

						HandleChild(child, iInto);
					}

					// iInto++;
					isIn = isInL;
					if (!iterInto.MoveNext())
					{
						iInto = _childCountInit;
						return 2;
					}

					iInto = iterInto.Current;

					return 0;
				}

				iInto = _childCountInit;
				return 2;
			}

			// var iOut = _childCountInit - 1;
			var iOut = iterOutRet0 ? iterOut.Current : _childCountInit;

			int InterOutContainer(bool suff)
			{
				if (0 <= iOut && iOut < _childCountInit)
				{
					var child = this._container.GetChild(iOut);
					var isInL = IsInContainer(child);
					if (!IsVirtualNode(child) && !isInL)
					{
						if (StandPool.Count == 0 && suff)
						{
							return 1;
						}

						HandleChild(child, iOut);
					}

					// iOut--;
					isIn = isInL;
					if (!iterOut.MoveNext())
					{
						iOut = _childCountInit;
						return 2;
					}

					iOut = iterOut.Current;

					return 0;
				}

				iOut = _childCountInit;
				return 2;
			}

			Profiler.BeginSample("_ScrollRect_OnValueChanged1");

			var seekIn = true;
			while (IsValidIndex(iInto) || IsValidIndex(iOut))
			{
				if (seekIn)
				{
					var rIn = IterIntoContainer(IsValidIndex(iOut));
					if (rIn >= 1)
					{
						seekIn = false;
					}
				}
				else
				{
					var rOut = InterOutContainer(IsValidIndex(iInto) && Pool.Count > 0);
					if (rOut >= 1)
					{
						seekIn = true;
					}
				}
			}

			Debug.Assert(!iterInto.MoveNext());
			Debug.Assert(!iterOut.MoveNext());
			_gridIterNext.FinishIter();
			_gridIter.Copy(ref _gridIterNext);
			//
			// for (var i = 0; i < _childCountInit; i++)
			// {
			// 	var child = _container.GetChild(i);
			// 	if (IsVirtualNode(child) == IsInContainer(child))
			// 	{
			// 		Debug.LogError($"lkwjeflk: {i}");
			// 	}
			// }

			Profiler.EndSample();
		}

		public bool IsValidIndex(int index)
		{
			return 0 <= index && index < _childCountInit;
		}

		public bool IsInvalidIndex(int index)
		{
			return 0 > index || index >= _childCountInit;
		}

		protected bool HandleChild(Transform child, int index)
		{
			var isInContainer = IsInContainer(child);
			if (!isInContainer && IsVirtualNode(child))
			{
				return true;
			}

			if (isInContainer)
			{
				this.HandleChildAppear(child, index);
			}
			else
			{
				this.HandleChildDisappear(child, index);
			}

			return false;
		}

		protected bool IsInContainer(int index)
		{
			return IsInContainer(_container.GetChild(index));
		}

		protected bool IsInContainerSafe(int index)
		{
			return 0 <= index && index < _childCountInit && IsInContainer(_container.GetChild(index));
		}

		protected bool IsInContainer(Transform child)
		{
			var rect = GetRectBounds(child);
			var isInContainerVertical = false;
			var isInContainerHorizontal = false;
			if (scrollRect.vertical)
			{
				if (rect.yMax >= _scrollRectRange.yMin && rect.yMin <= _scrollRectRange.yMax)
				{
					isInContainerVertical = true;
				}
			}
			else
			{
				isInContainerVertical = true;
			}

			if (scrollRect.horizontal)
			{
				if (rect.xMax >= _scrollRectRange.xMin && rect.xMin <= _scrollRectRange.xMax)
				{
					isInContainerHorizontal = true;
				}
			}
			else
			{
				isInContainerHorizontal = true;
			}

			var isInContainer = isInContainerVertical && isInContainerHorizontal;
			return isInContainer;
		}

		protected LocalNodeLinearPool Pool;
		protected LocalNodePoolFast StandPool;
		protected LocalNodeLinearPool LentPool;

		protected bool IsVirtualNode(Transform child)
		{
			// return child.gameObject.name.StartsWith("$");
			var name = child.gameObject.name;
			if (name != null && name.Length > 0)
			{
				if (name[0] == '$')
				{
					return true;
				}
			}

			return false;
		}

		protected override Transform GetTemplateNode(int index)
		{
			if (TemplateNode.IsInValid())
			{
				TemplateNode = null;
				for (var i = 0; i < _container.childCount; i++)
				{
					var child = _container.GetChild(i);
					if (!IsVirtualNode(child))
					{
						TemplateNode = child;
						return TemplateNode;
					}
				}
			}

			return TemplateNode;
		}

		/// <summary>
		/// 只有滚动的时候调用
		/// </summary>
		/// <param name="child"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		private Transform HandleChildAppear(Transform child, int index)
		{
			if (!IsVirtualNode(child))
			{
				// 当前是真实节点，无需处理
				return child;
			}
			else
			{
				StandPool.Recycle(child);
				var childReal = Pool.Get(index);
				LentPool.Recycle(childReal);
				// AddPendingResetLocationAction(childReal, index, child.localPosition);
				childReal.localPosition = child.localPosition;
				childReal.SetSiblingIndex(index);

				UpdateCurrentDataBind(childReal, index);
				return childReal;
			}
		}

		/// <summary>
		/// 只有滚动的时候调用， 如果子节点不再是真实节点，那么返回false
		/// </summary>
		/// <param name="child"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		private bool HandleChildDisappear(Transform child, int index)
		{
			if (!IsVirtualNode(child))
			{
				// Pool.Recycle(child);
				// LentPool.Remove(child);
				RecycleLent(child);
				var childVirt = StandPool.Get(index);
				// AddPendingResetLocationAction(childVirt, index, child.localPosition);
				childVirt.localPosition = child.localPosition;
				childVirt.SetSiblingIndex(index);
				return true;
			}
			else
			{
				return false;
			}
		}

		protected override void UpdateCurrentDataBind(Transform child, int index)
		{
			if (OldList != null && OldList.Count > index)
			{
				UpdateDataBind(child, OldList[index], index);
			}
			else
			{
				// TODO: add a warning
			}
		}

		protected override void UpdateDataBind(Transform child, object dataSource, int index)
		{
			if (!IsVirtualNode(child))
			{
				base.UpdateDataBind(child, dataSource, index);
			}
		}

		Vector2 ToVec2(Vector3 pos)
		{
			return new Vector2(pos.x, pos.y);
		}

		public Rect GetRectBounds(Transform trans)
		{
			var rect = ((RectTransform)trans).rect;
			// rect.center = rect.center + ToVec2(transform.position) - ToVec2(_root.transform.position);
			rect.center += ToVec2(trans.localPosition);
			return rect;
		}

		public int previewCount = -1;

		public void CalcPreviewCount()
		{
			if (previewCount < 0)
			{
				var tempNode = this.GetTemplateNode(0);
				if (tempNode == null)
				{
					previewCount = 0;
					return;
				}

				var tempSize = ((RectTransform)tempNode).sizeDelta;
				var rectSize = this._container.sizeDelta;

				var ix = scrollRect.horizontal ? Mathf.FloorToInt(rectSize.x / tempSize.x * 0.5f) : 1;
				var iy = scrollRect.vertical ? Mathf.FloorToInt(rectSize.y / tempSize.y * 0.5f) : 1;
				previewCount = ix * iy;
			}
		}

		public virtual Transform RequirePlacer(int index, string name)
		{
			var child = StandPool.Get(index);
			child.gameObject.name = name;
			return child;
		}

		public virtual Transform RequirePlacer(int index)
		{
			var child = StandPool.Get(index);
			return child;
		}

		private RectTransform _standTempNode;
		public virtual Transform GenPlacer(int index)
		{
			var emptyNode = new GameObject($"${index}", typeof(RectTransform));

// #if UNITY_EDITOR
			if (enableDebugView)
			{
				emptyNode.AddComponent<Image>();
			}
// #endif
			emptyNode.transform.SetParent(this._container);

			if (_standTempNode.IsInValid())
			{
				if (index < this._container.childCount)
				{
					_standTempNode = (RectTransform)this._container.GetChild(index);
				}
				else
				{
					_standTempNode = (RectTransform)GetTemplateNode(index);
				}
			}

			var emptyNodeTrans = (RectTransform)emptyNode.transform;
			emptyNodeTrans.sizeDelta = _standTempNode.sizeDelta;
			emptyNodeTrans.pivot = _standTempNode.pivot;
			emptyNodeTrans.localPosition = InVisiblePos;
			return emptyNodeTrans;
		}

		public virtual Transform CreateNewNodeAsync(int index,
			Action<Transform, Transform, Action<Transform>> onCreatedAsync,
			Func<int, string, Transform> createStandSync, Action<Transform> recycleStandAsync)
		{
			// onCreatedAsync(childAsync, createStandSync(index, $"Stand{index}"), recycleStandAsync);
			return createStandSync(index, $"Stand{index}");
		}

		protected virtual Transform CreateNewNodeAsyncInternal(int index, Func<int, string, Transform> createStandSync,
			Action<Transform> defaultRecycleStandAsync)
		{
			return CreateNewNodeAsync(index, (childAsync, standSync, recycleStandAsync) =>
			{
				if (childAsync == standSync)
				{
					return;
				}

				Debug.Assert(childAsync.IsValid());

				LentPool.Recycle(childAsync);

				if (standSync.IsValid())
				{
					Debug.Assert(!IsVirtualNode(standSync));

					var isNotRecycled = !Pool.Contains(standSync);
					if (
						// not recycled
						isNotRecycled)
					{
						Debug.Assert(standSync.GetSiblingIndex() < _childCountInit);
						// Debug.Assert(IsInContainer(standSync));
						
						// recycle stand
						LentPool.Remove(standSync);

						var isInContainer = IsInContainer(standSync);
						if (!isInContainer)
						{
							SetLayoutDirtyForce();
						}
						
						{
							var index2 = standSync.GetSiblingIndex();
							this.UpdateCurrentDataBind(childAsync, index2);
							if (childAsync.parent != _container)
							{
								childAsync.SetParent(_container);
							}

							// AddPendingResetLocationAction(childAsync, index2, standSync.localPosition);
							childAsync.localPosition = standSync.localPosition;
							childAsync.SetSiblingIndex(index2);

							recycleStandAsync(standSync);
						}
					}
					else
					{
						PrepareDataBind(childAsync);
						if (childAsync.parent != _container)
						{
							childAsync.SetParent(_container);
						}

						RecycleNode(childAsync);
						UnuseNode(childAsync);

						var index2 = standSync.GetSiblingIndex();
						childAsync.localPosition = standSync.localPosition;
						childAsync.SetSiblingIndex(index2);

						Pool.Remove(standSync);
						recycleStandAsync(standSync);
					}
				}
				else
				{
					PrepareDataBind(childAsync);
					if (childAsync.parent != _container)
					{
						childAsync.SetParent(_container);
					}

					RecycleNode(childAsync);
					UnuseNode(childAsync);

					LentPool.Remove(standSync);
					
					// try remove just for safe
					Pool.Remove(standSync);
				}
			}, createStandSync, defaultRecycleStandAsync);
		}

		protected bool NeedDelayUpdateContainer = false;
		protected Coroutine CoDelayUpdateContainer;
		protected void SetLayoutDirtyForce()
		{
			NeedDelayUpdateContainer = true;
			if (CoDelayUpdateContainer == null)
			{
				StartCoroutine(DelayedSetDirty(this._container));
			}
		}

		IEnumerator DelayedSetDirty(RectTransform rectTransform)
		{
			// yield return new WaitForSeconds(1);
			yield return null;
			// yield return new WaitForEndOfFrame();
			if (CoDelayUpdateContainer != null)
			{
				StopCoroutine(CoDelayUpdateContainer);
				CoDelayUpdateContainer = null;
			}
			if (NeedDelayUpdateContainer)
			{
				UpdateContainerState(_scrollVal);
			}
		}

		public virtual Transform RequireNewNodeCompatAsync(int index)
		{
			Transform child;
			if (unallocNodes != null && unallocNodes.Count > 0)
			{
				child = unallocNodes[0];
				// TemplateNode = child;
				unallocNodes.RemoveAt(0);
				LentPool.Recycle(child);
			}
			else
			{
				child = CreateNewNode(index);
				if (child == null)
				{
					child = CreateNewNodeAsyncInternal(index, (index, name) =>
					{
						var placer = RequirePlacer(index, name);
						placer.gameObject.name = $"stand{index}";
						return placer;
					}, (standSync) =>
					{
						var o = standSync.gameObject;
						o.name = $"${o.name}";
						RecycleNode(standSync);
						UnuseNode(standSync);
					});
				}
			}

			return child;
		}

		public virtual Transform GenFromTemplate(int index)         
		{
			return RequireNewNodeCompatAsync(index);
		}

		public override Transform RequireNewNode(int index)
		{
			var child = RequireNewNodeInternal(index);
			Profiler.BeginSample("SetSiblingIndex");
			child.SetSiblingIndex(index);
			Profiler.EndSample();
			return child;
		}

		protected Transform RequireNewNodeInternal(int index)
		{
			if (!_isAwake)
			{
				if (this._container.childCount < previewCount && index < this._container.childCount)
				{
					var child = this._container.GetChild(index);
					if (child == TemplateNode || IsInContainer(child))
					{
						var childReal = RequireNewNodeCompatAsync(index);
						return childReal;
					}
					else
					{
						return RequirePlacer(index);
					}
				}
				else
				{
					return RequirePlacer(index);
				}
			}
			else
			{
				if (index < this._container.childCount)
				{
					var child = this._container.GetChild(index);
					if (child.gameObject.activeSelf)
					{
						if (IsInContainer(child))
						{
							var childReal = Pool.Get(index);
							LentPool.Recycle(childReal);
							return childReal;
						}
						else
						{
							return RequirePlacer(index);
						}
					}
					else
					{
						Profiler.BeginSample("RequirePlacer");
						var child2 = RequirePlacer(index);
						Profiler.EndSample();
						return child2;
					}
				}
				else
				{
					return RequirePlacer(index);
				}
			}
		}

		protected override void RecycleNode(Transform child)
		{
			if (IsVirtualNode(child))
			{
				StandPool.Recycle(child);
			}
			else
			{
				// Pool.Recycle(child);
				// LentPool.Remove(child);
				RecycleLent(child);
			}
		}

		protected void RecycleLent(Transform child)
		{
			if (child.name.StartsWith("stand"))
			{
				Pool.RecycleBack(child);
			}
			else
			{
				Pool.Recycle(child);
			}
			LentPool.Remove(child);
		}

		protected override void UnuseNode(Transform child)
		{
			child.gameObject.SetActive(false);
		}

		public virtual void ReleaseNode(Transform child)
		{
			child.gameObject.DestorySafe();
		}
		
		protected override void ApplyResetPosition(Transform child, int index, ref Vector3 localPosition)
		{
			base.ApplyResetPosition(child, index, ref localPosition);
			HandleChild(child, index);
		}

		protected virtual void HandleScollDone()
		{
			// while (Pool.Count>1
			//        && -0.001f<=_scrollVal.y && _scrollVal.y<=1 && -0.001f<=_scrollVal.x && _scrollVal.x<=1
			//        )
			// {
			// 	var child= Pool.PopRaw();
			// 	if (child != TemplateNode)
			// 	{
			// 		child.gameObject.DestorySafe();
			// 	}
			// }

			if (!IsUpdatingItems)
			{
				ApplyAllResetPosition();
			}
			// else
			// {
			// 	SetLayoutDirtyForce();
			// }

			if (_childCountInit != _container.childCount)
			{
				// var standCount = 0;
				// for (var i = 0; i < _childCountInit; i++)
				// {
				// 	var child = _container.GetChild(i);
				// 	if (IsVirtualNode(child))
				// 	{
				// 		standCount++;
				// 	}
				// }
				//
				// var lackCount = _childCountInit - standCount;
				// while (StandPool.Count > lackCount)
				// {
				// 	var child = StandPool.PopRaw();
				// 	Debug.Assert(IsVirtualNode(child));
				// 	this.ReleaseNode(child);
				// }

				for (var i = _container.childCount - 1; i >= _childCountInit; i--)
				{
					var child = _container.GetChild(i);
					if (child.IsValid())
					{
						this.UnuseNode(child);
					}
				}
			}

			if (!IsLayoutDirty())
			{
				UnMarkRebuild();
			}
			else
			{
				// UpdateLentSizes();
			}
		}

		protected override void OnUpdateDone(IList dataSources, int oldListCount, int start, int end)
		{
			if (PendingResetPostion.Count > 0 || oldListCount < dataSources.Count)
			{
				var dirtyIndexes = this._gridIterNext.DirtyIndexes;
				if (dirtyIndexes == null)
				{
					dirtyIndexes = this._gridIterNext.DirtyIndexes =
						new(PendingResetPostion.Count + (dataSources.Count - oldListCount));
				}
				dirtyIndexes.Clear();

				// 新增的部分可能在布局变更同时进行，需要延后到刷新布局时进行二次更新
				if (PendingResetPostion.Count > 0)
				{
					dirtyIndexes.AddRange(PendingResetPostion.Select(p=>p.Item1));
				}

				// if (oldListCount < dataSources.Count)
				// {
				// 	for (var i = oldListCount; i < dataSources.Count; i++)
				// 	{
				// 		dirtyIndexes.Add(i);
				// 	}
				// }

				// if (dirtyIndexes.Count > 0)
				// {
				// 	var sb = new StringBuilder();
				// 	foreach (var dirtyIndex in dirtyIndexes)
				// 	{
				// 		sb.Append($"{dirtyIndex},");
				// 	}
				// 	Debug.Log($"DirtyIndexes: {sb.ToString()}");
				// }
			}

			base.OnUpdateDone(dataSources, oldListCount, start, end);

			// TODO: 优化数据更新，尽量避免更新 layout
			if (!IsLayoutDirty())
			{
				UnMarkRebuild();
			}
			
			// CheckLent();
		}

		private void CheckLent()
		{
			foreach (var transform1 in LentPool.PeekAll())
			{
				if (!IsInContainer(transform1) && !IsVirtualNode(transform1))
				{
					Debug.LogError("lkwjeflkj");
				}
			}
		}

		protected System.Collections.Generic.List<(Transform, Vector2)> LentSizes = new();

		protected void UpdateLentSizes()
		{
			LentSizes.Clear();
			var pool = LentPool.PeekAll();
			foreach (var child in pool)
			{
				LentSizes.Add((child, ((RectTransform)child).sizeDelta));
			}
		}

		protected void CheckLentSizes()
		{
			if (LentSizes.Count != LentPool.Count)
			{
				MarkLayoutDirty();
				return;
			}

			for (var i = 0; i < LentPool.Count; i++)
			{
				var (child, size) = LentSizes[i];
				if (size != ((RectTransform)child).sizeDelta)
				{
					MarkLayoutDirty();
					return;
				}
			}
		}

		// public virtual void Update()
		// {
		// 	CheckLentSizes();
		// }
		
		protected static IList<ICanvasElement> LayoutRebuildQueue;

		public static void InitLayoutRebuilder()
		{
			if (LayoutRebuildQueue == null)
			{
				var ii = CanvasUpdateRegistry.instance;
				var f = ii.GetType().GetField("m_LayoutRebuildQueue",
					BindingFlags.NonPublic | BindingFlags.Instance);
				LayoutRebuildQueue = f!.GetValue(ii) as IList<ICanvasElement>;
			}
		}

		protected bool IsRebuildingLayout()
		{
			ICanvasElement rebuilder = null;
			for (var i = 0; i < LayoutRebuildQueue.Count; i++)
			{
				if (LayoutRebuildQueue[i].transform == this.transform)
				{
					rebuilder = LayoutRebuildQueue[i];
					break;
				}
			}

			if (rebuilder != null)
			{
				return true;
			}

			return false;
		}

		protected void UnMarkRebuild()
		{
			ICanvasElement rebuilder = null;
			for (var i = 0; i < LayoutRebuildQueue.Count; i++)
			{
				if (LayoutRebuildQueue[i].transform == this.transform)
				{
					rebuilder = LayoutRebuildQueue[i];
					break;
				}
			}

			if (rebuilder != null)
			{
				LayoutRebuildQueue.Remove(rebuilder);
				rebuilder.LayoutComplete();
			}
		}
	}
}