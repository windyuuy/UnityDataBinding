using UnityEngine;
using System;
using System.Collections.Generic;
using DataBinding;

namespace DataBinding.UIBind
{
	/**
	 * 表达式监听集合
	 */
	public class ExprWatcher<T2>
	{
		/**
		 * 检测的数据
		 */
		protected IStdHost _host;
		/**
		 * 检测数据对象
		 * @param data
		 */
		protected virtual IStdHost host
		{
			get
			{
				return this._host;
			}
		}

		protected List<vm.Watcher> watchList = new List<vm.Watcher>();

		public virtual void observeData(T2 data)
		{
			this._host = vm.Utils.implementStdHost(data);
		}

		/**
		 * 拦截host的watch方法，收集所有watcher，统一释放
		 * @param expOrFn 方法名
		 * @param cb 回调函数
		 */
		protected virtual vm.Watcher watch(vm.CombineType<string, Action> expOrFn, Action<object, object> cb, bool sync = false)
		{
			if (this.host == null)
			{
				return null;
			}

			var watcher = this.host.Watch(expOrFn, (host, newVal, oldVal) =>
			{
				cb(newVal, oldVal);
			}, null, sync);
			if (watcher != null)
			{
				this.watchList.Add(watcher);
			}
			return watcher;
		}

		public virtual vm.Watcher watchExpr<T>(string expr, Action<T, T> call)
		{
			if (string.IsNullOrEmpty(expr))
			{
				return null;
			}

			var watcher = this.watch(expr, (newValue, oldValue) =>
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

		public virtual vm.Watcher watchExprChangeOnly<T>(string expr, Action<T, T> call)
		{
			if (string.IsNullOrEmpty(expr))
			{
				return null;
			}

			var watcher = this.watch(expr, (newValue, oldValue) =>
			{
				call((T)newValue, (T)oldValue);
			});

			return watcher;
		}

		public virtual void unwatch(vm.Watcher watcher)
		{
			this.watchList.Remove(watcher);
			watcher.teardown();
		}

		public virtual void flush()
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

		public virtual void clear()
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

		public virtual void flushAndClear()
		{
			this.flush();
			this.clear();
		}

	}
}
