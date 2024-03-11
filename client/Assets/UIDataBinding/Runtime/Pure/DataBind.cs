using System;
using System.Diagnostics;
using System.Collections.Generic;
using Game.Diagnostics.IO;
using Console = Game.Diagnostics.IO.Console;

namespace DataBinding.UIBind
{
	public class TPresetWatchingExpr
	{
		public string Key;
		public EventHandlerMV2<object, object> Call;
	}
	public class TWatchingExpr
	{
		public string Key;
		public EventHandlerMV2<object, object> Call;
	}

	public class DataBind
	{
		public Object RawObj;
		public DataBindHub BindHub;
		public void AddBindHub(IDataBindHub bindHub1)
		{
			this.AddBindHub((DataBindHub)bindHub1);
		}
		public void AddBindHub(DataBindHub bindHub1)
		{
			this.BindHub = bindHub1;
			this.RecoverWatchers();
		}
		public void RemoveBindHub(IDataBindHub bindHub1)
		{
			this.RemoveBindHub((DataBindHub)bindHub1);
		}
		public void RemoveBindHub(DataBindHub bindHub1)
		{
			if (bindHub1 == null || this.BindHub == bindHub1)
			{
				this.BreakWatchers();
				this.BindHub = null;
			}
			else
			{
				Console.Warn("invalid bindHub");
			}
		}

		protected readonly List<TPresetWatchingExpr> PresetWatchingExprs = new List<TPresetWatchingExpr>();

		protected void RecordPresetExpr(string expr, EventHandlerMV2<object, object> call)
		{
			this.PresetWatchingExprs.Add(new TPresetWatchingExpr()
			{
				Key = expr,
				Call = call,
			});
		}

		protected void UnRecordPresetExpr(string expr, EventHandlerMV2<object, object> call)
		{
			var item = this.PresetWatchingExprs.Find(item => item.Key == expr);
			if (item != null)
			{
				this.PresetWatchingExprs.Remove(item);
			}
		}

		protected void ClearRecordPresetExpr()
		{
			this.PresetWatchingExprs.Clear();
		}

		protected List<ISEventCleanInfo2<object, object>> WatchingExprs = new List<ISEventCleanInfo2<object, object>>();

		protected ISEventCleanInfo2<object, object> EaseWatchExpr(string expr, EventHandlerMV2<object, object> call)
		{
			if (this.BindHub != null)
			{
				var watcher = this.BindHub.EaseWatchExprValue(expr, call);
				if (watcher != null)
				{
					this.WatchingExprs.Add(watcher);
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

			var watcher = this.WatchingExprs.Find(watcher => watcher.Callback == call && watcher.key == expr);
			if (watcher != null)
			{
				this.WatchingExprs.Remove(watcher);
				if (this.BindHub != null)
				{
					var bindHub1 = this.BindHub;
					bindHub1.EaseUnWatchExprValue(expr, call);
				}
			}
		}

		public void RecoverWatchers()
		{
			if (this.BindHub != null)
			{
				this.PresetWatchingExprs.ForEach((TPresetWatchingExpr info) =>
				{
					var key = info.Key;
					var call = info.Call;
					this.EaseWatchExpr(key, call);
				});
			}
		}

		public void ClearWatchers()
		{
			this.BreakWatchers();
			// clear through, cannot be recover
			this.ClearRecordPresetExpr();
		}

		/// <summary>
		/// break watchers when hub is break
		/// </summary>
		public void BreakWatchers()
		{
			if (this.BindHub != null)
			{
				var bindHub1 = this.BindHub;
				this.WatchingExprs.ForEach((ISEventCleanInfo2<object, object> info) =>
				{
					var key = info.key;
					var callback = info.Callback;
					bindHub1.EaseUnWatchExprValue(key, callback);
				});
				this.WatchingExprs.Clear();
			}
			Debug.Assert(this.WatchingExprs.Count == 0);
		}

		public void Clear()
		{
			this.RemoveBindHub(this.BindHub);
		}

	}

}