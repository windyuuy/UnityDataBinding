using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace UI.DataBinding
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
		public void addBindHub(DataBindHub bindHub)
		{
			this.bindHub = bindHub;
			this.recoverWatchers();
		}
		public void removeBindHub(DataBindHub bindHub)
		{
			if (bindHub == null || this.bindHub == bindHub)
			{
				this.clearWatchers();
				this.bindHub = null;
			}
			else
			{
				console.warn("invalid bindHub");
			}
		}

		protected List<TPresetWatchingExpr> presetWatchingExprs = new List<TPresetWatchingExpr>();

		protected void recordPresetExpr(string expr, EventHandlerMV2<object, object> call)
		{
			this.presetWatchingExprs.Add(new TPresetWatchingExpr()
			{
				key = expr,
				call = call,
			});
		}

		protected void unRecordPresetExpr(string expr, EventHandlerMV2<object, object> call)
		{
			var item = this.presetWatchingExprs.Find(item => item.key == expr);
			if (item != null)
			{
				this.presetWatchingExprs.Remove(item);
			}
		}

		protected List<ISEventCleanInfo2<object, object>> watchingExprs = new List<ISEventCleanInfo2<object, object>>();

		protected ISEventCleanInfo2<object, object> easeWatchExpr(string expr, EventHandlerMV2<object, object> call)
		{
			if (this.bindHub != null)
			{
				var watcher = this.bindHub.easeWatchExprValue(expr, call);
				if (watcher != null)
				{
					this.watchingExprs.Add(watcher);
				}
				return watcher;
			}
			return null;
		}
		public ISEventCleanInfo2<object, object> watchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			this.recordPresetExpr(expr, call);

			return this.easeWatchExpr(expr, call);
		}
		public void unWatchExprValue(string expr, EventHandlerMV2<object, object> call)
		{
			this.unRecordPresetExpr(expr, call);

			var watcher = this.watchingExprs.Find(watcher => watcher.callback == call && watcher.key == expr);
			if (watcher != null)
			{
				this.watchingExprs.Remove(watcher);
				if (this.bindHub != null)
				{
					var bindHub = this.bindHub;
					bindHub.easeUnWatchExprValue(expr, call);
				}
			}
		}

		public void recoverWatchers()
		{
			if (this.bindHub != null)
			{
				this.presetWatchingExprs.ForEach((TPresetWatchingExpr info) =>
				{
					var key = info.key;
					var call = info.call;
					this.easeWatchExpr(key, call);
				});
			}
		}
		public void clearWatchers()
		{
			if (this.bindHub != null)
			{
				var bindHub = this.bindHub;
				this.watchingExprs.ForEach((ISEventCleanInfo2<object, object> info) =>
				{
					var key = info.key;
					var callback = info.callback;
					bindHub.easeUnWatchExprValue(key, callback);
				});
				this.watchingExprs.Clear();
			}
			Debug.Assert(this.watchingExprs.Count == 0);
		}

		public void clear()
		{
			this.removeBindHub(this.bindHub);
		}

	}

}