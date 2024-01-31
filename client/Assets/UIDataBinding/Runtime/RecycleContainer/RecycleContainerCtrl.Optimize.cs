using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataBinding.UIBind.RecycleContainer
{
	public partial class RecycleContainerCtrl
	{
		#region Optimize Scroll Performance

		/// <summary>
		/// 更新流程:
		/// - 初始化： DetectRect
		/// - 后续更新： 移动窗口 -> 计算新窗口坐标 -> UpdateRectByScroll, 获得新窗口中心（待精确） -> 遍历时, 扩展新窗口矩形 -> 重新计算精确窗口中心和Size 
		/// </summary>
		protected GridIter GridIterPre = new();

		protected GridIter GridIterNext = new();

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

			GridIterPre.Copy(ref GridIterNext);
			GridIterPre.IsPrecision = true;

			GridIterNext.NeedCheck = this.needCheck;
			GridIterPre.NeedCheck = this.needCheck;
			if (GridIterNext.NeedCheck)
			{
				InjectDebugDetails();
			}
		}

		protected void InjectDebugDetails()
		{
			#region DebugOnly

			bool DetectFunc(int index)
			{
				if (index >= ChildCountInit)
				{
					return false;
				}

				return IsInContainer(container.GetChild(index));
			}

			GridIterPre.DetectFunc = DetectFunc;
			GridIterNext.DetectFunc = DetectFunc;

			bool CheckFunc(int index)
			{
				if (index >= ChildCountInit)
				{
					return true;
				}

				var child = container.GetChild(index);
				return IsVirtualNode(child) != IsInContainer(child);
			}

			GridIterPre.CheckFunc = CheckFunc;
			GridIterNext.CheckFunc = CheckFunc;

			#endregion
		}

		protected Vector3 UpdateDistance(GridIter gridIter)
		{
			var lineBreakSize = gridIter.LineBreakSize;
			var distance = Vector3.zero;
			if (ChildCountInit >= 2)
			{
				if (lineBreakSize + 2 <= ChildCountInit)
				{
					distance = container.GetChild(gridIter.ToIndex(1, 1)).localPosition -
					           container.GetChild(0).localPosition;
				}
				else if (2 <= lineBreakSize && lineBreakSize + 1 <= ChildCountInit)
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

		protected LayoutGroup LayoutGroup;

		/// <summary>
		/// UpdateContainerBodySize
		/// </summary>
		/// <param name="gridIter"></param>
		/// <returns>return false if not update fully</returns>
		/// <exception cref="NotImplementedException"></exception>
		protected bool UpdateContainerParams(GridIter gridIter)
		{
			var isOk = false;

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
				bodySizeInfo = new IntVector2(1, ChildCountInit);
				lineBreakSize = int.MaxValue;
				iterSize = new IntVector2(1, int.MaxValue);
				isOk = true;
			}
			else if (layoutGroup is GridLayoutGroup gridLayoutGroup)
			{
				if (ChildCountInit == 0)
				{
					bodySizeInfo = new IntVector2(1, 1);
					lineBreakSize = 1;
					iterSize = new IntVector2(1, 1);
				}
				else if (ChildCountInit == 1)
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

					if (ChildCountInit >= 2)
					{
						var p1 = container.GetChild(1);
						if (gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
						{
							var xMax = p0I0.localPosition.x;
							var xSign = Math.Sign(p1.localPosition.x - xMax);
							for (var i = lineXSize; i < ChildCountInit; i++)
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

							lineYSize = (ChildCountInit + lineXSize - 1) / lineXSize;

							lineBreakSize = lineXSize;
							iterSize = new IntVector2(1, lineBreakSize);
						}
						else if (gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Vertical)
						{
							var yMax = p0I0.localPosition.y;
							var ySign = Math.Sign(p1.localPosition.y - yMax);
							for (var i = lineYSize; i < ChildCountInit; i++)
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

							lineXSize = (ChildCountInit + lineYSize - 1) / lineYSize;

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

			gridIter.TotalCount = ChildCountInit;
			gridIter.LineBreakSize = lineBreakSize;
			gridIter.BodySizeInfo = bodySizeInfo;
			gridIter.IterSize = iterSize;
			gridIter.ScrollRectRange = this.ScrollRectRange;

			var distance = UpdateDistance(gridIter);
			gridIter.Distance = distance;

			return isOk;
		}

		public void UpdateRectByScroll(Vector2 scrollPos)
		{
			Debug.Assert(GridIterNext.IsPrecision);

			var lineXSize = GridIterNext.LineXSize;
			var lineYSize = GridIterNext.LineYSize;
			var size = GridIterNext.Size;
			var bodyRect = GridIterNext.GetBodyRect();
			var moveOffset = scrollPos - GridIterNext.ScrollPos;
			var center = GridIterNext.Pos;

			var distance = GridIterNext.Distance;
			var iSign = IntVector2.Sign(distance);

			var isValidCenterFound = false;
			IntVector2 centerInContainer;
			Vector2 guessCenter;
			if (GridIterNext.Distance != Vector3.zero)
			{
				var iDir = new Vector2(moveOffset.x / distance.x, moveOffset.y / distance.y);
				guessCenter = center + iDir;
			}
			else
			{
				guessCenter = center;
			}

			// assumption 1
			bool AssumptPt(Vector2 ptCenter, out IntVector2 ptInContainer)
			{
				if (ptCenter.x > -1 && ptCenter.y > -1)
				{
					var center1 = new IntVector2(Mathf.FloorToInt(ptCenter.x),
						Mathf.FloorToInt(ptCenter.y));
					var center2 = new IntVector2(Mathf.CeilToInt(ptCenter.x),
						Mathf.FloorToInt(ptCenter.y));
					var center3 = new IntVector2(Mathf.FloorToInt(ptCenter.x),
						Mathf.CeilToInt(ptCenter.y));
					var center4 = new IntVector2(Mathf.CeilToInt(ptCenter.x),
						Mathf.CeilToInt(ptCenter.y));

					var centers = new IntVector2[4]
					{
						center1,
						center2,
						center3,
						center4,
					};
					foreach (var centerN in centers)
					{
						if (0 <= centerN.x && centerN.x < lineXSize
						                   && 0 <= centerN.y)
						{
							var pos = GridIterNext.ToIndex(centerN);
							if (IsInContainerSafe(pos))
							{
								ptInContainer = centerN;
								return true;
							}
						}
					}
				}

				ptInContainer = new IntVector2(0, 0);
				return false;
			}

			// presume center
			var ret = AssumptPt(guessCenter, out centerInContainer);
			if (ret)
			{
				isValidCenterFound = true;
			}

			if (!isValidCenterFound && bodyRect.IsBroad())
			{
				var halfSize = new Vector2(size.x * 0.5f, size.y * 0.5f);
				var fWinSize = ScrollRectRange.size;
				var iWinSize = new Vector2(fWinSize.x / Mathf.Abs(distance.x), fWinSize.y / Mathf.Abs(distance.y));
				var iMoveSign = new IntVector2(Math.Sign(moveOffset.x * iSign.x), Math.Sign(moveOffset.y * iSign.y));

				IntVector2 searchCorner0 = new IntVector2(
					(int)(center.x - halfSize.x * iMoveSign.x),
					(int)(center.y - halfSize.y * iMoveSign.y));
				searchCorner0 = bodyRect.Limit(searchCorner0);

				if (distance != Vector3.zero)
				{
					var iMoveOffset = new Vector2(moveOffset.x / distance.x, moveOffset.y / distance.y);

					// presume corner1 with offset
					var searchCorner1 = new Vector2(searchCorner0.x + iMoveOffset.x,
						searchCorner0.y + iMoveOffset.y);
					var ret1 = AssumptPt(searchCorner1, out centerInContainer);
					if (ret1)
					{
						isValidCenterFound = true;
					}
				}

				if (!isValidCenterFound)
				{
					(int, int) SortNum(int a, int b)
					{
						return (Math.Min(a, b), Math.Max(a, b));
					}

					// presume corner0 to corner2
					IntVector2 searchCorner2 = new IntVector2(0, 0);
					if (distance != Vector3.zero)
					{
						var iMoveOffset = new Vector2(moveOffset.x / distance.x, moveOffset.y / distance.y);
						searchCorner2 = new IntVector2(
							Mathf.CeilToInt(searchCorner0.x + iMoveOffset.x + (iWinSize.x + 1) * iMoveSign.x),
							Mathf.CeilToInt(searchCorner0.y + iMoveOffset.y + (iWinSize.y + 1) * iMoveSign.y));
						searchCorner2 = bodyRect.Limit(searchCorner2);
					}

					var (x1, x2) = SortNum(searchCorner0.x, searchCorner2.x);
					var (y1, y2) = SortNum(searchCorner0.y, searchCorner2.y);
					if (SearchAnyPtInContainer(x1, x2, y1, y2, iSign, out var xHit, out var yHit))
					{
						Debug.Assert(IsInContainer(GridIterNext.ToIndex(xHit, yHit)));
						centerInContainer = new IntVector2(xHit, yHit);
						isValidCenterFound = true;
					}
				}
			}

			if (!isValidCenterFound && bodyRect.IsBroad())
			{
				if (SearchAnyPtInContainer(bodyRect.xMin, bodyRect.xMax, bodyRect.yMin, bodyRect.yMax, iSign,
					    out var xHit,
					    out var yHit))
				{
					Debug.Assert(IsInContainer(GridIterNext.ToIndex(xHit, yHit)));
					centerInContainer = new IntVector2(xHit, yHit);
					isValidCenterFound = true;
				}
			}

			GridIterNext.ScrollPos = scrollPos;
			if (!isValidCenterFound)
			{
				// 未找到任何窗内元素
				GridIterNext.Size = new IntVector2(-1, -1);
				GridIterNext.IsPrecision = true;
				// Debug.LogError("isValidCenterFound");
			}
			else
			{
				// find fuzzy size
				{
					var (x1, x2, y1, y2) = DetectRectBorder(centerInContainer.x, centerInContainer.y, lineXSize,
						lineYSize, ChildCountInit);

					GridIterNext.Size = new IntVector2(x2 - x1, y2 - y1);
					GridIterNext.Pos = new Vector2((x2 + x1) * 0.5f, (y2 + y1) * 0.5f);
				}
				GridIterNext.PosSure = centerInContainer;
				GridIterNext.IsPrecision = false;
			}

			UpdateLentPool();
		}

		protected void UpdateLentPool()
		{
			var totalCount = GridIterNext.TotalCount;
			var lentPool = GridIterNext.LentPool;
			lentPool.Clear();
			foreach (var child in this.LentPool.PeekAll())
			{
				var index = child.GetSiblingIndex();
				Debug.Assert(index <= totalCount);
				lentPool.Add(index);
			}
		}

		protected bool SearchAnyPtInContainer(int x1, int x2, int y1, int y2, IntVector2 iSign,
			out int xHit, out int yHit)
		{
			Debug.Assert(x1 <= x2 && y1 <= y2);

			if (iSign.x == 0)
			{
				iSign.x = 1;
			}

			if (iSign.y == 0)
			{
				iSign.y = 1;
			}

			// 二分查找
			(int, int) Select2XY(int mode, int n1, int n2, int mid)
			{
				if (mode == -1)
				{
					return (n1, mid - 1);
				}
				else if (mode == 1)
				{
					return (mid + 1, n2);
				}
				else if (mode == 0)
				{
					return (n1, n2);
				}
				else
				{
					throw new NotImplementedException($"{mode}, {n1}, {n2}, {mid}");
				}
			}

			bool Search2XYInternal(int x1, int x2, int y1, int y2, out int xHit, out int yHit)
			{
				if (x1 > x2)
				{
					xHit = -1;
					yHit = -1;
					return false;
				}

				if (y1 > y2)
				{
					xHit = -1;
					yHit = -1;
					return false;
				}

				var xmid = Mathf.FloorToInt((x1 + x2) * 0.5f);
				var ymid = Mathf.FloorToInt((y1 + y2) * 0.5f);

				var index = GridIterNext.ToIndex(xmid, ymid);
				if (index >= ChildCountInit)
				{
					return Search2XYInternal(x1, xmid - 1, y1, y2, out xHit, out yHit)
					       || Search2XYInternal(xmid, x2, y1, ymid - 1, out xHit, out yHit);
				}

				var child = container.GetChild(index);
				var rect = GetRectBounds(child);

				int dy;
				if (rect.yMin > ScrollRectRange.yMax)
				{
					dy = -1 * iSign.y;
				}
				else if (ScrollRectRange.yMin > rect.yMax)
				{
					dy = 1 * iSign.y;
				}
				else
				{
					dy = 0;
				}

				int dx;
				if (rect.xMin > ScrollRectRange.xMax)
				{
					dx = -1 * iSign.x;
				}
				else if (ScrollRectRange.xMin > rect.xMax)
				{
					dx = 1 * iSign.x;
				}
				else
				{
					dx = 0;
				}

				if (dx == 0 && dy == 0)
				{
					// in container and in index range
					xHit = xmid;
					yHit = ymid;
					return true;
				}
				else
				{
					var xr = Select2XY(dx, x1, x2, xmid);
					var yr = Select2XY(dy, y1, y2, ymid);
					return Search2XYInternal(xr.Item1, xr.Item2, yr.Item1, yr.Item2, out xHit, out yHit);
				}
			}

			return Search2XYInternal(x1, x2, y1, y2, out xHit, out yHit);
		}

		protected (int x1, int x2, int y1, int y2) DetectRectBorder(int ax, int ay, int lineXSize, int lineYSize,
			int total)
		{
			var y0 = ay;
			var x1 = ax;
			for (var ix = x1; ix >= 0; ix--)
			{
				var ipos = GridIterNext.ToIndex(ix, y0);
				var child = container.GetChild(ipos);
				if (!IsInContainer(child))
				{
					break;
				}

				x1 = ix;
			}

			var x0 = x1;
			var y1 = ay;
			for (var iy = y1; iy >= 0; iy--)
			{
				var ipos = GridIterNext.ToIndex(x0, iy);
				var child = container.GetChild(ipos);
				if (!IsInContainer(child))
				{
					break;
				}

				y1 = iy;
			}

			y0 = y1;
			var x2 = ax;
			for (var ix = x2; ix < lineXSize; ix++)
			{
				var ipos = GridIterNext.ToIndex(ix, y0);
				if (ipos >= total)
				{
					break;
				}

				var child = container.GetChild(ipos);
				if (!IsInContainer(child))
				{
					break;
				}

				x2 = ix;
			}

			x0 = x1;
			var y2 = ay;
			for (var iy = y2; iy < lineYSize; iy++)
			{
				var ipos = GridIterNext.ToIndex(x0, iy);
				if (ipos >= total)
				{
					break;
				}

				var child = container.GetChild(ipos);
				if (!IsInContainer(child))
				{
					break;
				}

				y2 = iy;
			}

			return (x1, x2, y1, y2);
		}

		protected bool IsNearBottom()
		{
			if (OldList == null)
			{
				return false;
			}

			var detectRect = GridIterNext.GetTopIndex();
			var lineBreakSize = GridIterNext.LineBreakSize;
			var size = GridIterNext.Size;
			var nearDistance = Math.Max(1, Mathf.RoundToInt(Math.Max(size.x, size.y) * 0.5f));
			if (OldList.Count <= detectRect + lineBreakSize * nearDistance)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// 是否允许异步加载
		/// </summary>
		public bool loadAsync = false;
		protected IEnumerator UpdateItemsByTicksIter;

		protected override IEnumerator UpdateItemsByTicks(IList dataSources)
		{
			if (loadAsync)
			{
				var iter = UpdateItemsByTicksInternal(dataSources).GetEnumerator();
				using var unknown = iter as IDisposable;
				UpdateItemsByTicksIter = iter;
				while (iter.MoveNext())
				{
					yield return iter.Current;
				}

				UpdateItemsByTicksIter = null;
			}
			else
			{
				yield return base.UpdateItemsByTicks(dataSources);
			}
		}

		protected void EnsureBottomItemsEnough()
		{
			if (UpdateItemsByTicksIter != null)
			{
				var iter = UpdateItemsByTicksIter;
				if (iter.Current is WaitForEndOfFrame)
				{
					return;
				}

				var next = false;
				while (true)
				{
					next = iter.MoveNext();
					if (!next)
					{
						break;
					}

					if (!IsNearBottom())
					{
						Debug.Log($"supply once");
						break;
					}

					if (iter.Current is WaitForEndOfFrame)
					{
						break;
					}
				}

				if (!next)
				{
					UpdateItemsByTicksIter = null;
				}
			}
		}

		protected IEnumerable UpdateItemsByTicksInternal(IList dataSources)
		{
			var area = Math.Max(128, (int)GridIterNext.GetDetectRect().GetArea());
			var batch1 = area;
			foreach (var o in UpdateItemsByData(dataSources, 0, batch1, 0, batch1))
			{
				yield return o;
			}

			LayoutRebuilder.MarkLayoutForRebuild(this.container);
			yield return new WaitForEndOfFrame();
			UpdateContainerState(this.ScrollVal);
			yield return null;

			var batch2 = area;
			const int increaseRate = 3;
			const int maxStep = 2048;
			for (; batch2 < dataSources.Count; )
			{
				yield return null;

				var nextBatch = Math.Min(Math.Min(batch2 * increaseRate, batch2 + maxStep), dataSources.Count);
				Debug.Log($"preload-stand-begin: {nextBatch}");
				for (var i = batch2+StandPool.Count; i < nextBatch; i++)
				{
					if ((i & 31) == 0)
					{
						UnMarkRebuildSafe();

						yield return null;
					}

					var standNode = this.GenPlacer(i);
					StandPool.Recycle(standNode);
					// UnuseNode(standNode);
				}

				if (!IsLayoutDirty())
				{
					UnMarkRebuild();
				}

				yield return null;

				foreach (var o in UpdateItemsByData(dataSources, batch2, nextBatch, batch2, nextBatch))
				{
					yield return o;
				}

				LayoutRebuilder.MarkLayoutForRebuild(this.container);
				yield return new WaitForEndOfFrame();
				var timeStart = DateTime.Now.Millisecond;
				UpdateContainerState(this.ScrollVal);
				var timeEnd = DateTime.Now.Millisecond;
				var dt = (timeEnd - timeStart + 1000) % 1000;
				Debug.Log($"preload-stand-end, {dt}");
				yield return null;

				batch2 = nextBatch;
			}

			Debug.Assert(OldList.Count == dataSources.Count, "OldList.Count == dataSources.Count");

			Debug.Log($"load all data done");
		}

		protected void UnMarkRebuildSafe()
		{
			if (!IsLayoutDirty())
			{
				UnMarkRebuild();
			}
		}

		#endregion
	}
}