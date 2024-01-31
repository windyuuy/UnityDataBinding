using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace DataBinding.UIBind.RecycleContainer
{
	public class GridGridScroller : CustomGridScrollerBase
	{
		public GameObject prefab;

		protected GridLayoutGroup GridLayoutGroup => (GridLayoutGroup)LayoutGroup;

		#region Controller

		#endregion

		protected int GuessWrapCount = 1;
		protected GridIter GridIterPre = new();
		protected override void UpdateContainerState(Vector2 val)
		{
			Profiler.BeginSample("UpdateContainerState");

			UpdateBasicStates(val);

			Profiler.EndSample();
		}

		private void TryAddTestNodes(int createCount)
		{
			for (var i = 0; i < createCount; i++)
			{
				var testNode = new GameObject("$test", typeof(RectTransform));
				testNode.transform.SetParent(container, false);
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(container);
		}

		private void Awake()
		{
			Vector2 cellSize;
			if (container.childCount > 0)
			{
				cellSize = ((RectTransform)container.GetChild(0)).sizeDelta;
			}
			else
			{
				cellSize = ((RectTransform)prefab.transform).sizeDelta;
			}

			if (GridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
			{
				GuessWrapCount = Mathf.CeilToInt(container.sizeDelta.x / cellSize.x);
			}
			else if (GridLayoutGroup.startAxis == GridLayoutGroup.Axis.Vertical)
			{
				GuessWrapCount = Mathf.CeilToInt(container.sizeDelta.y / cellSize.y);
			}
			else
			{
				throw new NotImplementedException();
			}

			GuessWrapCount = Math.Clamp(GuessWrapCount, 1, 100);

			int createCount = GuessWrapCount - container.childCount + 1;
			TryAddTestNodes(createCount);
		}

		private void Start()
		{
			if (GridIterNext.BodySizeInfo.x == 0 || GridIterNext.BodySizeInfo.y == 0)
			{
				UpdateContainerParams(GridIterNext);
			}

			while (GridIterNext.BodySizeInfo.x == 0 || GridIterNext.BodySizeInfo.y == 0)
			{
				TryAddTestNodes(GuessWrapCount);
				UpdateContainerParams(GridIterNext);
			}

			var pos0 = container.GetChild(0).localPosition;
			var posX1 = container.GetChild(GridIterNext.ToIndex(1, 0)).localPosition;
			var posY1 = container.GetChild(GridIterNext.ToIndex(0, 1)).localPosition;
			var vecX = posX1 - pos0;
			var vecY = posY1 - pos0;

			ClearTestNodes();
			
			// refresh real items for datas
			if (OldList == null)
			{
				OldList = new();
			}

			for (var i = container.childCount; i < ChildCountInit; i++)
			{
				var obj=GameObject.Instantiate(prefab);
			}

			for (var i = 0; i < ChildCountInit; i++)
			{
				var child = container.GetChild(i);
				
				var ipos = GridIterNext.ToPt(i);
				var pos = vecX*ipos.x + vecY*ipos.y;
				child.localPosition = pos;
				child.gameObject.SetActive(true);
			}

			for (var i = 0; i < ChildCountInit; i++)
			{
				var child=container.GetChild(i);
				UpdateDataBind(child, OldList[i], i);
			}

			for (var i = container.childCount-1; i >= ChildCountInit; i++)
			{
				var child = container.GetChild(i);
				child.gameObject.SetActive(false);
			}
		}

		private void ClearTestNodes()
		{
			for (var i = container.childCount - 1; i >= 0; i++)
			{
				var child = container.GetChild(i);
				if (child.gameObject.name.StartsWith("$"))
				{
					child.gameObject.DestorySafe();
				}
			}
		}
	}
}