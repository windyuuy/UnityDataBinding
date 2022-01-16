
/**
 * 表达式监听集合
 */
export class ExprWatcher<T = any> {
	/**
	 * 检测的数据
	 */
	private _host?: vm.IHost
		/**
		 * 检测数据对象
		 * @param data
		 */
		| undefined;
	protected get host(): vm.IHost & T
		/**
		 * 检测数据对象
		 * @param data
		 */
		| undefined {
		return this._host as (vm.IHost & T)
	}

	protected watchList: vm.Watcher[] = []

	observeData(data: T) {
		this._host = vm.implementHost(data)
	}

	/**
	 * 拦截host的watch方法，收集所有watcher，统一释放
	 * @param expOrFn 方法名
	 * @param cb 回调函数
	 */
	protected watch(expOrFn: string | Function, cb: (newValue: any, oldValue: any) => void, sync?: boolean): vm.Watcher | undefined {
		if (!this.host) {
			return undefined
		}

		let watcher = this.host.$watch(expOrFn, (newVal, oldVal) => {
			cb(newVal, oldVal);
		}, undefined, sync);
		if (watcher) {
			this.watchList.push(watcher);
		}
		return watcher;
	}

	watchExpr<T>(expr: string, call: (value: T, old?: T) => void): vm.Watcher | undefined {
		if (!expr) {
			return undefined
		}

		let watcher = this.watch(expr, (newValue, oldValue) => {
			call(newValue, oldValue)
		});
		// console.log("checkLabel", watcher)
		if (watcher && watcher.value !== undefined) {
			call(watcher.value)
		}
		return watcher
	}

	watchExprChangeOnly<T>(expr: string, call: (value: T, old?: T) => void): vm.Watcher | undefined {
		if (!expr) {
			return undefined
		}

		let watcher = this.watch(expr, (newValue, oldValue) => {
			call(newValue, oldValue)
		});

		return watcher
	}

	unwatch(watcher: vm.Watcher) {
		this.watchList.remove(watcher)
		watcher.teardown()
	}

	flush() {
		// 及时在清理之前应用更新
		this.watchList.concat().forEach(item => {
			if (item) {
				item.run()
			}
		})
	}

	clear() {
		this.watchList.forEach(item => {
			item && item.teardown();
		})
		this.watchList.clear()
	}

	flushAndClear() {
		this.flush()
		this.clear()
	}

}
