
using System;
using System.Linq;
using System.Collections.Generic;

namespace UI.DataBinding
{
	using number = System.Double;

	public class ContainerBind
	{
		public DataBindHub bindHub;
		public Object rawObj;
		public void addBindHub(DataBindHub bindHub)
		{
			this.bindHub = bindHub;
		}
		public void removeBindHub(DataBindHub bindHub)
		{
			if (bindHub == null || this.bindHub == bindHub)
			{
				this.bindHub = null;
			}
		}

		public string expr;
		protected ISEventCleanInfo2<object, object> exprWatcher;

		public void bindExpr(string expr)
		{
			if (this.expr != expr)
			{
				this.unbindExpr();
			}
			if (expr != null)
			{
				this.expr = expr;
				if (this.bindHub != null)
				{
					var bindHub = this.bindHub;
					EventHandlerMV2<object, object> onValueChanged = (object v1, object v2) =>
					 {
						 this.onDataChangedEvent.emit(v1, v2);

						 var bindList = this.bindList;
						 foreach (var oid in bindList.Keys)
						 {
							 var item = bindList[oid];
							 if (item.realDataHub != null)
							 {
								 var ls = v1 as vm.IHost[];
								 // 确认是否需要换成 observeData
								 item.realDataHub.setDataHost(ls[(int)item.index]);
							 }
						 }
					 };
					this.exprWatcher = bindHub.watchExprValue(expr, onValueChanged);
					if (this.exprWatcher != null)
					{
						bindHub.doWatchNewExpr(expr, onValueChanged);
						bindHub.syncExprValue(expr, onValueChanged);
					}

				}
			}
		}

		public void unbindExpr()
		{
			if (this.exprWatcher != null)
			{
				if (this.bindHub != null)
				{
					var bindHub = this.bindHub;
					bindHub.unWatchExprValue(this.exprWatcher.key, this.exprWatcher.callback);
					bindHub.doUnWatchExprOnce(this.expr);
				}
				this.exprWatcher = null;
			}
		}

		protected Dictionary<number, ContainerItem> bindList = new Dictionary<number, ContainerItem>();
		public void bindItem(ContainerItem item)
		{
			this.bindList[item.oid] = item;
		}

		public void unbindItem(ContainerItem item)
		{
			if (this.bindList.ContainsKey(item.oid))
			{
				if (item.realDataHub != null)
				{
					item.realDataHub.unsetDataHost();
				}
				this.bindList.Remove(item.oid);
			}
		}

		protected SimpleEventMV<object> onDataChangedEvent = new SimpleEventMV<object>();


		protected List<EventHandlerMV<object>> watcherList = new List<EventHandlerMV<object>>();

		public EventHandlerMV<object> watchList(EventHandlerMV<object> call)
		{
			var watcher = this.onDataChangedEvent.on(call);
			this.watcherList.Add(watcher);
			return watcher;
		}

		public void unWatchList(EventHandlerMV<object> watcher)
		{
			this.onDataChangedEvent.off(watcher);
			this.watcherList.Remove(watcher);
		}

		public void unWatchLists()
		{
			this.watcherList.ForEach(watcher =>
			{
				this.onDataChangedEvent.off(watcher);
			});
			// this.watcherList.clear();
			this.watcherList = null;
		}

	}
}
