using UnityEngine;
using System;
using System.Collections.Generic;
using DataBind;

namespace DataBind.UIBind
{
	/**
	 * 表达式监听集合
	 */
	public class ExprWatcher<T2>
	{
		/**
		 * 检测的数据
		 */
		private IStdHost _host;
		/**
		 * 检测数据对象
		 * @param data
		 */
		protected virtual IStdHost Host
		{
			get
			{
				return this._host;
			}
		}

		protected readonly List<VM.Watcher> watchList = new List<VM.Watcher>();

		public virtual void ObserveData(T2 data)
		{
			this._host = VM.Utils.ImplementStdHost(data);
		}

		/**
		 * 拦截host的watch方法，收集所有watcher，统一释放
		 * @param expOrFn 方法名
		 * @param cb 回调函数
		 */
		protected virtual VM.Watcher Watch(VM.CombineType<object, string, Action> expOrFn, Action<object, object> cb, bool sync = false)
		{
			if (this.Host == null)
			{
				return null;
			}

			var watcher = this.Host.Watch(expOrFn, (host, newVal, oldVal) =>
			{
				cb(newVal, oldVal);
			}, null, sync);
			if (watcher != null)
			{
				this.watchList.Add(watcher);
			}
			return watcher;
		}

		public virtual VM.Watcher WatchExpr<T>(string expr, Action<T, T> call)
		{
			if (string.IsNullOrEmpty(expr))
			{
				return null;
			}

			var watcher = this.Watch(expr, (newValue, oldValue) =>
			{
				call((T)newValue, (T)oldValue);
			});
			// console.log("checkLabel", watcher)
			if (watcher != null && watcher.value != null)
			{
				call((T)watcher.value, default(T));
			}
			return watcher;
		}

		public virtual VM.Watcher WatchExprChangeOnly<T>(string expr, Action<T, T> call)
		{
			if (string.IsNullOrEmpty(expr))
			{
				return null;
			}

			var watcher = this.Watch(expr, (newValue, oldValue) =>
			{
				call((T)newValue, (T)oldValue);
			});

			return watcher;
		}

		public virtual void Unwatch(VM.Watcher watcher)
		{
			this.watchList.Remove(watcher);
			watcher.teardown();
		}

		public virtual void Flush()
		{
			// 及时在清理之前应用更新
			this.watchList.ToArray().ForEach(item =>
			{
				if (item != null)
				{
					item.run();
				}
			});
		}

		public virtual void Clear()
		{
			this.watchList.ToArray().ForEach(item =>
			{
				if (item != null)
				{
					item.teardown();
				}
			});
			this.watchList.Clear();
		}

		public virtual void FlushAndClear()
		{
			this.Flush();
			this.Clear();
		}

	}
}
