using System;
using System.Linq;
using System.Collections.Generic;

namespace UI.DataBinding
{
	public class TPresetWatchingExpr
	{
		public string key;
		public Action<object, object> call;
	}
	public class TWatchingExpr
	{
		public string key;
		public EventHandlerMV<object> call;
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

		protected void recordPresetExpr<T>(string expr, Action<object, object> call)
		{
			this.presetWatchingExprs.Add(new TPresetWatchingExpr()
			{
				key = expr,
				call = call,
			});
		}

		protected void unRecordPresetExpr<T>(string expr, Action<T, T> call)
		{
			var item = this.presetWatchingExprs.Find(item => item.key == expr);
			if (item != null)
			{
				this.presetWatchingExprs.Remove(item);
			}
		}

		protected List<TWatchingExpr> watchingExprs = new List<TWatchingExpr>();

		protected void easeWatchExpr<T>(string expr, Action<T, T> call)
		{
			if (this.bindHub != null)
			{
				var watcher = this.bindHub.easeWatchExprValue(expr, call);
				if (watcher)
				{
					this.watchingExprs.push(watcher);
				}
				return watcher;
			}
			return null;
		}
		public void watchExprValue<T>(string expr, Action<T, T> call)
		{
			this.recordPresetExpr(expr, call);

			return this.easeWatchExpr(expr, call);
		}
		public void unWatchExprValue<T>(string expr, Action<T, T> call)
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
			if (this.bindHub)
			{
				this.presetWatchingExprs.forEach(({ key, call }) => {
					this.easeWatchExpr(key, call);
				});
			}
		}
		public void clearWatchers()
		{
			if (this.bindHub)
			{
				const bindHub = this.bindHub;
				this.watchingExprs.forEach(({ key, callback }) => {
					bindHub.easeUnWatchExprValue(key, callback);
				});
				this.watchingExprs.clear();
			}
			assert(this.watchingExprs.length == 0);
		}

		public void clear()
		{
			this.removeBindHub(this.bindHub);
		}

	}

}