using UnityEngine;
using System;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	[AddComponentMenu("DataDrive/DataBindHub")]
	public abstract class DataBindCompBase : MyComponent, ICCDataBindBase
	{
		// constructor(...args: any[]) {
		// 	super(...args)
		// }

		public virtual DataBind DataBind { get; set; } = new DataBind();

		public virtual ISEventCleanInfo2<object, object> WatchValueChange<T>(string key, EventHandlerMV2<object, object> call)
		{
			return this.DataBind.WatchExprValue(key, call);
		}
		public virtual void ClearWatchers()
		{
			this.DataBind.ClearWatchers();
		}

		public virtual void DoBindItems()
		{
			this.OnBindItems();
		}

		protected abstract void OnBindItems();

		public virtual void DoUnBindItems()
		{
			this.OnUnBindItems();
		}
		protected virtual void OnUnBindItems()
		{
			this.ClearWatchers();
		}

		// /**
		//  * 获取当前组件所在节点的host
		//  */
		// public virtual CCDialogComp findDialogComp()
		// {
		// 	var node = this.transform;
		//
		// 	try
		// 	{
		// 		while (node.GetComponent<CCDialogComp>() != null)
		// 		{
		// 			node = node.parent;
		// 		}
		// 	}
		// 	catch
		// 	{
		//
		// 	}
		// 	if (!node)
		// 	{
		// 		return null;
		// 	}
		//
		// 	var dialogComponent = node.GetComponent<CCDialogComp>();
		// 	return dialogComponent;
		// }

		public virtual void Integrate()
		{
			DataBindHubHelper.OnAddDataBind(this);
		}

		public virtual void Deintegrate()
		{
			this.DataBind.Clear();
		}

		public virtual void Relate()
		{
			DataBindHubHelper.OnRelateDataBind(this);
		}

		public virtual void Derelate()
		{
			DataBindHubHelper.OnDerelateDataBind(this);
		}

		protected override void OnPreload()
		{
			this.Integrate();
		}

		protected override void OnPreDestroy()
		{
			this.Deintegrate();
		}

		protected override void OnAttach()
		{
			this.Relate();
		}

		protected override void OnDeattach()
		{
			this.Derelate();
		}

	}
}
