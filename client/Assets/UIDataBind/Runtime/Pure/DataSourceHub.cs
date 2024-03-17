
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using EngineAdapter.LinqExt;
using Console = EngineAdapter.Diagnostics.Console;

namespace DataBind.UIBind
{
	using TExprCall = EventHandlerMV2<object, object>;
	using TWatchExprCall = EventHandlerMV2<string, EventHandlerMV2<object, object>>;

	public class TPendingInfo
	{
		public string Expr;
		public object NewValue;
		public object OldValue;

		public TPendingInfo()
		{

		}

		public TPendingInfo(string expr, object newValue, object oldValue)
		{
			this.Expr = expr;
			this.NewValue = newValue;
			this.OldValue = oldValue;
		}

		public TPendingInfo Clone()
		{
			var cl = new TPendingInfo();
			cl.Expr = this.Expr;
			cl.NewValue = this.NewValue;
			cl.OldValue = this.OldValue;
			return cl;
		}
	}
	/**
	 * 适配 DataHost, 提供更易用的接口规范
	 */
	public class DataSourceHub : IDataBindHubTree, IDataSourcePump, IDataBindHubSubTree
	{

		// public static readonly NoneOldValue = Symbol("NoneOldValue");

		public IStdHost DataHost = null;
		public Object RawObj { get; set; }

		protected readonly EventHandlerMV2<string, EventHandlerMV2<object, object>> OnWatchNewExprCall;
		protected readonly EventHandlerMV2<string, EventHandlerMV2<object, object>> OnUnWatchExprCall;
		public DataSourceHub()
		{
			this.OnWatchNewExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.DoWatchNewExpr(expr, call);
			};
			this.OnUnWatchExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.DoUnWatchExprOnce(expr);
			};
		}

		public void ObserveData(object data)
		{
			var d = VM.Utils.ImplementStdHost(data);
			if (d == null || d is IStdHost)
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
			if (this.DataHost == null)
			{
				return;
			}

			var dataHost0 = this.DataHost;
			this._unsetDataHost();


			if (dataHost0 != this.DataHost)
			{
				this.EmitValueChangedEvent("&this", this.DataHost, dataHost0);
			}
		}
		protected void _unsetDataHost()
		{
			foreach (var expr in this.WatcherList.Keys)
			{
				var watcher = this.WatcherList[expr];
				watcher.teardown();
			}
			this.WatcherList.Clear();

			this.DataHost = null;
		}

		protected void ReplaceDataHost(IStdHost dataHost)
		{
			if (this.DataHost == dataHost)
			{
				return;
			}

			var dataHost0 = this.DataHost;
			this._unsetDataHost();


			this.DataHost = dataHost;
			if (dataHost0 != dataHost)
			{
				this.EmitValueChangedEvent("&this", dataHost, dataHost0);
			}
			if (dataHost != null)
			{
				foreach (var expr in this.WatchingExprs.Keys)
				{
					var acc = this.WatchingExprs[expr];
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
							Console.Error(e);
						}
					}
				}
			}
		}

		public DataBindHub BindHub { get; set; } = null;
		public IEnumerable<T> PeekSourceBindEvents<T>()
		{
			return this.ValueChangedEvent.PeekListeningObject<T>().Distinct();
		}

		public void AddBindHub(IDataBindHub bindHub)
		{
			AddBindHub((DataBindHub)bindHub);
		}
		public void AddBindHub(DataBindHub bindHub0)
		{
			if (this.BindHub != bindHub0)
			{
				if (this.BindHub != null)
				{
					this.RemoveBindHub(this.BindHub);
				}

				if (bindHub0 != null)
				{
					Debug.Assert(this.BindHub == null);
					this.BindHub = bindHub0;
					Debug.Assert(bindHub0.Parents.Count == 0);
					bindHub0.Parents.Add(this);


					bindHub0.OnWatchNewExpr(this.OnWatchNewExprCall);
					bindHub0.OnUnWatchExpr(this.OnUnWatchExprCall);
					this.OnValueChanged(bindHub0.OnValueChanged);
					foreach (var expr in bindHub0.WatchingExprs.Keys)
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
			if (bindHub0 != null && this.BindHub == bindHub0)
			{
				var watchingExprs1 = bindHub0.WatchingExprs;
				foreach (var expr in watchingExprs1.Keys)
				{
					if (watchingExprs1[expr] > 0)
					{
						this.DoUnWatchExprOnce(expr);
					}
				}
				bindHub0.OffWatchNewExpr(this.OnWatchNewExprCall);
				bindHub0.OffUnWatchExpr(this.OnUnWatchExprCall);
				this.OffValueChanged(bindHub0.OnValueChanged);
				bindHub0.Parents.Remove(this);
				this.BindHub = null;
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
				call(this.DataHost, null);
			}
			else
			{
				if (this.WatcherList.TryGetValue(expr, out var watcher))
				{
					if (watcher != null)
					{
						call(watcher.value, null);
					}
				}
			}
		}

		protected readonly SimpleEventMV3<string, object, object> ValueChangedEvent = new SimpleEventMV3<string, object, object>();
		public EventHandlerMV3<string, object, object> OnValueChanged(EventHandlerMV3<string, object, object> call)
		{
			return this.ValueChangedEvent.On(call);
		}
		public void OffValueChanged(EventHandlerMV3<string, object, object> call)
		{
			this.ValueChangedEvent.Off(call);
		}

		protected readonly Dictionary<string, long> WatchingExprs = new Dictionary<string, long>();

		protected readonly Dictionary<string, VM.Watcher> WatcherList = new Dictionary<string, VM.Watcher>();
		protected readonly Dictionary<string, TPendingInfo> PendingInfoMergedCache = new Dictionary<string, TPendingInfo>();

		private bool _running = true;
		public bool Running
		{
			get
			{
				return this._running;
			}
			set
			{
				this._running = value;
				if (value && this.PendingInfo.Count > 0)
				{
					var pendingInfoCopy = this.PendingInfo;
					var pendingInfoMerged = this.PendingInfoMergedCache;
					pendingInfoMerged.Clear();
					foreach (var info in pendingInfoCopy)
					{
						var expr = info.Expr;
						var v = info.NewValue;
						var oldValue = info.OldValue;
						var infoMerged = pendingInfoMerged.TryGet(expr);
						if (infoMerged == null)
						{
							infoMerged = info.Clone();
							pendingInfoMerged[expr] = infoMerged;
						}
						else
						{
							infoMerged.NewValue = v;
						}
					}
					this.PendingInfo.Clear();

					foreach (var entry in pendingInfoMerged)
					{
						var expr = entry.Value.Expr;
						var v = entry.Value.NewValue;
						var oldValue = entry.Value.OldValue;
						this.EmitValueChangedEvent(expr, v, oldValue);
					}
				}
			}
		}

		protected readonly List<TPendingInfo> PendingInfo = new List<TPendingInfo>();
		protected void EmitValueChangedEvent<T>(string expr, T value, T oldValue)
		{
			if (this.Running)
			{
				this.ValueChangedEvent.Emit(expr, value, oldValue);
			}
			else
			{
				this.PendingInfo.Add(new TPendingInfo(expr, value, oldValue));
			}
		}

		protected void _doWatchNewExpr(string expr, EventHandlerMV2<object, object> call)
		{
			if (expr == "&this")
			{
				this.EmitValueChangedEvent(expr, this.DataHost, null);
			}
			else
			{
				//VM.Watcher watcher;
				if (this.DataHost == null)
				{
					//watcher = null;
				}
				else
				{
					if (this.WatchingExprs[expr] > 0)
					{
						var watcher = this.DataHost.Watch(expr, (host, value, oldValue) =>
						{
							if (this.WatchingExprs[expr] > 0)
							{
								this.EmitValueChangedEvent(expr, value, oldValue);
							}
						}, null, false);

						if (watcher != null)
						{
							this.WatcherList[expr] = watcher;
							// 需要额外通知自身变化
							this.EmitValueChangedEvent(expr, watcher.value, null);
						}
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
			this.WatchingExprs[expr] = this.WatchingExprs.TryGetValue(expr, out var watchingExpr) ? watchingExpr : 0;
			this.WatchingExprs[expr]++;

			if (this.WatcherList.ContainsKey(expr) == false)
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
			this.WatchingExprs[expr] = this.WatchingExprs.TryGetValue(expr, out var watchingExpr) ? watchingExpr : 0;
			this.WatchingExprs[expr]--;
			Debug.Assert(this.WatchingExprs[expr] >= 0);

			if (this.WatchingExprs[expr] == 0)
			{
				this.WatcherList.Remove(expr);
			}
		}

		/// <summary>
		/// debug only
		/// </summary>
		public List<IDataBindHubTree> Parents { get; } = new();
	}
}
