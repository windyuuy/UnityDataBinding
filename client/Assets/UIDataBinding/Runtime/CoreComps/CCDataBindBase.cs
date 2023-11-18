using UnityEngine;
using System;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	public class CCDataBindBase : CCMyComponent, ICCDataBindBase
	{
		// constructor(...args: any[]) {
		// 	super(...args)
		// }

		public virtual DataBind dataBind { get; set; } = new DataBind();

		public virtual ISEventCleanInfo2<object, object> watchValueChange<T>(string key, EventHandlerMV2<object, object> call)
		{
			return this.dataBind.watchExprValue(key, call);
		}
		public virtual void clearWatchers()
		{
			this.dataBind.clearWatchers();
		}

		public virtual void doBindItems()
		{
			this.onBindItems();
		}
		protected virtual void onBindItems()
		{

		}

		public virtual void doUnBindItems()
		{
			this.onUnBindItems();
		}
		protected virtual void onUnBindItems()
		{
			this.clearWatchers();
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

		public virtual void integrate()
		{
			DataBindHubHelper.onAddDataBind(this);
		}

		public virtual void deintegrate()
		{
			this.dataBind.clear();
		}

		public virtual void relate()
		{
			DataBindHubHelper.onRelateDataBind(this);
		}

		public virtual void derelate()
		{
			DataBindHubHelper.onDerelateDataBind(this);
		}

		protected override void onPreload()
		{
			this.integrate();
		}

		protected override void onPreDestroy()
		{
			this.deintegrate();
		}

		protected override void onAttach()
		{
			this.relate();
		}

		protected override void onDeattach()
		{
			this.derelate();
		}

	}
}
