using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding.UIBind;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace EaseScrollView.EnhanceScrollView.Plugins
{
	[ExecuteInEditMode]
	public class DefaultEnhancedScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
	{
		/// <summary>
		/// This is our scroller we will be a delegate for
		/// </summary>
		public EnhancedScroller enhancedScroller;

		/// <summary>
		/// This will be the prefab of each cell in our scroller. Note that you can use more
		/// than one kind of cell, but this example just has the one type.
		/// </summary>
		protected GameObject CellViewPrefab;

		protected EnhancedScrollerCellView CellViewPrefabComp;

		private bool _isAwaked = false;
		protected bool IsDirty = false;

		protected virtual void Awake()
		{
			InitContainer();
			
			if (CellViewPrefab == null)
			{
				if (enhancedScroller.Container.childCount > 0)
				{
					CellViewPrefab = enhancedScroller.Container.GetChild(0).gameObject;
				}
			}

			Debug.Assert(CellViewPrefab != null, "cellViewPrefab != null");
			var cellViewComp = CellViewPrefab.GetComponent<EnhancedScrollerCellView>();
			if (cellViewComp == null)
			{
				cellViewComp = CellViewPrefab.AddComponent<EnhancedScrollerCellView>();
			}

			CellViewPrefabComp = cellViewComp;

			_isAwaked = true;
			
			DelayReloadData();
		}

		public virtual void OnScrollerInitialized()
		{
			DelayReloadData();
		}

		public void DelayReloadData()
		{
			if (IsDirty && _isAwaked && enhancedScroller.IsInitialized)
			{
				IsDirty = true;
				enhancedScroller.ReloadData();
			}
		}

		protected virtual void InitContainer()
		{
			if (enhancedScroller == null)
			{
				enhancedScroller = this.gameObject.GetComponent<EnhancedScroller>();
				if (enhancedScroller == null)
				{
					enhancedScroller = this.gameObject.AddComponent<EnhancedScroller>();
				}
			}

			Debug.Assert(enhancedScroller != null, "enhancedScroller != null");
			// tell the scroller that this script will be its delegate
			enhancedScroller.Delegate = this;

			if (CellViewPrefab == null)
			{
				CellViewPrefab = enhancedScroller.cellViewPrefab;
			}
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

			if (_isAwaked && enhancedScroller.IsInitialized)
			{
				enhancedScroller.ReloadData();
			}
			else
			{
				IsDirty = true;
			}
		}

		public bool EnablePreview
		{
			get
			{
				if (enhancedScroller!=null)
				{
					return enhancedScroller.EnablePreview;
				}

				return false;
			}
		}

		public int PreviewCount
		{
			get
			{
				if (enhancedScroller!=null)
				{
					return enhancedScroller.PreviewCount;
				}

				return 0;
			}
		}
		#region EnhancedScroller Handlers

		/// <summary>
		/// This tells the scroller the number of cells that should have room allocated. This should be the length of your data array.
		/// </summary>
		/// <returns>The number of cells</returns>
		public virtual int GetNumberOfCells()
		{
#if UNITY_EDITOR
			if (EnablePreview)
			{
				return PreviewCount;
			}
#endif
			// in this example, we just pass the number of our data elements
			return OldList?.Count ?? enhancedScroller.Container.childCount;
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
			var cellView = this.enhancedScroller.GetCellViewAtDataIndex(dataIndex);
			if (cellView == null)
			{
				cellView = this.CellViewPrefabComp;
			}

			switch (this.enhancedScroller.scrollDirection)
			{
				// in this example, even numbered cells are 30 pixels tall, odd numbered cells are 100 pixels tall
				// return (dataIndex % 2 == 0 ? 30f : 100f);
				case EnhancedScroller.ScrollDirectionEnum.Horizontal:
					return ((RectTransform)cellView.transform).sizeDelta.x;
				case EnhancedScroller.ScrollDirectionEnum.Vertical:
					return ((RectTransform)cellView.transform).sizeDelta.y;
				default:
				{
					throw new NotImplementedException();
				}
			}
		}

		protected virtual void UpdateDataBind(Transform child, object dataSource,int index)
		{
			var ccItem = child.GetComponent<ContainerItemComp>();
			if (ccItem == null)
			{
				ccItem = child.gameObject.AddComponent<ContainerItemComp>();
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

		/// <summary>
		/// Gets the cell to be displayed. You can have numerous cell types, allowing variety in your list.
		/// Some examples of this would be headers, footers, and other grouping cells.
		/// </summary>
		/// <param name="scroller">The scroller requesting the cell</param>
		/// <param name="dataIndex">The index of the data that the scroller is requesting</param>
		/// <param name="cellIndex">The index of the list. This will likely be different from the dataIndex if the scroller is looping</param>
		/// <returns>The cell for the scroller to use</returns>
		public virtual EnhancedScrollerCellView GetCellView( int dataIndex, int cellIndex)
		{
			// first, we get a cell from the scroller by passing a prefab.
			// if the scroller finds one it can recycle it will do so, otherwise
			// it will create a new cell.
			var cellView = this.enhancedScroller.GetCellView(CellViewPrefabComp);

			if (!EnablePreview)
			{
				UpdateDataBind(cellView.transform, OldList[dataIndex], dataIndex);
			}

			// set the name of the game object to the cell's data index.
			// this is optional, but it helps up debug the objects in 
			// the scene hierarchy.
			cellView.name = "Cell " + dataIndex.ToString();

			// return the cell to the scroller
			return cellView;
		}

		#endregion
	}
}