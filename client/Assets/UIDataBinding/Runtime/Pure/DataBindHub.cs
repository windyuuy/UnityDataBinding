
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	using TWatchExprCall = EventHandlerMV2<string, EventHandlerMV2<object, object>>;
	using TExprCall = EventHandlerMV2<object, object>;

	public class DataBindHub : IDataBindHub, IDataBindHubTree
	{

		protected static long oidAcc = 0;
		public long Oid = ++DataBindHub.oidAcc;

		public Object RawObj;

		public readonly Dictionary<string, long> watchingExprs = new Dictionary<string, long>();

		public List<IDataBindHubTree> Parents { get; set; } = new List<IDataBindHubTree>();
		public IDataBindHubTree Parent
		{
			get
			{
				if (this.Parents.Count > 0)
				{
					return this.Parents[0];
				}
				else
				{
					return null;
				}
			}
			set
			{
				if (this.Parent != value)
				{
					if (this.Parent != null)
					{
						var parent = this.Parent;
						this.Parents.Remove(parent);
						parent.RemoveBindHub(this);
					}
					if (value != null)
					{
						value.AddBindHub(this);
					}
				}
			}
		}

		public readonly List<DataBindHub> bindHubs = new List<DataBindHub>();

		public void RemoveParents()
		{
			while (this.Parents.Count > 0)
			{
				this.Parent = null;
			}
		}
		public void RemoveChildren()
		{
			while (this.bindHubs.Count > 0)
			{
				var bindHub = this.bindHubs[0];
				this.removeBindHub(bindHub);
			}
		}

		protected readonly EventHandlerMV2<string, EventHandlerMV2<object, object>> onWatchNewExprCall;
		protected readonly EventHandlerMV2<string, EventHandlerMV2<object, object>> onUnWatchExprCall;
		public readonly EventHandlerMV3<string, object, object> onValueChanged;
		public DataBindHub()
		{
			this.onWatchNewExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.DoWatchNewExpr(expr, call);
			};
			this.onUnWatchExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.DoUnWatchExprOnce(expr);
			};
			this.onValueChanged = (string expr, object value, object oldValue) =>
			{
				this.EmitExprValueChanged(expr, value, oldValue);
			};
		}

		public void AddBindHub(IDataBindHub bindHub)
		{
			addBindHub((DataBindHub)bindHub);
		}
		public void addBindHub(DataBindHub bindHub)
		{
			if (!this.bindHubs.Contains(bindHub))
			{
				this.bindHubs.Add(bindHub);
				Debug.Assert(bindHub.Parents.Count == 0);
				bindHub.Parents.Add(this);
				bindHub.OnWatchNewExpr(this.onWatchNewExprCall);
				bindHub.OnUnWatchExpr(this.onUnWatchExprCall);
				this.OnAnyValueChanged(bindHub.onValueChanged);
				foreach (var expr in bindHub.watchingExprs.Keys)
				{
					EventHandlerMV2<object, object> call = (object value, object oldValue) =>
					{
						this.EmitExprValueChanged(expr, value, oldValue);
					};
					this.DoWatchNewExpr(expr, call);
					this.SyncExprValue(expr, call);
				}
			}
		}

		public void RemoveBindHub(IDataBindHub bindHub)
		{
			removeBindHub((DataBindHub)bindHub);
		}
		public void removeBindHub(DataBindHub bindHub)
		{
			if (this.bindHubs.Contains(bindHub))
			{
				var watchingExprs = bindHub.watchingExprs;
				foreach (var expr in watchingExprs.Keys)
				{
					if (watchingExprs[expr] > 0)
					{
						this.DoUnWatchExprOnce(expr);
					}
				}
				bindHub.OffWatchNewExpr(this.onWatchNewExprCall);
				bindHub.OffUnWatchExpr(this.onUnWatchExprCall);
				this.OffAnyValueChanged(bindHub.onValueChanged);
				bindHub.Parents.Remove(this);
				this.bindHubs.Remove(bindHub);
			}
		}

		protected SimpleEventMV2<string, EventHandlerMV2<object, object>> newExprEvent = new SimpleEventMV2<string, EventHandlerMV2<object, object>>();
		protected SimpleEventMV2<string, EventHandlerMV2<object, object>> removeExprEvent = new SimpleEventMV2<string, EventHandlerMV2<object, object>>();

		/**
		 * 监听未监听过的接口
		 * @param expr 
		 */
		public TWatchExprCall OnWatchNewExpr(TWatchExprCall call)
		{
			return this.newExprEvent.On(call);
		}
		public void OffWatchNewExpr(TWatchExprCall call)
		{
			this.newExprEvent.Off(call);
		}
		/**
		 * 不再监听某个接口
		 * @param expr 
		 */
		public TWatchExprCall OnUnWatchExpr(TWatchExprCall call)
		{
			return this.removeExprEvent.On(call);
		}
		public void OffUnWatchExpr(TWatchExprCall call)
		{
			this.removeExprEvent.Off(call);
		}

		/**
		 * 立即同步表达式的值
		 * @param expr 
		 * @param call 
		 * @returns 
		 */
		public void SyncExprValue(string expr, TExprCall call)
		{
			this.Parents.ForEach(parent => parent.SyncExprValue(expr, call));
		}

		/**
		 * 监听未监听过的接口
		 * @param expr 
		 */
		public void DoWatchNewExpr(string expr, EventHandlerMV2<object, object> call)
		{
			this.watchingExprs[expr] = this.watchingExprs.TryGetValue(expr, out var watchingExpr) ? watchingExpr : 0;
			this.watchingExprs[expr]++;

			if (this.watchingExprs[expr] == 1)
			{
				this.newExprEvent.emit(expr, call);
			}
		}
		/**
		 * 不监听某个接口
		 * @param expr 
		 */
		public void DoUnWatchExprOnce(string expr)
		{
			this.watchingExprs[expr] = this.watchingExprs.TryGetValue(expr, out var watchingExpr) ? watchingExpr : 0;
			this.watchingExprs[expr]--;



			if (this.watchingExprs[expr] == 0)
			{
				this.removeExprEvent.emit(expr, null);
			}
		}

		public EventHandlerMV3<string, object, object> OnAnyValueChanged(EventHandlerMV3<string, object, object> call)
		{
			return this.listeners.OnAnyEvent(call);
		}
		public void OffAnyValueChanged(EventHandlerMV3<string, object, object> call)
		{
			this.listeners.OffAnyEvent(call);
		}

		protected readonly SEventMV2<object, object> listeners = new SEventMV2<object, object>();
		public ISEventCleanInfo2<object, object> WatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			return this.listeners.On(expr, call);
		}
		public void UnWatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			this.listeners.Off(expr, call);
		}
		protected void EmitExprValueChanged(string expr, object value, object oldValue)
		{
			this.listeners.Emit(expr, value, oldValue);
		}

		public void Clear()
		{
			this.RemoveParents();
			this.RemoveChildren();
		}

		public ISEventCleanInfo2<object, object> EaseWatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			var bindHub = this;

			var watcher = bindHub.WatchExprValue(expr, call);
			if (watcher != null)
			{
				bindHub.DoWatchNewExpr(expr, call);
				bindHub.SyncExprValue(expr, call);
			}
			return watcher;
		}

		public void EaseUnWatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			var bindHub = this;
			bindHub.UnWatchExprValue(expr, call);


			bindHub.DoUnWatchExprOnce(expr);
		}

	}
}
