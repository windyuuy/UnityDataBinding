using System;
using UnityEngine;

namespace UIDataBinding.Runtime.RecycleContainer
{
	[Serializable]
	public struct IntRect
	{
		public override int GetHashCode()
		{
			return HashCode.Combine(xMax, xMin, yMax, yMin);
		}

		public int xMax;
		public int xMin;
		public int yMax;
		public int yMin;

		public IntRect(int x0, int y0, int x1, int y1)
		{
			this.xMin = x0;
			this.yMin = y0;
			this.xMax = x1;
			this.yMax = y1;
		}

		public override string ToString()
		{
			return $"(({xMin},{yMin}),({xMax},{yMax}))";
		}

		public void Init(Vector2 start, Vector2 end)
		{
			xMin = Mathf.RoundToInt(start.x) + 1;
			xMax = Mathf.RoundToInt(end.x) - 1;
			yMin = Mathf.RoundToInt(start.y) + 1;
			yMax = Mathf.RoundToInt(end.y) - 1;
		}

		public void Init(IntVector2 start, IntVector2 end)
		{
			xMin = start.x + 1;
			xMax = end.x - 1;
			yMin = start.y + 1;
			yMax = end.y - 1;
		}

		public long GetArea()
		{
			var dx = Math.Max(0, xMax - xMin + 1);
			var dy = Math.Max(0, yMax - yMin + 1);
			return dx * dy;
		}

		public void ExpandMax(IntVector2 pt)
		{
			xMin = Math.Min(xMin, pt.x);
			xMax = Math.Max(xMax, pt.x);
			yMin = Math.Min(yMin, pt.y);
			yMax = Math.Max(yMax, pt.y);
		}

		public void ExpandMax1Round()
		{
			++xMax;
			--xMin;
			++yMax;
			--yMin;
		}

		public void Copy(IntRect rect)
		{
			this.xMax = rect.xMax;
			this.xMin = rect.xMin;
			this.yMax = rect.yMax;
			this.yMin = rect.yMin;
		}


		public IntVector2 BL()
		{
			return new IntVector2((int)xMin, (int)yMin);
		}

		public IntVector2 TR()
		{
			return new IntVector2((int)xMax, (int)yMax);
		}

		public bool ExpandLimit(ref IntRect rangeRect, out IntRect detectRect, IntRect bodyRect)
		{
			// if rangeRect is overflow than limit0, expand detectRect base on rangeRect
			detectRect = rangeRect;
			if (rangeRect.xMin < this.xMin)
			{
				detectRect.xMin--;
				detectRect.xMin = Mathf.Clamp(detectRect.xMin, bodyRect.xMin, bodyRect.xMax);
			}

			if (rangeRect.xMax > this.xMax)
			{
				detectRect.xMax++;
				detectRect.xMax = Mathf.Clamp(detectRect.xMax, bodyRect.xMin, bodyRect.xMax);
			}

			if (rangeRect.yMin < this.yMin)
			{
				detectRect.yMin--;
				detectRect.yMin = Mathf.Clamp(detectRect.yMin, bodyRect.yMin, bodyRect.yMax);
			}

			if (rangeRect.yMax > this.yMax)
			{
				detectRect.yMax++;
				detectRect.yMax = Mathf.Clamp(detectRect.yMax, bodyRect.yMin, bodyRect.yMax);
			}

			this = rangeRect;

			var isAllLimited = detectRect == rangeRect;
			return isAllLimited;
		}

		public static bool operator ==(IntRect r1, IntRect r2)
		{
			return r1.xMin == r2.xMin
			       && r1.xMax == r2.xMax
			       && r1.yMin == r2.yMin
			       && r1.yMax == r2.yMax;
		}

		public static bool operator !=(IntRect r1, IntRect r2)
		{
			return r1.xMin != r2.xMin
			       || r1.xMax != r2.xMax
			       || r1.yMin != r2.yMin
			       || r1.yMax != r2.yMax;
		}

		public bool Equals(IntRect other)
		{
			return xMax == other.xMax && xMin == other.xMin && yMax == other.yMax && yMin == other.yMin;
		}

		public override bool Equals(object obj)
		{
			return obj is IntRect other && Equals(other);
		}

		public Vector2 Center()
		{
			return new Vector2((xMax + xMin) * 0.5f, (yMax + yMin) * 0.5f);
		}

		public IntVector2 Size()
		{
			return new IntVector2(Mathf.RoundToInt(xMax - xMin), Mathf.RoundToInt(yMax - yMin));
		}

		public void BeLimit(IntRect detectedRect)
		{
			if (xMin < detectedRect.xMin)
			{
				xMin = detectedRect.xMin;
			}

			if (xMax > detectedRect.xMax)
			{
				xMax = detectedRect.xMax;
			}

			if (yMin < detectedRect.yMin)
			{
				yMin = detectedRect.yMin;
			}

			if (yMax > detectedRect.yMax)
			{
				yMax = detectedRect.yMax;
			}
		}

		// public void Split(IntRect detectedRect, out int count, out IntRect[] mainRect)
		// {
		// 	float nx, x1, x2, x3, x4;
		// 	if (xMin < detectedRect.xMin)
		// 	{
		// 		if (xMax > detectedRect.xMax)
		// 		{
		// 			nx = 4;
		// 			x1 = xMin;
		// 			x2 = detectedRect.xMin - 1;
		// 			x3 = detectedRect.xMax + 1;
		// 			x4 = xMax;
		// 		}
		// 		else
		// 		{
		// 			nx = 2;
		// 			x1 = xMin;
		// 			x2 = detectedRect.xMin - 1;
		// 		}
		// 	}
		// 	else
		// 	{
		// 		if (xMax > detectedRect.xMax)
		// 		{
		// 			nx = 2;
		// 			x1 = detectedRect.xMax+1;
		// 			x2 = xMax;
		// 		}
		// 		else
		// 		{
		// 			nx = 0;
		// 		}
		// 	}
		// 	
		// 	float ny, y1, y2, y3, y4;
		// 	if (yMin < detectedRect.yMin)
		// 	{
		// 		if (yMax > detectedRect.yMax)
		// 		{
		// 			ny = 4;
		// 			y1 = yMin;
		// 			y2 = detectedRect.yMin - 1;
		// 			y3 = detectedRect.yMax + 1;
		// 			y4 = yMax;
		// 		}
		// 		else
		// 		{
		// 			ny = 2;
		// 			y1 = yMin;
		// 			y2 = detectedRect.yMin - 1;
		// 		}
		// 	}
		// 	else
		// 	{
		// 		if (yMax > detectedRect.yMax)
		// 		{
		// 			ny = 2;
		// 			y1 = detectedRect.yMax+1;
		// 			y2 = yMax;
		// 		}
		// 		else
		// 		{
		// 			ny = 0;
		// 		}
		// 	}
		// 	
		// 	var nxy=nx+ny;
		// 	int na = nxy switch
		// 	{
		// 		// 0+0
		// 		0 => 1,
		// 		// 0+2
		// 		2 => 1,
		// 		// 0+4, 2+2
		// 		4 => 2,
		// 		// 4+2
		// 		6 => 3,
		// 		// 4+4
		// 		8 => 8,
		// 		_ => throw new NotImplementedException(),
		// 	};
		// 	
		// 	mainRect=new IntRect[na];
		// 	
		// }
		public bool Contains(float ix, float iy)
		{
			return xMin <= ix && ix <= xMax
			                  && yMin <= iy && iy <= yMax;
		}
		public bool Contains(IntVector2 pt)
		{
			return xMin <= pt.x && pt.x <= xMax
			                  && yMin <= pt.y && pt.y <= yMax;
		}

		public bool ContainsAll(IntRect detectedRect)
		{
			return xMin <= detectedRect.xMin && detectedRect.xMax <= xMax
			                                 && yMin <= detectedRect.yMin && detectedRect.yMax <= yMax;
		}

		public void ShrinkMax1Round()
		{
			--xMax;
			++xMin;
			--yMax;
			++yMin;
		}

		public IntVector2 Limit(IntVector2 searchCorner1)
		{
			searchCorner1.x = Math.Clamp(searchCorner1.x, this.xMin, this.xMax);
			searchCorner1.y = Math.Clamp(searchCorner1.y, this.yMin, this.yMax);
			return searchCorner1;
		}

		public bool IsNarrow()
		{
			return this.xMin > this.xMax || this.yMin > this.yMax;
		}

		public bool IsBroad()
		{
			return this.xMin <= this.xMax || this.yMin <= this.yMax;
		}
	}
}