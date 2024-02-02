using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding.UIBind;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EaseScrollView.EnhanceScrollView.Plugins
{
	public class MyGridScrollerController : MonoBehaviour, IGridScrollerDelegate
	{
		/// <summary>
		/// This is our scroller we will be a delegate for
		/// </summary>
		public MyGridLayoutGroup enhancedScroller;

		public ScrollRect scrollRect;
		protected RectTransform ScrollRectTransform;
		protected Rect ScrollRectRange;

		/// <summary>
		/// This will be the prefab of each cell in our scroller. Note that you can use more
		/// than one kind of cell, but this example just has the one type.
		/// </summary>
		[SerializeField] protected GameObject cellViewPrefab;

		private bool _isAwaked = false;
		protected bool IsDirty = false;

		protected virtual void Awake()
		{
			InitContainer();

			if (cellViewPrefab == null)
			{
				if (scrollRect.content.childCount > 0)
				{
					cellViewPrefab = scrollRect.content.GetChild(0).gameObject;
				}
			}

			Debug.Assert(cellViewPrefab != null, "cellViewPrefab != null");

			_isAwaked = true;

			DelayReloadData();
		}

		public void DelayReloadData()
		{
			if (IsDirty && _isAwaked)
			{
				IsDirty = true;
				LayoutRebuilder.MarkLayoutForRebuild(this.scrollRect.content);
			}
		}

		protected virtual void InitContainer()
		{
			if (scrollRect == null)
			{
				scrollRect = this.GetComponent<ScrollRect>();
			}

			Debug.Assert(scrollRect != null, "scrollRect != null");
			ScrollRectTransform = (RectTransform)scrollRect.transform;
			ScrollRectRange = ScrollRectTransform.rect;
			ScrollRectRange.center -= ToVec2(this.scrollRect.content.localPosition);

			if (enhancedScroller == null)
			{
				enhancedScroller = scrollRect.content.GetComponent<MyGridLayoutGroup>();
			}

			Debug.Assert(enhancedScroller != null, "enhancedScroller != null");

			// tell the scroller that this script will be its delegate
			enhancedScroller.Delegate = this;
		}

		protected List<object> OldList;

		protected virtual void OnDataChanged(System.Collections.IList dataSources)
		{
			if (this.OldList == null)
			{
				this.OldList = new List<object>(dataSources.Count);
			}
			else
			{
				this.OldList.Clear();
			}

			foreach (var dataSource in dataSources)
			{
				OldList.Add(dataSource);
			}

			if (_isAwaked)
			{
				LayoutRebuilder.MarkLayoutForRebuild(this.scrollRect.content);
			}
			else
			{
				IsDirty = true;
			}
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

		private RectTransform sampleChild;
		private void _ScrollRect_OnValueChanged(Vector2 val)
		{
			ScrollRectRange = ScrollRectTransform.rect;
			ScrollRectRange.center -= ToVec2(this.scrollRect.content.localPosition);

			// if (sampleChild == null)
			// {
			// 	this.sampleChild = (RectTransform)new GameObject("ef", typeof(RectTransform), typeof(Image)).transform;
			// }
			// this.sampleChild.SetParent(this.scrollRect.content, false);
			// sampleChild.sizeDelta = ScrollRectRange.size;
			// sampleChild.localPosition = ScrollRectRange.center;
			// this.sampleChild.SetParent(this.transform, true);
			
			if (!IsContainRectFully(ref _richDelta,ref ScrollRectRange))
			{
				LayoutRebuilder.MarkLayoutForRebuild(this.scrollRect.content);
			}
		}

		#region EnhancedScroller Handlers

		/// <summary>
		/// This tells the scroller the number of cells that should have room allocated. This should be the length of your data array.
		/// </summary>
		/// <returns>The number of cells</returns>
		public virtual int GetNumberOfCells()
		{
			// in this example, we just pass the number of our data elements
			return OldList?.Count ?? 0;
		}

		/// <summary>
		/// This tells the scroller what the size of a given cell will be. Cells can be any size and do not have
		/// to be uniform. For vertical scrollers the cell size will be the height. For horizontal scrollers the
		/// cell size will be the width.
		/// </summary>
		/// <param name="scroller">The scroller requesting the cell size</param>
		/// <param name="dataIndex">The index of the data that the scroller is requesting</param>
		/// <returns>The size of the cell</returns>
		public virtual float GetCellViewSize(int dataIndex)
		{
			throw new NotImplementedException();
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

		public virtual RectTransform GetCellView(int dataIndex, int cellIndex)
		{
			// first, we get a cell from the scroller by passing a prefab.
			// if the scroller finds one it can recycle it will do so, otherwise
			// it will create a new cell.
			// var cellView = this.enhancedScroller.GetCellView(CellViewPrefabComp);
			var cellView = GameObject.Instantiate(cellViewPrefab, this.scrollRect.content);

			UpdateDataBind(cellView.transform, OldList[dataIndex], dataIndex);

			// set the name of the game object to the cell's data index.
			// this is optional, but it helps up debug the objects in 
			// the scene hierarchy.
			cellView.name = "Cell " + dataIndex.ToString();

			// return the cell to the scroller
			return (RectTransform)cellView.transform;
		}

		public static readonly Vector3 InVisiblePos = new Vector3(float.MaxValue * 0.5f, float.MaxValue * 0.5f, 0);
		public virtual void RecycleCellView(RectTransform cellView, int dataIndex)
		{
			cellView.localPosition = InVisiblePos;
		}

		public Vector2 ToVec2(Vector3 pos)
		{
			return new Vector2(pos.x, pos.y);
		}

		protected bool IsContainRectFully(ref Rect scrollRectRange, ref Rect rect)
		{
			var isInContainer = (scrollRectRange.yMax >= rect.yMax && rect.yMin >= scrollRectRange.yMin) &&
			                    (scrollRectRange.xMax >= rect.xMax && rect.xMin >= scrollRectRange.xMin);
			return isInContainer;
		}

		public virtual bool IsVisible(ref Rect child, int dataIndex, bool isVisible)
		{
			return isVisible;
		}

		public virtual Rect GetClipRect()
		{
			return ScrollRectRange;
		}

		private Rect _richDelta;

		public virtual void SetRichDelta(float xMin, float yMin, float xMax, float yMax)
		{
			_richDelta.Set(xMin, yMin, xMax - xMin, yMax - yMin);
		}

		#endregion
	}
}