
using System;
using System.Linq;
using System.Linq.Ext;
using System.Diagnostics;
using System.Collections.Generic;
using DataBinding;
using Game.Diagnostics.IO;

namespace DataBinding.UIBind
{
	using number = System.Double;

	using TExprCall = EventHandlerMV2<object, object>;
	using TWatchExprCall = EventHandlerMV2<string, EventHandlerMV2<object, object>>;

	public class TPendingInfo
	{
		public string expr;
		public object newValue;
		public object oldValue;

		public TPendingInfo()
		{

		}

		public TPendingInfo(string expr, object newValue, object oldValue)
		{
			this.expr = expr;
			this.newValue = newValue;
			this.oldValue = oldValue;
		}

		public TPendingInfo Clone()
		{
			var cl = new TPendingInfo();
			cl.expr = this.expr;
			cl.newValue = this.newValue;
			cl.oldValue = this.oldValue;
			return cl;
		}
	}
	/**
	 * 适配 DataHost, 提供更易用的接口规范
	 */
	public class DataSourceHub : IDataBindHubTree
	{

		// public static readonly NoneOldValue = Symbol("NoneOldValue");

		public IStdHost dataHost = null;
		public Object rawObj;

		protected EventHandlerMV2<string, EventHandlerMV2<object, object>> _onWatchNewExprCall;
		protected EventHandlerMV2<string, EventHandlerMV2<object, object>> _onUnWatchExprCall;
		public DataSourceHub()
		{
			this._onWatchNewExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.doWatchNewExpr(expr, call);
			};
			this._onUnWatchExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.doUnWatchExprOnce(expr);
			};
		}

		public void observeData(object data)
		{
			var d = vm.Utils.implementStdHost(data);
			if(d==null||d is IStdHost)
            {
				this.setDataHost(d);
            }
            else
            {
				throw new InvalidCastException("cannot cast data to IStdHost");
            }
		}

		public void setDataHost(IStdHost dataHost)
		{
			this.replaceDataHost(dataHost);
		}

		public void unsetDataHost()
		{
			if (this.dataHost == null)
			{
				return;
			}

			var dataHost0 = this.dataHost;
			this._unsetDataHost();


			if (dataHost0 != this.dataHost)
			{
				this.emitValueChangedEvent("&this", this.dataHost, dataHost0);
			}
		}
		protected void _unsetDataHost()
		{
			foreach (var expr in this.watcherList.Keys)
			{
				var watcher = this.watcherList[expr];
				watcher.teardown();
			}
			this.watcherList.Clear();

			this.dataHost = null;
		}

		protected void replaceDataHost(IStdHost dataHost)
		{
			if (this.dataHost == dataHost)
			{
				return;
			}

			var dataHost0 = this.dataHost;
			this._unsetDataHost();


			this.dataHost = dataHost;
			if (dataHost0 != dataHost)
			{
				this.emitValueChangedEvent("&this", dataHost, dataHost0);
			}
			if (dataHost != null)
			{
				foreach (var expr in this.watchingExprs.Keys)
				{
					var acc = this.watchingExprs[expr];
					if (acc >= 1)
					{
						try
						{
							this._doWatchNewExpr(expr, (value, oldValue) =>
							{
								this.emitValueChangedEvent(expr, value, oldValue);


							});
						}
						catch (Exception e)
						{
							console.error(e);
						}
					}
				}
			}
		}

		public DataBindHub bindHub = null;
		public void addBindHub(IDataBindHub bindHub)
		{
			addBindHub((DataBindHub)bindHub);
		}
		public void addBindHub(DataBindHub bindHub)
		{
			if (this.bindHub != bindHub)
			{
				if (this.bindHub != null)
				{
					this.removeBindHub(this.bindHub);
				}

				if (bindHub != null)
				{
					Debug.Assert(this.bindHub == null);
					this.bindHub = bindHub;
					Debug.Assert(bindHub.parents.Count == 0);
					bindHub.parents.Add(this);


					bindHub.onWatchNewExpr(this._onWatchNewExprCall);
					bindHub.onUnWatchExpr(this._onUnWatchExprCall);
					this.onValueChanged(bindHub._onValueChanged);
					foreach (var expr in bindHub.watchingExprs.Keys)
					{
						this.doWatchNewExpr(expr, (value, oldValue) =>
						{
							this.emitValueChangedEvent(expr, value, oldValue);


						});
					}
				}
			}

		}

		public void removeBindHub(IDataBindHub bindHub)
		{
			removeBindHub((DataBindHub)bindHub);
		}
		public void removeBindHub(DataBindHub bindHub)
		{
			if (bindHub != null && this.bindHub == bindHub)
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
				this.offValueChanged(bindHub._onValueChanged);
				bindHub.parents.Remove(this);
				this.bindHub = null;
			}
		}

		/**
		 * 立即同步表达式的值
		 * @param expr
		 * @param call
		 * @returns
		 */
		public void syncExprValue(string expr, TExprCall call)
		{
			if (expr == "&this")
			{
				call(this.dataHost, null);
			}
			else
			{
				if (this.watcherList.ContainsKey(expr))
				{
					var watcher = this.watcherList[expr];
					if (watcher != null)
					{
						call(watcher.value, null);
					}
				}
			}
		}

		protected SimpleEventMV3<string, object, object> valueChangedEvent = new SimpleEventMV3<string, object, object>();
		public EventHandlerMV3<string, object, object> onValueChanged(EventHandlerMV3<string, object, object> call)
		{
			return this.valueChangedEvent.on(call);
		}
		public void offValueChanged(EventHandlerMV3<string, object, object> call)
		{
			this.valueChangedEvent.off(call);
		}

		protected Dictionary<string, number> watchingExprs = new Dictionary<string, number>();

		protected Dictionary<string, vm.Watcher> watcherList = new Dictionary<string, vm.Watcher>();
		protected Dictionary<string, TPendingInfo> pendingInfoMergedCache = new Dictionary<string, TPendingInfo>();

		protected bool _running = true;
		public bool running
		{
			get
			{
				return this._running;
			}
			set
			{
				this._running = value;
				if (value && this.pendingInfo.Count > 0)
				{
					var pendingInfoCopy = this.pendingInfo;
					var pendingInfoMerged = this.pendingInfoMergedCache;
					pendingInfoMerged.Clear();
					foreach (var info in pendingInfoCopy)
					{
						var expr = info.expr;
						var v = info.newValue;
						var oldValue = info.oldValue;
						var infoMerged = pendingInfoMerged.TryGet(expr);
						if (infoMerged == null)
						{
							infoMerged = info.Clone();
							pendingInfoMerged[expr] = infoMerged;
						}
						else
						{
							infoMerged.newValue = v;
						}
					}
					this.pendingInfo.Clear();

					foreach (var entry in pendingInfoMerged)
					{
						var expr = entry.Value.expr;
						var v = entry.Value.newValue;
						var oldValue = entry.Value.oldValue;
						this.emitValueChangedEvent(expr, v, oldValue);
					}
				}
			}
		}

		protected List<TPendingInfo> pendingInfo = new List<TPendingInfo>();
		protected void emitValueChangedEvent<T>(string expr, T value, T oldValue)
		{
			if (this.running)
			{
				this.valueChangedEvent.emit(expr, value, oldValue);
			}
			else
			{
				this.pendingInfo.Add(new TPendingInfo(expr, value, oldValue));
			}
		}

		protected void _doWatchNewExpr(string expr, EventHandlerMV2<object, object> call)
		{
			if (expr == "&this")
			{
				this.emitValueChangedEvent(expr, this.dataHost, null);
			}
			else
			{
				vm.Watcher watcher;
				if(this.dataHost == null)
                {
					watcher = null;
                }
                else
                {
					watcher = this.dataHost.Watch(expr, (host, value, oldValue) =>
					{
						if (this.watchingExprs[expr] > 0)
						{
							this.emitValueChangedEvent(expr, value, oldValue);
						}
					}, null, false);
				}
				if (watcher != null)
				{
					if (this.watchingExprs[expr] > 0)
					{
						this.watcherList[expr] = watcher;
						// 需要额外通知自身变化
						this.emitValueChangedEvent(expr, watcher.value, null);
					}
				}
			}
		}
		/**
		 * 监听未监听过的接口
		 * @param expr 
		 */
		protected void doWatchNewExpr(string expr, EventHandlerMV2<object, object> call)
		{
			this.watchingExprs[expr] = this.watchingExprs.ContainsKey(expr) ? this.watchingExprs[expr] : 0;
			this.watchingExprs[expr]++;

			if (this.watcherList.ContainsKey(expr) == false)
			{
				// if (expr == "&this") {
				// 	this.emitValueChangedEvent(expr, this.dataHost, null)
				// } else {
				// 	var watcher = this.dataHost?.$watch(expr, (value, oldValue) => {
				// 		if (this.watchingExprs[expr] > 0) {
				// 			this.emitValueChangedEvent(expr, value, oldValue)
				// 		}
				// 	})
				// 	if (watcher) {
				// 		this.watcherList[expr] = watcher
				// 		// 需要额外通知自身变化
				// 		this.emitValueChangedEvent(expr, watcher.value, null)
				// 	}
				// }
				this._doWatchNewExpr(expr, call);
			}
			else
			{
				// var watcher = this.watcherList[expr]
				// if (watcher) {
				// 	var value = watcher.value
				// 	call(value, value)
				// }
			}
		}
		/**
		 * 不监听某个接口
		 * @param expr 
		 */
		protected void doUnWatchExprOnce(string expr)
		{
			this.watchingExprs[expr] = this.watchingExprs.ContainsKey(expr) ? this.watchingExprs[expr] : 0;
			this.watchingExprs[expr]--;
			Debug.Assert(this.watchingExprs[expr] >= 0);
		}

	}
}
