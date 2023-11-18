
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	using number = System.Double;

	using TWatchExprCall = EventHandlerMV2<string, EventHandlerMV2<object, object>>;
	using TExprCall = EventHandlerMV2<object, object>;

	public class DataBindHub : IDataBindHub, IDataBindHubTree
	{

		protected static number oidAcc = 0;
		public number oid = ++DataBindHub.oidAcc;

		public Object rawObj;

		public Dictionary<string, number> watchingExprs = new Dictionary<string, number>();

		public List<IDataBindHubTree> parents { get; set; } = new List<IDataBindHubTree>();
		public IDataBindHubTree parent
		{
			get
			{
				if (this.parents.Count > 0)
				{
					return this.parents[0];
				}
				else
				{
					return null;
				}
			}
			set
			{
				if (this.parent != value)
				{
					if (this.parent != null)
					{
						var parent = this.parent;
						this.parents.Remove(parent);
						parent.removeBindHub(this);
					}
					if (value != null)
					{
						value.addBindHub(this);
					}
				}
			}
		}

		public List<DataBindHub> bindHubs = new List<DataBindHub>();

		public void removeParents()
		{
			while (this.parents.Count > 0)
			{
				this.parent = null;
			}
		}
		public void removeChildren()
		{
			while (this.bindHubs.Count > 0)
			{
				var bindHub = this.bindHubs[0];
				this.removeBindHub(bindHub);
			}
		}

		protected EventHandlerMV2<string, EventHandlerMV2<object, object>> _onWatchNewExprCall;
		protected EventHandlerMV2<string, EventHandlerMV2<object, object>> _onUnWatchExprCall;
		public EventHandlerMV3<string, object, object> _onValueChanged;
		public DataBindHub()
		{
			this._onWatchNewExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.doWatchNewExpr(expr, call);
			};
			this._onUnWatchExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.doUnWatchExprOnce(expr);
			};
			this._onValueChanged = (string expr, object value, object oldValue) =>
			{
				this.emitExprValueChanged(expr, value, oldValue);
			};
		}

		public void addBindHub(IDataBindHub bindHub)
		{
			addBindHub((DataBindHub)bindHub);
		}
		public void addBindHub(DataBindHub bindHub)
		{
			if (!this.bindHubs.Contains(bindHub))
			{
				this.bindHubs.Add(bindHub);
				Debug.Assert(bindHub.parents.Count == 0);
				bindHub.parents.Add(this);
				bindHub.onWatchNewExpr(this._onWatchNewExprCall);
				bindHub.onUnWatchExpr(this._onUnWatchExprCall);
				this.onAnyValueChanged(bindHub._onValueChanged);
				foreach (var expr in bindHub.watchingExprs.Keys)
				{
					EventHandlerMV2<object, object> call = (object value, object oldValue) =>
					{
						this.emitExprValueChanged(expr, value, oldValue);
					};
					this.doWatchNewExpr(expr, call);
					this.syncExprValue(expr, call);
				}
			}
		}

		public void removeBindHub(IDataBindHub bindHub)
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
						this.doUnWatchExprOnce(expr);
					}
				}
				bindHub.offWatchNewExpr(this._onWatchNewExprCall);
				bindHub.offUnWatchExpr(this._onUnWatchExprCall);
				this.offAnyValueChanged(bindHub._onValueChanged);
				bindHub.parents.Remove(this);
				this.bindHubs.Remove(bindHub);
			}
		}

		protected SimpleEventMV2<string, EventHandlerMV2<object, object>> newExprEvent = new SimpleEventMV2<string, EventHandlerMV2<object, object>>();
		protected SimpleEventMV2<string, EventHandlerMV2<object, object>> removeExprEvent = new SimpleEventMV2<string, EventHandlerMV2<object, object>>();

		/**
		 * 监听未监听过的接口
		 * @param expr 
		 */
		public TWatchExprCall onWatchNewExpr(TWatchExprCall call)
		{
			return this.newExprEvent.on(call);
		}
		public void offWatchNewExpr(TWatchExprCall call)
		{
			this.newExprEvent.off(call);
		}
		/**
		 * 不再监听某个接口
		 * @param expr 
		 */
		public TWatchExprCall onUnWatchExpr(TWatchExprCall call)
		{
			return this.removeExprEvent.on(call);
		}
		public void offUnWatchExpr(TWatchExprCall call)
		{
			this.removeExprEvent.off(call);
		}

		/**
		 * 立即同步表达式的值
		 * @param expr 
		 * @param call 
		 * @returns 
		 */
		public void syncExprValue(string expr, TExprCall call)
		{
			this.parents.ForEach(parent => parent.syncExprValue(expr, call));
		}

		/**
		 * 监听未监听过的接口
		 * @param expr 
		 */
		public void doWatchNewExpr(string expr, EventHandlerMV2<object, object> call)
		{
			this.watchingExprs[expr] = this.watchingExprs.ContainsKey(expr) ? this.watchingExprs[expr] : 0;
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
		public void doUnWatchExprOnce(string expr)
		{
			this.watchingExprs[expr] = this.watchingExprs.ContainsKey(expr) ? this.watchingExprs[expr] : 0;
			this.watchingExprs[expr]--;



			if (this.watchingExprs[expr] == 0)
			{
				this.removeExprEvent.emit(expr, null);
			}
		}

		public EventHandlerMV3<string, object, object> onAnyValueChanged(EventHandlerMV3<string, object, object> call)
		{
			return this.listeners.onAnyEvent(call);
		}
		public void offAnyValueChanged(EventHandlerMV3<string, object, object> call)
		{
			this.listeners.offAnyEvent(call);
		}

		protected SEventMV2<object, object> listeners = new SEventMV2<object, object>();
		public ISEventCleanInfo2<object, object> watchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			return this.listeners.on(expr, call);
		}
		public void unWatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			this.listeners.off(expr, call);
		}
		protected void emitExprValueChanged(string expr, object value, object oldValue)
		{
			this.listeners.emit(expr, value, oldValue);
		}

		public void clear()
		{
			this.removeParents();
			this.removeChildren();
		}

		public ISEventCleanInfo2<object, object> easeWatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			var bindHub = this;

			var watcher = bindHub.watchExprValue(expr, call);
			if (watcher != null)
			{
				bindHub.doWatchNewExpr(expr, call);
				bindHub.syncExprValue(expr, call);
			}
			return watcher;
		}

		public void easeUnWatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			var bindHub = this;
			bindHub.unWatchExprValue(expr, call);


			bindHub.doUnWatchExprOnce(expr);
		}

	}
}
