using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataBind;
using UnityEngine;
using Object = System.Object;

namespace DataBind.UIBind
{
	public class ContainerBind: IDataSourceDispatcher
	{
		public DataBindHub BindHub { get; set; }

		public Object RawObj { get; set; }
		public virtual void AddBindHub(IDataBindHub bindHub1)
		{
			this.AddBindHub((DataBindHub)bindHub1);
		}
		public virtual void AddBindHub(DataBindHub bindHub1)
		{
			this.BindHub = bindHub1;
			bindHub1.DataBindPumps.Add(this);
		}
		public virtual void RemoveBindHub(IDataBindHub bindHub1)
		{
			this.RemoveBindHub((DataBindHub)bindHub1);
		}
		public virtual void RemoveBindHub(DataBindHub bindHub1)
		{
			if (bindHub1 == null || this.BindHub == bindHub1)
			{
				if (bindHub1 != null)
				{
					bindHub1.DataBindPumps.Remove(this);
				}
				this.BindHub = null;
			}
		}

		public string Expr;
		protected ISEventCleanInfo2<object, object> ExprWatcher;

		public void BindExpr(string expr0)
		{
			if (this.Expr != expr0)
			{
				this.UnbindExpr();
			}
			if (expr0 != null)
			{
				this.Expr = expr0;
				if (this.BindHub != null)
				{
					var bindHub1 = this.BindHub;
					EventHandlerMV2<object, object> onValueChanged = (object value, object oldValue) =>
					{
						 this.OnDataChangedEvent.Emit(value, oldValue);

						 if (value is IList ls)
						 {
							 UpdateValidItems();
							 var bindList1 = this.BindList;
							 foreach (var oid in bindList1.Keys)
							 {
								 var item = bindList1[oid];
								 if (item.RealDataHub != null && item.Index<ls.Count)
								 {
									 // 确认是否需要换成 observeData
									 item.RealDataHub.SetDataHost((IStdHost)ls[item.Index]);
								 }
							 }
						 }
						 else
						 {
							 Debug.LogException(new Exception("invalid list data"));
						 }
					};
					this.ExprWatcher = bindHub1.WatchExprValue(expr0, onValueChanged);
					if (this.ExprWatcher != null)
					{
						bindHub1.DoWatchNewExpr(expr0, onValueChanged);
						bindHub1.SyncExprValue(expr0, onValueChanged);
					}

				}
			}
		}

		protected void UpdateValidItems()
		{
			var invalids=BindList.Where(item => item.Value == null);
			// ReSharper disable once PossibleMultipleEnumeration
			if (invalids.Any())
			{
				// ReSharper disable once PossibleMultipleEnumeration
				foreach (var item in invalids.ToArray())
				{
					BindList.Remove(item.Key);
				}
			}
		}

		public void UnbindExpr()
		{
			if (this.ExprWatcher != null)
			{
				if (this.BindHub != null)
				{
					var bindHub1 = this.BindHub;
					bindHub1.UnWatchExprValue(this.ExprWatcher.Key, this.ExprWatcher.Callback);
					bindHub1.DoUnWatchExprOnce(this.Expr);
				}
				this.ExprWatcher = null;
			}
		}

		protected readonly Dictionary<long, ContainerItem> BindList = new Dictionary<long, ContainerItem>();
		public void BindItem(ContainerItem item)
		{
			this.BindList[item.Oid] = item;
		}

		public void UnbindItem(ContainerItem item)
		{
			if (this.BindList.ContainsKey(item.Oid))
			{
				if (item.RealDataHub != null)
				{
					item.RealDataHub.UnsetDataHost();
				}
				this.BindList.Remove(item.Oid);
			}
		}

		protected readonly SimpleEventMV2<object,object> OnDataChangedEvent = new SimpleEventMV2<object,object>();


		protected List<EventHandlerMV2<object,object>> WatcherList = new List<EventHandlerMV2<object,object>>();

		public EventHandlerMV2<object,object> WatchList(EventHandlerMV2<object,object> call)
		{
			var watcher = this.OnDataChangedEvent.On(call);
			this.WatcherList.Add(watcher);
			return watcher;
		}

		public void UnWatchList(EventHandlerMV2<object,object> watcher)
		{
			this.OnDataChangedEvent.Off(watcher);
			this.WatcherList.Remove(watcher);
		}

		public void UnWatchLists()
		{
			this.WatcherList.ForEach(watcher =>
			{
				this.OnDataChangedEvent.Off(watcher);
			});
			// this.watcherList.clear();
			this.WatcherList = null;
		}

		public IEnumerable<DataSourceHub> GetDataSourceHubs()
		{
			return BindList.Select(bind => bind.Value.RealDataHub);
		}
	}
}
