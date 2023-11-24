
using System;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	using number = System.Double;

	public class ContainerBind
	{
		public DataBindHub bindHub;
		public Object rawObj;
		public virtual void AddBindHub(IDataBindHub bindHub1)
		{
			this.AddBindHub((DataBindHub)bindHub1);
		}
		public virtual void AddBindHub(DataBindHub bindHub1)
		{
			this.bindHub = bindHub1;
		}
		public virtual void RemoveBindHub(IDataBindHub bindHub1)
		{
			this.RemoveBindHub((DataBindHub)bindHub1);
		}
		public virtual void RemoveBindHub(DataBindHub bindHub1)
		{
			if (bindHub1 == null || this.bindHub == bindHub1)
			{
				this.bindHub = null;
			}
		}

		public string expr;
		protected ISEventCleanInfo2<object, object> exprWatcher;

		public void BindExpr(string expr0)
		{
			if (this.expr != expr0)
			{
				this.UnbindExpr();
			}
			if (expr0 != null)
			{
				this.expr = expr0;
				if (this.bindHub != null)
				{
					var bindHub1 = this.bindHub;
					EventHandlerMV2<object, object> onValueChanged = (object v1, object v2) =>
					 {
						 this.onDataChangedEvent.emit(v1, v2);

						 var bindList1 = this.bindList;
						 foreach (var oid in bindList1.Keys)
						 {
							 var item = bindList1[oid];
							 if (item.RealDataHub != null)
							 {
								 var ls = v1 as IStdHost[];
								 // 确认是否需要换成 observeData
								 item.RealDataHub.SetDataHost(ls[(int)item.Index]);
							 }
						 }
					 };
					this.exprWatcher = bindHub1.WatchExprValue(expr0, onValueChanged);
					if (this.exprWatcher != null)
					{
						bindHub1.DoWatchNewExpr(expr0, onValueChanged);
						bindHub1.SyncExprValue(expr0, onValueChanged);
					}

				}
			}
		}

		public void UnbindExpr()
		{
			if (this.exprWatcher != null)
			{
				if (this.bindHub != null)
				{
					var bindHub1 = this.bindHub;
					bindHub1.UnWatchExprValue(this.exprWatcher.key, this.exprWatcher.callback);
					bindHub1.DoUnWatchExprOnce(this.expr);
				}
				this.exprWatcher = null;
			}
		}

		protected readonly Dictionary<number, ContainerItem> bindList = new Dictionary<number, ContainerItem>();
		public void BindItem(ContainerItem item)
		{
			this.bindList[item.Oid] = item;
		}

		public void UnbindItem(ContainerItem item)
		{
			if (this.bindList.ContainsKey(item.Oid))
			{
				if (item.RealDataHub != null)
				{
					item.RealDataHub.UnsetDataHost();
				}
				this.bindList.Remove(item.Oid);
			}
		}

		protected readonly SimpleEventMV2<object,object> onDataChangedEvent = new SimpleEventMV2<object,object>();


		protected List<EventHandlerMV2<object,object>> watcherList = new List<EventHandlerMV2<object,object>>();

		public EventHandlerMV2<object,object> WatchList(EventHandlerMV2<object,object> call)
		{
			var watcher = this.onDataChangedEvent.On(call);
			this.watcherList.Add(watcher);
			return watcher;
		}

		public void UnWatchList(EventHandlerMV2<object,object> watcher)
		{
			this.onDataChangedEvent.Off(watcher);
			this.watcherList.Remove(watcher);
		}

		public void UnWatchLists()
		{
			this.watcherList.ForEach(watcher =>
			{
				this.onDataChangedEvent.Off(watcher);
			});
			// this.watcherList.clear();
			this.watcherList = null;
		}

	}
}
