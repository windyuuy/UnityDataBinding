using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIDataBinding.Runtime.RecycleContainer
{
	public interface ILayoutRectIter
	{
		public IEnumerable<int> GetForwardIter(GridIter preGridIter, Func<bool> iterInput);
		public IEnumerable<int> GetBackwardIter(GridIter preGridIter, Func<bool> iterInput);
	}

	public class GridIter : ILayoutRectIter
	{
		/// <summary>
		/// center
		/// </summary>
		public Vector2 Pos;

		public IntVector2 PosSure;

		public IntVector2 Size;

		/// <summary>
		/// (BodySizeMain, BodySizeX)
		/// </summary>
		public IntVector2 BodySizeInfo;

		public int LineBreakSize;
		public int LineXSize => BodySizeInfo.x;

		public int LineYSize => BodySizeInfo.y;

		public IntRect GetBodyRect()
		{
			return new IntRect
			{
				xMax = this.LineXSize - 1,
				xMin = 0,
				yMax = this.LineYSize - 1,
				yMin = 0,
			};
		}

		public IntVector2 IterSize;
		public int TotalCount;
		public Vector2 ScrollPos;
		public Vector3 Distance;
		public bool IsPrecision;

		public bool IsEmpty()
		{
			return this.IsPrecision && this.Size.IsAnyNegative();
		}

		public GridIter()
		{
		}

		public GridIter(Vector2 pos, IntVector2 size, int totalCount, Vector2 scrollPos)
		{
			Pos = pos;
			Size = size;
			TotalCount = totalCount;
			ScrollPos = scrollPos;
			IsPrecision = false;
		}

		public static GridIter FromCorners(IntVector2 pos1, IntVector2 pos2, int countMax, Vector2 scrollPos)
		{
			var center = new Vector2(((pos1.x + pos2.x) * 0.5f),
				((pos1.y + pos2.y) * 0.5f));
			var size = new IntVector2(pos2.x - pos1.x, pos2.y - pos1.y);
			return new GridIter(center, size, countMax, scrollPos);
		}

		public int IterIndex = 0;
		public int IterAcc = 0;
		public bool NeedCheck = false;

		public IEnumerable<int> GetForwardIter(GridIter preGridIter, Func<bool> iterInput)
		{
			var iterIndex = IterIndex;
			var isPrecision = this.IsPrecision;
			// Debug.Log($"PosSure: {PosSure}");

			List<(int i, bool b, bool b2)> checkList0;
			if (NeedCheck)
			{
				checkList0 = new();
				// if (iterIndex == 1)
				{
					var childCountInit = this.TotalCount;
					for (var i = 0; i < childCountInit; i++)
					{
						var b = DetectFunc(i);
						var b2 = CheckFunc(i);
						checkList0.Add((i, b, b2));
					}
				}
			}

			var bodySizeRect = new IntRect
			{
				xMax = LineXSize - 1,
				xMin = 0,
				yMax = this.LineYSize - 1,
				yMin = 0,
			};

			var rangeRect = new IntRect
			{
				xMax = -1,
				xMin = 1,
				yMax = -1,
				yMin = 1
			};

			var detectRect = this.GetDetectRect();

			Dictionary<int, bool> checkDict = null;

			void CheckIndex(int index)
			{
				if (!NeedCheck)
				{
					return;
				}

				checkDict ??= new Dictionary<int, bool>();
				if (checkDict.ContainsKey(index))
				{
					Debug.LogError("duplicate index: " + index);
				}
				else
				{
					checkDict.Add(index, true);
				}
			}

			var isEmpty = this.IsEmpty();
			if (!isEmpty)
			{
				// init with any point in container
				rangeRect.Init(PosSure, PosSure);

				if (isPrecision)
				{
					detectRect.BeLimit(bodySizeRect);
					for (var iy = detectRect.yMin; iy <= detectRect.yMax; iy++)
					{
						for (var ix = detectRect.xMin; ix <= detectRect.xMax; ix++)
						{
							var ii = this.ToIndex(ix, iy);
							if (ii >= TotalCount)
							{
								continue;
							}

							CheckIndex(ii);
							yield return ii;
							var isInContainer = iterInput();
							if (isInContainer)
							{
								rangeRect.ExpandMax(new IntVector2(ix, iy));
							}
						}
					}
				}
				else
				{
					detectRect.ExpandMax1Round();
					detectRect.BeLimit(bodySizeRect);
					for (var iy = detectRect.yMin; iy <= detectRect.yMax; iy++)
					{
						for (var ix = detectRect.xMin; ix <= detectRect.xMax; ix++)
						{
							var ii = this.ToIndex(ix, iy);
							if (ii >= TotalCount)
							{
								continue;
							}

							CheckIndex(ii);
							yield return ii;
							var isInContainer = iterInput();
							if (isInContainer)
							{
								rangeRect.ExpandMax(new IntVector2(ix, iy));
							}
						}
					}
				}

				Debug.Assert(rangeRect.GetArea() > 0);
				if (rangeRect.GetArea() <= 0)
				{
					Debug.LogError("lkwej");
				}
			}

			// detect previours rect left
			Debug.Assert(preGridIter.IsPrecision);
			if (!preGridIter.IsEmpty())
			{
				var preDetectRect = preGridIter.GetDetectRect();
				// preDetectRect.ExpandMax1Round();
				// preDetectRect.BeLimit(bodySizeRect);
				// preDetectRect.Split(detectedRect);

				if (!detectRect.ContainsAll(preDetectRect))
				{
					for (var iy = preDetectRect.yMin; iy <= preDetectRect.yMax; iy++)
					{
						for (var ix = preDetectRect.xMin; ix <= preDetectRect.xMax; ix++)
						{
							if (!detectRect.Contains(ix, iy))
							{
								var ii = preGridIter.ToIndex(ix, iy);
								if (ii >= TotalCount)
								{
									continue;
								}

								CheckIndex(ii);
								yield return ii;
							}
						}
					}
				}
			}

			IterAcc++;
			if (IterAcc == 2 && NeedCheck)
			{
				var childCountInit = this.TotalCount;
				for (var i = 0; i < childCountInit; i++)
				{
					var b = CheckFunc(i);
					if (!b)
					{
						Debug.LogError($"global-CheckFunc-Failed1: {i}");
					}
				}

				for (var iy = rangeRect.yMin; iy <= rangeRect.yMax; iy++)
				{
					for (var ix = rangeRect.xMin; ix <= rangeRect.xMax; ix++)
					{
						var ipos = this.ToIndex(ix, iy);
						var b = CheckFunc(ipos);
						if (!b)
						{
							Debug.LogError($"rangeRect-CheckFunc-Failed2: ({ix},{iy})");
							break;
						}
					}
				}

				var preDetectRect = preGridIter.GetDetectRect();

				if (preDetectRect != rangeRect)
				{
					Debug.Log($"diff: {preDetectRect} -> {detectRect} -> {rangeRect}");
				}

				for (var iy = preDetectRect.yMin; iy <= preDetectRect.yMax; iy++)
				{
					for (var ix = preDetectRect.xMin; ix <= preDetectRect.xMax; ix++)
					{
						var ipos = this.ToIndex(ix, iy);
						var b = CheckFunc(ipos);
						if (!b)
						{
							Debug.LogError($"preDetectRect-CheckFunc-Failed3: ({ix},{iy})");
							break;
						}
					}
				}

				for (var iy = bodySizeRect.yMin; iy < bodySizeRect.yMax; iy++)
				{
					for (var ix = Math.Max(rangeRect.xMax, preDetectRect.xMax) + 1; ix <= bodySizeRect.xMax; ix++)
					{
						var ipos = this.ToIndex(ix, iy);
						var b = DetectFunc(ipos);
						if (b)
						{
							Debug.LogError($"bodySizeRect-Detect-Failed4: ({ix},{iy})");
							break;
						}
					}
				}
			}

			if (iterIndex == 1)
			{
				IterHis1.Insert(0, rangeRect);
			}

			if (iterIndex == 2)
			{
				IterHis2.Insert(0, rangeRect);
			}

			this.Pos = rangeRect.Center();
			this.Size = rangeRect.Size();
			this.IsPrecision = true;
		}

		public static List<IntRect> IterHis1 = new();
		public static List<IntRect> IterHis2 = new();

		public Func<int, bool> DetectFunc;
		public Func<int, bool> CheckFunc;

		public IntRect GetDetectRect()
		{
			if (Size.IsAnyNegative())
			{
				return new IntRect
				{
					xMax = Mathf.RoundToInt(Pos.x - 1),
					xMin = Mathf.RoundToInt(Pos.x + 1),
					yMax = Mathf.RoundToInt(Pos.y - 1),
					yMin = Mathf.RoundToInt(Pos.y + 1),
				};
			}

			return new IntRect
			{
				xMax = Mathf.RoundToInt(Pos.x + Size.x * 0.5f),
				xMin = Mathf.RoundToInt(Pos.x - Size.x * 0.5f),
				yMax = Mathf.RoundToInt(Pos.y + Size.y * 0.5f),
				yMin = Mathf.RoundToInt(Pos.y - Size.y * 0.5f),
			};
		}

		public IEnumerable<int> GetBackwardIter(GridIter preGridIter, Func<bool> iterInput)
		{
			// TODO: implement reverse
			return GetForwardIter(preGridIter, iterInput);
		}

		public int ToIndex(int x, int y)
		{
			return x * IterSize.x + y * IterSize.y;
		}

		public int ToIndex(IntVector2 vec)
		{
			return vec.x * IterSize.x + vec.y * IterSize.y;
		}

		public void Copy(ref GridIter gridIter)
		{
			this.Pos = gridIter.Pos;
			this.PosSure = gridIter.PosSure;
			this.Size = gridIter.Size;
			this.BodySizeInfo = gridIter.BodySizeInfo;
			this.LineBreakSize = gridIter.LineBreakSize;
			this.IterSize = gridIter.IterSize;
			this.TotalCount = gridIter.TotalCount;
			this.ScrollPos = gridIter.ScrollPos;
			this.Distance = gridIter.Distance;
			this.IsPrecision = gridIter.IsPrecision;
		}

		// public IntVector2 BL()
		// {
		// 	return new IntVector2((int)(Pos.x - Size.x * 0.5f), (int)(Pos.y - Size.y * 0.5f));
		// }
		//
		// public IntVector2 TL()
		// {
		// 	return new IntVector2((int)(Pos.x - Size.x * 0.5f), (int)(Pos.y + Size.y * 0.5f));
		// }
		//
		// public IntVector2 BR()
		// {
		// 	return new IntVector2((int)(Pos.x + Size.x * 0.5f), (int)(Pos.y - Size.y * 0.5f));
		// }
		//
		// public IntVector2 TR()
		// {
		// 	return new IntVector2((int)(Pos.x + Size.x * 0.5f), (int)(Pos.y + Size.y * 0.5f));
		// }
	}
}