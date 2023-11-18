using System;
using UnityEngine;

namespace DataBinding.UIBind
{
	public class DataBindHubUtils
	{
		public static void foreachSurfChildren<T>(Transform target, Action<T> call) where T : MonoBehaviour
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
					foreachSurfChildren<T>(child, call);
				}
			});
		}

		public static void foreachSurf<T>(Transform target, Action<T> call) where T : MonoBehaviour
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
					foreachSurf(child, call);
				});
			}

		}
	}
}
