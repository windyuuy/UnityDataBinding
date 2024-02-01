using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DataBinding.UIBind.RecycleContainer
{
	public class CustomGridScrollerBase : MonoBehaviour
	{
		protected bool IsAwake = false;

		[SerializeField] protected ScrollRect scrollRect;

		/// <summary>
		/// Cached reference to the scrollRect's transform
		/// </summary>
		protected RectTransform ScrollRectTransform;

// #if UNITY_EDITOR
		[SerializeField] protected bool enableDebugView;
// #endif

		[SerializeField] protected RectTransform container = null;

		protected LayoutGroup LayoutGroup;

		protected List<object> OldList;

		/// <summary>
		/// 可滚动长度
		/// </summary>
		public float GetScrollSizeY()
		{
			return Mathf.Max(container.rect.height - ScrollRectTransform.rect.height, 0);
		}

		/// <summary>
		/// 可滚动长度
		/// </summary>
		public float GetScrollSizeX()
		{
			return Mathf.Max(container.rect.width - ScrollRectTransform.rect.width, 0);
		}

		protected Vector2 ScrollVal;

		/// <summary>
		/// The scrollers position
		/// </summary>    
		private Vector2 _scrollPosV;

		private Vector2 _scrollPosH;

		protected Rect ScrollRectRange;
		protected GridIter GridIterNext = new();

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
		/// Handler for when the scroller changes value
		/// </summary>
		/// <param name="val">The scroll rect's value</param>
		private void _ScrollRect_OnValueChanged(Vector2 val)
		{
			UpdateContainerState(val);
		}

		protected virtual void UpdateContainerState(Vector2 val)
		{
			Profiler.BeginSample("UpdateContainerState");

			UpdateBasicStates(val);

			Profiler.EndSample();
		}

		protected virtual void UpdateBasicStates(Vector2 val)
		{
			if (!GridIterNext.IsPrecision)
			{
				DetectRectInit(ScrollRectRange.center);
			}

			UpdateChildCount();
			UpdateScrollRectStatus(val);
			UpdateContainerParams(GridIterNext);
		}

		public bool needCheck = false;

		public void DetectRectInit(Vector2 scrollPos)
		{
			// create grid iters
			// no element
			// _gridIterNext =
			// 	GridIter.FromCorners(new IntVector2(0, 0), new IntVector2(-2, -2), _childCountInit, scrollPos);
			GridIterNext.Pos = new Vector2(0, 0);
			GridIterNext.TotalCount = ChildCountInit;
			GridIterNext.ScrollPos = scrollPos;
			GridIterNext.Size = new IntVector2(-2, -2);

			// if (_childCountInit == 0)
			{
				GridIterNext.IsPrecision = true;
			}

			GridIterNext.ScrollRectRange = ScrollRectRange;

			GridIterNext.NeedCheck = this.needCheck;
		}

		protected int ChildCountInit;

		protected void UpdateChildCount()
		{
			ChildCountInit = OldList?.Count??0;
		}

		private void UpdateScrollRectStatus(Vector2 val)
		{
			ScrollVal = val;

			var rect = ScrollRectTransform.rect;
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

			var size = ScrollRectTransform.sizeDelta;
			ScrollRectRange = new Rect(_scrollPosH.x, -_scrollPosV.x - size.y, size.x, size.y);
		}

		/// <summary>
		/// UpdateContainerBodySize
		/// </summary>
		/// <param name="gridIter"></param>
		/// <returns>return false if not update fully</returns>
		/// <exception cref="NotImplementedException"></exception>
		protected bool UpdateContainerParams(GridIter gridIter)
		{
			var isOk = false;

			var childCount = ChildCountInit;
			IntVector2 bodySizeInfo;
			IntVector2 iterSize;
			var lineBreakSize = 1;
			LayoutGroup ??= container.GetComponent<LayoutGroup>();
			var layoutGroup = LayoutGroup;
			if (layoutGroup is VerticalLayoutGroup)
			{
				bodySizeInfo = new IntVector2(1, 1);
				lineBreakSize = 1;
				iterSize = new IntVector2(int.MaxValue, 1);
				isOk = true;
			}
			else if (layoutGroup is HorizontalLayoutGroup)
			{
				bodySizeInfo = new IntVector2(1, childCount);
				lineBreakSize = int.MaxValue;
				iterSize = new IntVector2(1, int.MaxValue);
				isOk = true;
			}
			else if (layoutGroup is GridLayoutGroup gridLayoutGroup)
			{
				if (childCount == 0)
				{
					bodySizeInfo = new IntVector2(1, 1);
					lineBreakSize = 1;
					iterSize = new IntVector2(1, 1);
				}
				else if (childCount == 1)
				{
					bodySizeInfo = new IntVector2(1, 1);
					lineBreakSize = 1;
					iterSize = new IntVector2(1, 1);
				}
				else
				{
					var p0I0 = container.GetChild(0);
					var lineXSize = 1;
					var lineYSize = 1;

					if (childCount >= 2)
					{
						var p1 = container.GetChild(1);
						if (gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
						{
							var xMax = p0I0.localPosition.x;
							var xSign = Math.Sign(p1.localPosition.x - xMax);
							for (var i = lineXSize; i < childCount; i++)
							{
								var child = container.GetChild(i);
								var childX = child.localPosition.x;
								if (xSign * (childX - xMax) > 0)
								{
									xMax = childX;
									lineXSize++;
								}
								else
								{
									isOk = true;
									break;
								}
							}

							lineYSize = (childCount + lineXSize - 1) / lineXSize;

							lineBreakSize = lineXSize;
							iterSize = new IntVector2(1, lineBreakSize);
						}
						else if (gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Vertical)
						{
							var yMax = p0I0.localPosition.y;
							var ySign = Math.Sign(p1.localPosition.y - yMax);
							for (var i = lineYSize; i < childCount; i++)
							{
								var child = container.GetChild(i);
								var childY = child.localPosition.y;
								if (ySign * (childY - yMax) > 0)
								{
									yMax = childY;
									lineYSize++;
								}
								else
								{
									isOk = true;
									break;
								}
							}

							lineXSize = (childCount + lineYSize - 1) / lineYSize;

							lineBreakSize = int.MaxValue;
							iterSize = new IntVector2(lineYSize, 1);
						}
						else
						{
							throw new NotImplementedException();
						}
					}
					else
					{
						iterSize = new IntVector2(1, 1);
					}

					bodySizeInfo = new IntVector2(lineXSize, lineYSize);
				}
			}
			else
			{
				throw new NotImplementedException();
			}

			gridIter.TotalCount = childCount;
			gridIter.LineBreakSize = lineBreakSize;
			gridIter.BodySizeInfo = bodySizeInfo;
			gridIter.IterSize = iterSize;
			gridIter.ScrollRectRange = this.ScrollRectRange;

			var distance = UpdateDistance(gridIter);
			gridIter.Distance = distance;

			return isOk;
		}

		protected Vector3 UpdateDistance(GridIter gridIter)
		{
			var lineBreakSize = gridIter.LineBreakSize;
			var distance = Vector3.zero;
			var childCount = ChildCountInit;
			if (childCount >= 2)
			{
				if (lineBreakSize + 2 <= childCount)
				{
					distance = container.GetChild(gridIter.ToIndex(1, 1)).localPosition -
					           container.GetChild(0).localPosition;
				}
				else if (2 <= lineBreakSize && lineBreakSize + 1 <= childCount)
				{
					var distanceX = container.GetChild(gridIter.ToIndex(1, 0)).localPosition -
					                container.GetChild(0).localPosition;
					var distanceY = container.GetChild(gridIter.ToIndex(0, 1)).localPosition -
					                container.GetChild(0).localPosition;
					distance = new Vector3(distanceX.x, distanceY.y, distanceX.z + distanceY.x);
				}
				else
				{
					distance = container.GetChild(lineBreakSize - 1).localPosition -
					           container.GetChild(0).localPosition;
				}
			}

			return distance;
		}
		
		
		protected bool IsInContainer(int index)
		{
			return IsInContainer(container.GetChild(index));
		}

		protected bool IsInContainerSafe(int index)
		{
			return 0 <= index && index < ChildCountInit && IsInContainer(container.GetChild(index));
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

		public Vector2 ToVec2(Vector3 pos)
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

		protected bool IsInContainer(Transform child)
		{
			var rect = GetRectBounds(child);
			return IsInContainer(rect);
		}

		protected bool IsInContainer(Rect rect)
		{
			var isInContainerVertical = false;
			var isInContainerHorizontal = false;
			if (scrollRect.vertical)
			{
				if (rect.yMax >= ScrollRectRange.yMin && rect.yMin <= ScrollRectRange.yMax)
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
				if (rect.xMax >= ScrollRectRange.xMin && rect.xMin <= ScrollRectRange.xMax)
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
	}
}