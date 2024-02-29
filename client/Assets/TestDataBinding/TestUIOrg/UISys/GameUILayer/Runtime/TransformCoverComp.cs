using System;
using System.Collections.Generic;
using UnityEngine;

namespace UISys.Runtime
{
	public interface ITransformCoverHandler
	{
		public void OnTransformShield();
		public void OnTransformExpose();
	}
	[AddComponentMenu("UISys/TransformCover")]
	public class TransformCoverComp : MonoBehaviour
	{
		[SerializeField] protected bool isCover;

		public bool IsCover => isCover;

		[SerializeField] protected bool ignoreCover;
		public bool IgnoreCover => ignoreCover;

		[NonSerialized] protected HashSet<ITransformCoverHandler> Handlers = new();

		public void Register(ITransformCoverHandler handler)
		{
			Handlers.Add(handler);
		}

		public void UnRegister(ITransformCoverHandler handler)
		{
			Handlers.Remove(handler);
		}

		public void DispatchTransformPause()
		{
			foreach (var transformCoverHandler in this.Handlers)
			{
				transformCoverHandler.OnTransformShield();
			}
		}
		public void DispatchTransformResume()
		{
			foreach (var transformCoverHandler in this.Handlers)
			{
				transformCoverHandler.OnTransformExpose();
			}
		}
	}
}