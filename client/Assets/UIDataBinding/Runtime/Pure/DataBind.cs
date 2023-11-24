using System;
using System.Diagnostics;
using System.Collections.Generic;
using Game.Diagnostics.IO;
using Console = Game.Diagnostics.IO.Console;

namespace DataBinding.UIBind
{
	public class TPresetWatchingExpr
	{
		public string key;
		public EventHandlerMV2<object, object> call;
	}
	public class TWatchingExpr
	{
		public string key;
		public EventHandlerMV2<object, object> call;
	}

	public class DataBind
	{
		public Object rawObj;
		public DataBindHub bindHub;
		public void AddBindHub(IDataBindHub bindHub1)
		{
			this.AddBindHub((DataBindHub)bindHub1);
		}
		public void AddBindHub(DataBindHub bindHub1)
		{
			this.bindHub = bindHub1;
			this.RecoverWatchers();
		}
		public void RemoveBindHub(IDataBindHub bindHub1)
		{
			this.RemoveBindHub((DataBindHub)bindHub1);
		}
		public void RemoveBindHub(DataBindHub bindHub1)
		{
			if (bindHub1 == null || this.bindHub == bindHub1)
			{
				this.ClearWatchers();
				this.bindHub = null;
			}
			else
			{
				Console.Warn("invalid bindHub");
			}
		}

		protected readonly List<TPresetWatchingExpr> presetWatchingExprs = new List<TPresetWatchingExpr>();

		protected void RecordPresetExpr(string expr, EventHandlerMV2<object, object> call)
		{
			this.presetWatchingExprs.Add(new TPresetWatchingExpr()
			{
				key = expr,
				call = call,
			});
		}

		protected void UnRecordPresetExpr(string expr, EventHandlerMV2<object, object> call)
		{
			var item = this.presetWatchingExprs.Find(item => item.key == expr);
			if (item != null)
			{
				this.presetWatchingExprs.Remove(item);
			}
		}

		protected List<ISEventCleanInfo2<object, object>> watchingExprs = new List<ISEventCleanInfo2<object, object>>();

		protected ISEventCleanInfo2<object, object> EaseWatchExpr(string expr, EventHandlerMV2<object, object> call)
		{
			if (this.bindHub != null)
			{
				var watcher = this.bindHub.EaseWatchExprValue(expr, call);
				if (watcher != null)
				{
					this.watchingExprs.Add(watcher);
				}
				return watcher;
			}
			return null;
		}
		public ISEventCleanInfo2<object, object> WatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			this.RecordPresetExpr(expr, call);

			return this.EaseWatchExpr(expr, call);
		}
		public void UnWatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			this.UnRecordPresetExpr(expr, call);

			var watcher = this.watchingExprs.Find(watcher => watcher.callback == call && watcher.key == expr);
			if (watcher != null)
			{
				this.watchingExprs.Remove(watcher);
				if (this.bindHub != null)
				{
					var bindHub1 = this.bindHub;
					bindHub1.EaseUnWatchExprValue(expr, call);
				}
			}
		}

		public void RecoverWatchers()
		{
			if (this.bindHub != null)
			{
				this.presetWatchingExprs.ForEach((TPresetWatchingExpr info) =>
				{
					var key = info.key;
					var call = info.call;
					this.EaseWatchExpr(key, call);
				});
			}
		}
		public void ClearWatchers()
		{
			if (this.bindHub != null)
			{
				var bindHub1 = this.bindHub;
				this.watchingExprs.ForEach((ISEventCleanInfo2<object, object> info) =>
				{
					var key = info.key;
					var callback = info.callback;
					bindHub1.EaseUnWatchExprValue(key, callback);
				});
				this.watchingExprs.Clear();
			}
			Debug.Assert(this.watchingExprs.Count == 0);
		}

		public void Clear()
		{
			this.RemoveBindHub(this.bindHub);
		}

	}

}