using System.Collections.Generic;
using UnityEngine;

namespace DataBind.UIBind
{
	/**
	 * 确定CCContainerBind的具体绑定行为
	 */
	public class ContainerCtrl : MyComponent, ICCContainerCtrl
	{

		[Rename("使用容器选项覆盖")] public bool overrideWithContainerOptions = true;

		public virtual void Integrate()
		{
			this.SendMessage("InitContainer", SendMessageOptions.DontRequireReceiver);
			// var ccContainerBind = this.GetComponent<ContainerBindComp>();
			// if (ccContainerBind)
			// {
			// 	if (this.overrideWithContainerOptions)
			// 	{
			// 		this.bindChildren = ccContainerBind.bindChildren;
			// 	}
			//
			// 	if (this.bindChildren)
			// 	{
			// 		this.ForEachChildren(child =>
			// 		{
			// 			var ccDataHost = child.GetOrAddComponent<DataHostComp>();
			// 			ccDataHost.Integrate();
			// 		});
			// 	}
			// }
		}

		protected EventHandlerMV2<object, object> Watcher;

		public virtual void Relate()
		{
			var ccContainerBind = this.GetComponent<ContainerBindComp>();
			if (ccContainerBind != null)
			{
				this.Watcher = ccContainerBind.ContainerBind.WatchList((object newValue, object oldValue) =>
				{
					var dataSources = ((System.Collections.IList)newValue);
					// OnDataChanged(dataSources);
					this.SendMessage("OnDataChanged", dataSources, SendMessageOptions.DontRequireReceiver);
				});
			}
		}

		// protected abstract void OnDataChanged(System.Collections.IList dataSources);
		
		public virtual void Derelate()
		{
			var ccContainerBind = this.GetComponent<ContainerBindComp>();
			if (ccContainerBind)
			{
				if (this.Watcher != null)
				{
					ccContainerBind.ContainerBind.UnWatchList(this.Watcher);
				}
			}
		}

		protected override void OnPreDestroy()
		{
			this.Derelate();
		}
	}
}