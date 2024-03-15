using System;
using UnityEngine;

namespace DataBind.UIBind.RecycleContainer
{
	[Serializable]
	public struct IntVector2
	{
		public IntVector2(int x0, int y0)
		{
			x = x0;
			y = y0;
		}

		public int x;
		public int y;

		public bool IsZero()
		{
			return x <= 0 && y <= 0;
		}

		public bool IsAnyNegative()
		{
			return x < 0 || y < 0;
		}

		public override string ToString()
		{
			return $"({x},{y})";
		}

		public static IntVector2 Sign(Vector3 distance)
		{
			return new IntVector2(Math.Sign(distance.x), Math.Sign(distance.y));
		}
	}
}