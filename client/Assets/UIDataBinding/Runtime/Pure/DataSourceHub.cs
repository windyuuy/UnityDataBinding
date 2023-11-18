
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

		protected readonly EventHandlerMV2<string, EventHandlerMV2<object, object>> onWatchNewExprCall;
		protected readonly EventHandlerMV2<string, EventHandlerMV2<object, object>> onUnWatchExprCall;
		public DataSourceHub()
		{
			this.onWatchNewExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.DoWatchNewExpr(expr, call);
			};
			this.onUnWatchExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.DoUnWatchExprOnce(expr);
			};
		}

		public void ObserveData(object data)
		{
			var d = vm.Utils.implementStdHost(data);
			if(d==null||d is IStdHost)
            {
				this.SetDataHost(d);
            }
            else
            {
				throw new InvalidCastException("cannot cast data to IStdHost");
            }
		}

		public void SetDataHost(IStdHost dataHost)
		{
			this.ReplaceDataHost(dataHost);
		}

		public void UnsetDataHost()
		{
			if (this.dataHost == null)
			{
				return;
			}

			var dataHost0 = this.dataHost;
			this._unsetDataHost();


			if (dataHost0 != this.dataHost)
			{
				this.EmitValueChangedEvent("&this", this.dataHost, dataHost0);
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

		protected void ReplaceDataHost(IStdHost dataHost)
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
				this.EmitValueChangedEvent("&this", dataHost, dataHost0);
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
								this.EmitValueChangedEvent(expr, value, oldValue);


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
		public void AddBindHub(IDataBindHub bindHub)
		{
			AddBindHub((DataBindHub)bindHub);
		}
		public void AddBindHub(DataBindHub bindHub0)
		{
			if (this.bindHub != bindHub0)
			{
				if (this.bindHub != null)
				{
					this.RemoveBindHub(this.bindHub);
				}

				if (bindHub0 != null)
				{
					Debug.Assert(this.bindHub == null);
					this.bindHub = bindHub0;
					Debug.Assert(bindHub0.Parents.Count == 0);
					bindHub0.Parents.Add(this);


					bindHub0.OnWatchNewExpr(this.onWatchNewExprCall);
					bindHub0.OnUnWatchExpr(this.onUnWatchExprCall);
					this.OnValueChanged(bindHub0.onValueChanged);
					foreach (var expr in bindHub0.watchingExprs.Keys)
					{
						this.DoWatchNewExpr(expr, (value, oldValue) =>
						{
							this.EmitValueChangedEvent(expr, value, oldValue);


						});
					}
				}
			}

		}

		public void RemoveBindHub(IDataBindHub bindHub)
		{
			RemoveBindHub((DataBindHub)bindHub);
		}
		public void RemoveBindHub(DataBindHub bindHub0)
		{
			if (bindHub0 != null && this.bindHub == bindHub0)
			{
				var watchingExprs1 = bindHub0.watchingExprs;
				foreach (var expr in watchingExprs1.Keys)
				{
					if (watchingExprs1[expr] > 0)
					{
						this.DoUnWatchExprOnce(expr);
					}
				}
				bindHub0.OffWatchNewExpr(this.onWatchNewExprCall);
				bindHub0.OffUnWatchExpr(this.onUnWatchExprCall);
				this.OffValueChanged(bindHub0.onValueChanged);
				bindHub0.Parents.Remove(this);
				this.bindHub = null;
			}
		}

		/**
		 * 立即同步表达式的值
		 * @param expr
		 * @param call
		 * @returns
		 */
		public void SyncExprValue(string expr, TExprCall call)
		{
			if (expr == "&this")
			{
				call(this.dataHost, null);
			}
			else
			{
				if (this.watcherList.TryGetValue(expr, out var watcher))
				{
					if (watcher != null)
					{
						call(watcher.value, null);
					}
				}
			}
		}

		protected readonly SimpleEventMV3<string, object, object> valueChangedEvent = new SimpleEventMV3<string, object, object>();
		public EventHandlerMV3<string, object, object> OnValueChanged(EventHandlerMV3<string, object, object> call)
		{
			return this.valueChangedEvent.On(call);
		}
		public void OffValueChanged(EventHandlerMV3<string, object, object> call)
		{
			this.valueChangedEvent.Off(call);
		}

		protected readonly Dictionary<string, number> watchingExprs = new Dictionary<string, number>();

		protected readonly Dictionary<string, vm.Watcher> watcherList = new Dictionary<string, vm.Watcher>();
		protected readonly Dictionary<string, TPendingInfo> pendingInfoMergedCache = new Dictionary<string, TPendingInfo>();

		protected bool running = true;
		public bool Running
		{
			get
			{
				return this.running;
			}
			set
			{
				this.running = value;
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
						this.EmitValueChangedEvent(expr, v, oldValue);
					}
				}
			}
		}

		protected readonly List<TPendingInfo> pendingInfo = new List<TPendingInfo>();
		protected void EmitValueChangedEvent<T>(string expr, T value, T oldValue)
		{
			if (this.Running)
			{
				this.valueChangedEvent.Emit(expr, value, oldValue);
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
				this.EmitValueChangedEvent(expr, this.dataHost, null);
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
							this.EmitValueChangedEvent(expr, value, oldValue);
						}
					}, null, false);
				}
				if (watcher != null)
				{
					if (this.watchingExprs[expr] > 0)
					{
						this.watcherList[expr] = watcher;
						// 需要额外通知自身变化
						this.EmitValueChangedEvent(expr, watcher.value, null);
					}
				}
			}
		}
		/**
		 * 监听未监听过的接口
		 * @param expr 
		 */
		protected void DoWatchNewExpr(string expr, EventHandlerMV2<object, object> call)
		{
			this.watchingExprs[expr] = this.watchingExprs.TryGetValue(expr, out var watchingExpr) ? watchingExpr : 0;
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
		protected void DoUnWatchExprOnce(string expr)
		{
			this.watchingExprs[expr] = this.watchingExprs.TryGetValue(expr, out var watchingExpr) ? watchingExpr : 0;
			this.watchingExprs[expr]--;
			Debug.Assert(this.watchingExprs[expr] >= 0);
		}

	}
}
