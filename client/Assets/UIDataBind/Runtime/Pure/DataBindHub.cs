
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataBind.UIBind
{
	using TWatchExprCall = EventHandlerMV2<string, EventHandlerMV2<object, object>>;
	using TExprCall = EventHandlerMV2<object, object>;

	public class DataBindHub : IDataBindHub 
	{

		protected static long OidAcc = 0;
		public long Oid = ++DataBindHub.OidAcc;

		public Object RawObj { get; set; }

		public readonly Dictionary<string, long> WatchingExprs = new Dictionary<string, long>();

		public List<IDataBindHubTree> Parents { get;} = new List<IDataBindHubTree>();
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

		public List<DataBindHub> BindHubs { get; } = new List<DataBindHub>();

		public readonly List<IDataBindPump> DataBindPumps = new();
		public IEnumerable<(string key, IRawObjObservable target)> PeekBindEvents()
		{
			return this.Listeners.PeekNamedListeningObject<IRawObjObservable>().Distinct();
		}

		public IEnumerable<IDataBindPump> GetBindPumps()
		{
			return DataBindPumps;
		}

		public void RemoveParents()
		{
			while (this.Parents.Count > 0)
			{
				this.Parent = null;
			}
		}
		public void RemoveChildren()
		{
			while (this.BindHubs.Count > 0)
			{
				var bindHub = this.BindHubs[0];
				this.RemoveBindHub(bindHub);
			}
		}

		protected readonly EventHandlerMV2<string, EventHandlerMV2<object, object>> OnWatchNewExprCall;
		protected readonly EventHandlerMV2<string, EventHandlerMV2<object, object>> OnUnWatchExprCall;
		public readonly EventHandlerMV3<string, object, object> OnValueChanged;
		public DataBindHub()
		{
			this.OnWatchNewExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.DoWatchNewExpr(expr, call);
			};
			this.OnUnWatchExprCall = (string expr, EventHandlerMV2<object, object> call) =>
			{
				this.DoUnWatchExprOnce(expr);
			};
			this.OnValueChanged = (string expr, object value, object oldValue) =>
			{
				this.EmitExprValueChanged(expr, value, oldValue);
			};
		}

		public void AddBindHub(IDataBindHub bindHub)
		{
			AddBindHub((DataBindHub)bindHub);
		}
		public void AddBindHub(DataBindHub bindHub)
		{
			if (!this.BindHubs.Contains(bindHub))
			{
				this.BindHubs.Add(bindHub);
				Debug.Assert(bindHub.Parents.Count == 0);
				bindHub.Parents.Add(this);
				bindHub.OnWatchNewExpr(this.OnWatchNewExprCall);
				bindHub.OnUnWatchExpr(this.OnUnWatchExprCall);
				this.OnAnyValueChanged(bindHub.OnValueChanged);
				foreach (var expr in bindHub.WatchingExprs.Keys)
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
			RemoveBindHub((DataBindHub)bindHub);
		}
		public void RemoveBindHub(DataBindHub bindHub)
		{
			if (this.BindHubs.Contains(bindHub))
			{
				var watchingExprs = bindHub.WatchingExprs;
				foreach (var expr in watchingExprs.Keys)
				{
					if (watchingExprs[expr] > 0)
					{
						this.DoUnWatchExprOnce(expr);
					}
				}
				bindHub.OffWatchNewExpr(this.OnWatchNewExprCall);
				bindHub.OffUnWatchExpr(this.OnUnWatchExprCall);
				this.OffAnyValueChanged(bindHub.OnValueChanged);
				bindHub.Parents.Remove(this);
				this.BindHubs.Remove(bindHub);
			}
		}

		protected readonly SimpleEventMV2<string, EventHandlerMV2<object, object>> NewExprEvent = new SimpleEventMV2<string, EventHandlerMV2<object, object>>();
		protected readonly SimpleEventMV2<string, EventHandlerMV2<object, object>> RemoveExprEvent = new SimpleEventMV2<string, EventHandlerMV2<object, object>>();

		/**
		 * 监听未监听过的接口
		 * @param expr 
		 */
		public TWatchExprCall OnWatchNewExpr(TWatchExprCall call)
		{
			return this.NewExprEvent.On(call);
		}
		public void OffWatchNewExpr(TWatchExprCall call)
		{
			this.NewExprEvent.Off(call);
		}
		/**
		 * 不再监听某个接口
		 * @param expr 
		 */
		public TWatchExprCall OnUnWatchExpr(TWatchExprCall call)
		{
			return this.RemoveExprEvent.On(call);
		}
		public void OffUnWatchExpr(TWatchExprCall call)
		{
			this.RemoveExprEvent.Off(call);
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
			this.WatchingExprs[expr] = this.WatchingExprs.TryGetValue(expr, out var watchingExpr) ? watchingExpr : 0;
			this.WatchingExprs[expr]++;

			if (this.WatchingExprs[expr] == 1)
			{
				this.NewExprEvent.Emit(expr, call);
			}
		}
		/**
		 * 不监听某个接口
		 * @param expr 
		 */
		public void DoUnWatchExprOnce(string expr)
		{
			this.WatchingExprs[expr] = this.WatchingExprs.TryGetValue(expr, out var watchingExpr) ? watchingExpr : 0;
			this.WatchingExprs[expr]--;



			if (this.WatchingExprs[expr] == 0)
			{
				this.RemoveExprEvent.Emit(expr, null);
			}
		}

		public EventHandlerMV3<string, object, object> OnAnyValueChanged(EventHandlerMV3<string, object, object> call)
		{
			return this.Listeners.OnAnyEvent(call);
		}
		public void OffAnyValueChanged(EventHandlerMV3<string, object, object> call)
		{
			this.Listeners.OffAnyEvent(call);
		}

		protected readonly SEventMV2<object, object> Listeners = new SEventMV2<object, object>();
		public ISEventCleanInfo2<object, object> WatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			Debug.Assert(call.Target is IRawObjObservable, "call.Target is IRawObjObservable");
			return this.Listeners.On(expr, call);
		}
		public void UnWatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			this.Listeners.Off(expr, call);
		}
		protected void EmitExprValueChanged(string expr, object value, object oldValue)
		{
			this.Listeners.Emit(expr, value, oldValue);
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
