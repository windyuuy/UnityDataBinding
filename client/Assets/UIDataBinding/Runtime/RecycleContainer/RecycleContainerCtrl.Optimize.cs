using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace UIDataBinding.Runtime.RecycleContainer
{
	public partial class RecycleContainerCtrl
	{
		#region Optimize Scroll Performance

		/// <summary>
		/// 更新流程:
		/// - 初始化： DetectRect
		/// - 后续更新： 移动窗口 -> 计算新窗口坐标 -> UpdateRectByScroll, 获得新窗口中心（待精确） -> 遍历时, 扩展新窗口矩形 -> 重新计算精确窗口中心和Size 
		/// </summary>
		private GridIter _gridIter = new();

		public void DetectRect(Vector2 val0)
		{
			UpdateContainerBodySize(out var bodySize);

			// create grid iters
			if (bodySize == 0)
			{
				// no element
				_gridIterNext = GridIter.FromCorners(new IntVector2(0, 0), new IntVector2(-2, -2), bodySize,
					_childCountInit, Vector3.zero,
					val0);
				_gridIterNext.IsPrecision = true;
			}
			else
			{
				var bodyHeight = (_childCountInit + bodySize - 1) / bodySize;
				var distance = UpdateDistance(bodySize);
				var pSign = IntVector2.Sign(distance);

				if (
					SearchAnyPtInContainer(0, 2, 0, 2, bodySize, pSign, out var xHit, out var yHit)
				||SearchAnyPtInContainer(0, bodySize, 0, bodyHeight, bodySize, pSign, out  xHit, out  yHit)
					)
				{
					var (x1, x2, y1, y2) = DetectRectBorder(xHit, yHit, bodySize, bodyHeight);
					
					var p1 = new IntVector2(x1,y1);
					var p2 = new IntVector2(x2, y2);
					_gridIterNext = GridIter.FromCorners(p1, p2, bodySize, _childCountInit, distance, val0);
				}
				else
				{
					_gridIterNext = new GridIter(new Vector2(xHit, yHit), new IntVector2(-2, -2), bodySize,
						_childCountInit, distance, val0);
					_gridIterNext.IsPrecision = true;
				}

			}

			_gridIter.Copy(ref _gridIterNext);
			_gridIter.IsPrecision = true;

			InjectDebugDetails();
		}

		[Conditional("UNITY_EDITOR")]
		protected void InjectDebugDetails()
		{
			#region DebugOnly

			bool DetectFunc(int index)
			{
				if (index >= _childCountInit)
				{
					return false;
				}

				return IsInContainer(_container.GetChild(index));
			}

			_gridIter.DetectFunc = DetectFunc;
			_gridIterNext.DetectFunc = DetectFunc;

			bool CheckFunc(int index)
			{
				if (index >= _childCountInit)
				{
					return true;
				}

				var child = _container.GetChild(index);
				return IsVirtualNode(child) != IsInContainer(child);
			}

			_gridIter.CheckFunc = CheckFunc;
			_gridIterNext.CheckFunc = CheckFunc;
			
			#endregion
		}

		/// <summary>
		/// UpdateContainerBodySize
		/// </summary>
		/// <param name="bodySize"></param>
		/// <returns>return false if not update fully</returns>
		/// <exception cref="NotImplementedException"></exception>
		protected bool UpdateContainerBodySize(out int bodySize)
		{
			var isOk = false;
			
			var container = _container.GetComponent<LayoutGroup>();
			if (container is VerticalLayoutGroup)
			{
				bodySize = 1;
				isOk = true;
			}
			else if (container is HorizontalLayoutGroup)
			{
				bodySize = int.MaxValue;
				isOk = true;
			}
			else if (container is GridLayoutGroup)
			{
				if (_childCountInit == 0)
				{
					bodySize = 0;
				}
				else
				{
					bodySize = 1;
					var p0I0 = _container.GetChild(0);
					float xMax = p0I0.localPosition.x;
					for (var i = bodySize; i < _childCountInit; i++)
					{
						var child = _container.GetChild(i);
						var childX = child.localPosition.x;
						if (childX >= xMax)
						{
							xMax = childX;
							bodySize++;
						}
						else
						{
							bodySize = i;
							isOk = true;
							break;
						}
					}
				}
			}
			else
			{
				throw new NotImplementedException();
			}

			return isOk;
		}

		protected Vector3 UpdateDistance(int bodySize)
		{
			if (bodySize == 1)
			{
				var distance = Vector3.zero;
				if (bodySize < _childCountInit)
				{
					distance = _container.GetChild(bodySize).localPosition - _container.GetChild(0).localPosition;
				}

				return distance;
			}
			else if (bodySize == int.MaxValue)
			{
				var distance = Vector3.zero;
				if (1 < _childCountInit)
				{
					distance = _container.GetChild(1).localPosition - _container.GetChild(0).localPosition;
				}

				return distance;
			}
			else
			{
				var distance = Vector3.zero;
				if (bodySize + 1 < _childCountInit)
				{
					distance = _container.GetChild(bodySize + 1).localPosition - _container.GetChild(0).localPosition;
				}
				else if (bodySize < _childCountInit)
				{
					distance = _container.GetChild(bodySize).localPosition - _container.GetChild(0).localPosition;
				}

				return distance;
			}
		}

		private GridIter _gridIterNext = new();

		public void UpdateRectByScroll(Vector2 scrollPos)
		{
			Debug.Assert(_gridIterNext.IsPrecision);

			// update dynamic params at first
			{
				if (_gridIterNext.CountMax != _childCountInit)
				{
					_gridIterNext.CountMax = _childCountInit;

					UpdateContainerBodySize(out var newBodySize);
					_gridIterNext.BodySize = newBodySize;
					// re-update distance once
					var distance2 = UpdateDistance(_gridIterNext.BodySize);
					_gridIterNext.Distance = distance2;
				}
				else
				{
					if (_gridIterNext.Distance == Vector3.zero)
					{
						// re-update distance once
						var distance2 = UpdateDistance(_gridIterNext.BodySize);
						_gridIterNext.Distance = distance2;
					}
				}
			}
			
			var bodySize = _gridIterNext.BodySize;
			var bodyHeight = _gridIterNext.GetMaxRowCount();
			var size = _gridIterNext.Size;
			var bodyRect = new IntRect(0, 0, bodySize - 1, bodyHeight - 1);
			var dir = scrollPos - _gridIterNext.ScrollPos;
			var center = _gridIterNext.Pos;

			var distance = _gridIterNext.Distance;
			var pSign = IntVector2.Sign(distance);

			var isValidCenterFound = false;
			IntVector2 centerInContainer;
			Vector2 guessCenter;
			if (_gridIterNext.Distance != Vector3.zero)
			{
				// TODO: 适配不同模式
				var pDir = new Vector2(dir.x / distance.x, dir.y / distance.y);
				guessCenter = center + pDir;
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
						if (0 <= centerN.x && centerN.x < bodySize
						                   && 0 <= centerN.y)
						{
							var pos = GridIter.ToIndex(centerN, bodySize);
							if (0 <= pos && pos < _childCountInit)
							{
								if (IsInContainer(_container.GetChild(pos)))
								{
									ptInContainer = centerN;
									return true;
								}
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

			if (!isValidCenterFound && bodyRect.IsExtensive())
			{
				var pDirSign = new IntVector2(Math.Sign(dir.x), Math.Sign(-dir.y));
				// TODO: 适配不同模式
				var halfSize = new Vector2(size.x * 0.5f, size.y * 0.5f);
				var winSizeF = _scrollRectRange.size;
				var winSize = new Vector2(winSizeF.x / Mathf.Abs(distance.x), winSizeF.y / Mathf.Abs(distance.y));

				IntVector2 searchCorner1 = new IntVector2(
					(int)(center.x - halfSize.x * pDirSign.x),
					(int)(center.y - halfSize.y * pDirSign.y));
				searchCorner1 = bodyRect.Limit(searchCorner1);

				IntVector2 searchCorner2 = new IntVector2(0, 0);
				if (distance != Vector3.zero)
				{
					var pDir = new Vector2(dir.x / distance.x, dir.y / distance.y);

					// presume corner1 with offset
					var searchCorner1Assumpt = new Vector2(searchCorner1.x + pDir.x * pDirSign.x,
						searchCorner1.y + pDir.y * pDirSign.y);
					var ret1 = AssumptPt(searchCorner1Assumpt, out centerInContainer);
					if (ret1)
					{
						isValidCenterFound = true;
					}
					else
					{
						searchCorner2 = new IntVector2(
							Mathf.CeilToInt(searchCorner1.x + (pDir.x + winSize.x + 1) * pDirSign.x),
							Mathf.CeilToInt(searchCorner1.y + (pDir.y + winSize.y + 1) * pDirSign.y));
						searchCorner2 = bodyRect.Limit(searchCorner2);
					}
				}

				if (!isValidCenterFound)
				{
					(int, int) SortNum(int a, int b)
					{
						return (Math.Min(a, b), Math.Max(a, b));
					}

					{
						var (x1, x2) = SortNum(searchCorner1.x, searchCorner2.x);
						var (y1, y2) = SortNum(searchCorner1.y, searchCorner2.y);
						if (SearchAnyPtInContainer(x1, x2, y1, y2, bodySize,pSign,out var xHit, out var yHit))
						{
							Debug.Assert(IsInContainer(GridIter.ToIndex(xHit, yHit, bodySize)));
							centerInContainer = new IntVector2(xHit, yHit);
							isValidCenterFound = true;
						}
					}
				}
			}
			
			if (!isValidCenterFound && bodyRect.IsExtensive())
			{
				if (SearchAnyPtInContainer(bodyRect.xMin, bodyRect.xMax, bodyRect.yMin, bodyRect.yMax,bodySize,pSign, out var xHit,
					    out var yHit))
				{
					Debug.Assert(IsInContainer(GridIter.ToIndex(xHit, yHit, bodySize)));
					centerInContainer = new IntVector2(xHit, yHit);
					isValidCenterFound = true;
				}
			}

			_gridIterNext.ScrollPos = scrollPos;
			if (!isValidCenterFound)
			{
				// 未找到任何窗内元素
				_gridIterNext.Size = new IntVector2(-1, -1);
				_gridIterNext.IsPrecision = true;
				// Debug.LogError("isValidCenterFound");
			}
			else
			{
				// find fuzzy size
				{
					var (x1, x2, y1, y2) = DetectRectBorder(centerInContainer.x,centerInContainer.y, bodySize, bodyHeight);

					_gridIterNext.Size = new IntVector2(x2 - x1, y2 - y1);
					_gridIterNext.Pos = new Vector2((x2 + x1) * 0.5f, (y2 + y1) * 0.5f);
				}
				_gridIterNext.PosSure = centerInContainer;
				_gridIterNext.IsPrecision = false;
			}
		}

		protected bool SearchAnyPtInContainer(int x1, int x2, int y1, int y2, int bodySize, IntVector2 pSign, out int xHit, out int yHit)
		{
			Debug.Assert(x1 <= x2 && y1 <= y2);

			if (pSign.x == 0)
			{
				pSign.x = 1;
			}

			if (pSign.y == 0)
			{
				pSign.y = 1;
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

				var index = GridIter.ToIndex(xmid, ymid, bodySize);
				if (index >= _childCountInit)
				{
					return Search2XYInternal(x1, xmid - 1, y1, y2, out xHit, out yHit)
					       || Search2XYInternal(xmid, x2, y1, ymid - 1, out xHit, out yHit);
				}

				var child = _container.GetChild(index);
				var rect = GetRectBounds(child);

				int dy;
				if (rect.yMin > _scrollRectRange.yMax)
				{
					dy = -1 * pSign.y;
				}
				else if (_scrollRectRange.yMin > rect.yMax)
				{
					dy = 1 * pSign.y;
				}
				else
				{
					dy = 0;
				}

				int dx;
				if (rect.xMin > _scrollRectRange.xMax)
				{
					dx = -1 * pSign.x;
				}
				else if (_scrollRectRange.xMin > rect.xMax)
				{
					dx = 1 * pSign.x;
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

		protected (int x1, int x2, int y1, int y2) DetectRectBorder(int ax,int ay, int bodySize, int bodyHeight)
		{
			var y0 = ay;
			var x1 = ax;
			for (var ix = x1; ix >= 0; ix--)
			{
				var ipos = GridIter.ToIndex(ix, y0, bodySize);
				var child = _container.GetChild(ipos);
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
				var ipos = GridIter.ToIndex(x0, iy, bodySize);
				var child = _container.GetChild(ipos);
				if (!IsInContainer(child))
				{
					break;
				}

				y1 = iy;
			}

			y0 = y1;
			var x2 = ax;
			for (var ix = x2; ix < bodySize; ix++)
			{
				var ipos = GridIter.ToIndex(ix, y0, bodySize);
				var child = _container.GetChild(ipos);
				if (!IsInContainer(child))
				{
					break;
				}

				x2 = ix;
			}

			x0 = x1;
			var y2 = ay;
			for (var iy = y2; iy < bodyHeight; iy++)
			{
				var ipos = GridIter.ToIndex(x0, iy, bodySize);
				var child = _container.GetChild(ipos);
				if (!IsInContainer(child))
				{
					break;
				}

				y2 = iy;
			}

			return (x1, x2, y1, y2);
		}

		#endregion
	}
}