using System;
using UnityEngine;

namespace DataBind.UIBind
{
	public class DataBindHubUtils
	{
		public static void ForeachSurfChildren<T>(Transform target, Action<T> call) where T : MonoBehaviour
		{
			target.ForEachChildren((child) =>
			{
				var comps = child.GetComponents<T>();
				if (comps.Length > 0)
				{
					comps.ForEach(comp =>
					{
						call(comp);
					});
				}
				else
				{
					ForeachSurfChildren<T>(child, call);
				}
			});
		}

		public static void ForeachSurf<T>(Transform target, Action<T> call) where T : MonoBehaviour
		{
			var comps = target.GetComponents<T>();
			if (comps.Length > 0)
			{
				comps.ForEach(comp =>
				{
					call(comp);
				});
			}
			else
			{
				target.ForEachChildren(child =>
				{
					ForeachSurf(child, call);
				});
			}

		}
	}
}
