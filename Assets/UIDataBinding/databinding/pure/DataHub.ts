import { assert } from "cc";
import { EmptyTable, SimpleEventMV, TStrMap } from "../../basic";
import { DataBindHub, TExprCall } from "./DataBindHub";
import { IDataBindHubTree } from "./IDataBindHub";

export const NoneOldValue = Symbol("NoneOldValue")

/**
 * 适配 DataHost, 提供更易用的接口规范
 */
export class DataHub implements IDataBindHubTree {
	dataHost?: vm.IHost = undefined
	rawObj?: Object

	protected _onWatchNewExprCall!: (expr: string, call: (value: any, oldValue: any) => void) => void
	protected _onUnWatchExprCall!: (expr: string) => void
	constructor() {
		this._onWatchNewExprCall = (expr: string, call: (value: any, oldValue: any) => void) => {
			this.doWatchNewExpr(expr, call)
		}
		this._onUnWatchExprCall = (expr: string) => {
			this.doUnWatchExprOnce(expr)
		}
	}

	observeData(data: object) {
		let d = vm.implementHost(data)
		this.setDataHost(d)
	}

	setDataHost(dataHost?: vm.IHost) {
		this.replaceDataHost(dataHost)
	}

	unsetDataHost() {
		if (this.dataHost === undefined) {
			return
		}

		let dataHost0 = this.dataHost
		this._unsetDataHost()

		if (dataHost0 !== this.dataHost) {
			this.emitValueChangedEvent("&this", this.dataHost, dataHost0)
		}
	}
	protected _unsetDataHost() {
		for (let expr in this.watcherList) {
			let watcher = this.watcherList[expr]
			watcher.teardown()
		}
		for (let expr of Object.keys(this.watcherList)) {
			delete this.watcherList[expr]
		}
		this.dataHost = undefined
	}

	protected replaceDataHost(dataHost?: vm.IHost) {
		if (this.dataHost == dataHost) {
			return
		}

		let dataHost0 = this.dataHost
		this._unsetDataHost()

		this.dataHost = dataHost
		if (dataHost0 !== dataHost) {
			this.emitValueChangedEvent("&this", dataHost, dataHost0)
		}
		if (dataHost) {
			for (let expr in this.watchingExprs) {
				let acc = this.watchingExprs[expr]
				if (acc >= 1) {
					try {
						this._doWatchNewExpr(expr, (value, oldValue) => {
							this.emitValueChangedEvent(expr, value, oldValue)
						})
					} catch (e) {
						console.error(e)
					}
				}
			}
		}
	}

	bindHub?: DataBindHub = undefined
	addBindHub(bindHub: DataBindHub) {
		if (this.bindHub != bindHub) {
			if (this.bindHub) {
				this.removeBindHub(this.bindHub)
			}

			if (bindHub) {
				assert(this.bindHub == undefined)
				this.bindHub = bindHub
				assert(bindHub.parents.length == 0)
				bindHub.parents.push(this)

				bindHub.onWatchNewExpr(this._onWatchNewExprCall)
				bindHub.onUnWatchExpr(this._onUnWatchExprCall)
				this.onValueChanged(bindHub["_onValueChanged"])
				for (let expr in bindHub["watchingExprs"]) {
					this.doWatchNewExpr(expr, (value, oldValue) => {
						this.emitValueChangedEvent(expr, value, oldValue)
					})
				}
			}
		}

	}

	removeBindHub(bindHub: DataBindHub) {
		if (bindHub && this.bindHub == bindHub) {
			let watchingExprs = bindHub["watchingExprs"]
			for (let expr in watchingExprs) {
				if (watchingExprs[expr] > 0) {
					this.doUnWatchExprOnce(expr)
				}
			}
			bindHub.offWatchNewExpr(this._onWatchNewExprCall)
			bindHub.offUnWatchExpr(this._onUnWatchExprCall)
			this.offValueChanged(bindHub["_onValueChanged"])
			bindHub.parents.remove(this)
			this.bindHub = undefined
		}
	}

	/**
	 * 立即同步表达式的值
	 * @param expr
	 * @param call
	 * @returns
	 */
	syncExprValue(expr: string, call: TExprCall) {
		if (expr == "&this") {
			call(this.dataHost, undefined)
		} else {
			let watcher = this.watcherList[expr]
			if (watcher) {
				call(watcher.value, undefined)
			}
		}
	}

	protected valueChangedEvent: SimpleEventMV<any> = new SimpleEventMV()
	onValueChanged<T>(call: (expr: string, value: T, oldValue: T) => void) {
		return this.valueChangedEvent.on(call)
	}
	offValueChanged<T>(call: (expr: string, value: T, oldValue: T) => void) {
		return this.valueChangedEvent.off(call)
	}

	protected watchingExprs: TStrMap<number> = EmptyTable()

	protected watcherList: TStrMap<vm.Watcher> = EmptyTable()
	protected pendingInfoMergedCache: Map<string, [string, any, any]> = new Map()

	protected _running: boolean = true;
	get running(): boolean {
		return this._running;
	}
	set running(value: boolean) {
		this._running = value;
		if (value && this.pendingInfo.length > 0) {
			let pendingInfoCopy = this.pendingInfo//.concat()
			let pendingInfoMerged = this.pendingInfoMergedCache
			pendingInfoMerged.clear()
			for (let info of pendingInfoCopy) {
				let [expr, value, oldValue] = info
				let infoMerged = pendingInfoMerged.get(expr)
				if (infoMerged == null) {
					infoMerged = info.concat() as [string, any, any]
					pendingInfoMerged.set(expr, infoMerged)
				} else {
					infoMerged[1] = value
				}
			}
			this.pendingInfo.clear()

			for (let entry of pendingInfoMerged) {
				let [expr, value, oldValue] = entry[1]
				this.emitValueChangedEvent(expr, value, oldValue)
			}
		}
	}
	protected pendingInfo: [string, any, any][] = []
	protected emitValueChangedEvent<T>(expr: string, value: T, oldValue: T) {
		if (this.running) {
			this.valueChangedEvent.emit(expr, value, oldValue)
		} else {
			this.pendingInfo.push([expr, value, oldValue])
		}
	}

	protected _doWatchNewExpr<T>(expr: string, call: (value: T, oldValue: T) => void) {
		if (expr == "&this") {
			this.emitValueChangedEvent(expr, this.dataHost, undefined)
		} else {
			let watcher = this.dataHost?.$watch(expr, (value, oldValue) => {
				if (this.watchingExprs[expr] > 0) {
					this.emitValueChangedEvent(expr, value, oldValue)
				}
			})
			if (watcher) {
				if (this.watchingExprs[expr] > 0) {
					this.watcherList[expr] = watcher
					// 需要额外通知自身变化
					this.emitValueChangedEvent(expr, watcher.value, undefined)
				}
			}
		}
	}
	/**
	 * 监听未监听过的接口
	 * @param expr 
	 */
	protected doWatchNewExpr<T>(expr: string, call: (value: T, oldValue: T) => void) {
		this.watchingExprs[expr] = this.watchingExprs[expr] ?? 0
		this.watchingExprs[expr]++

		if (this.watcherList[expr] == null) {
			// if (expr == "&this") {
			// 	this.emitValueChangedEvent(expr, this.dataHost, undefined)
			// } else {
			// 	let watcher = this.dataHost?.$watch(expr, (value, oldValue) => {
			// 		if (this.watchingExprs[expr] > 0) {
			// 			this.emitValueChangedEvent(expr, value, oldValue)
			// 		}
			// 	})
			// 	if (watcher) {
			// 		this.watcherList[expr] = watcher
			// 		// 需要额外通知自身变化
			// 		this.emitValueChangedEvent(expr, watcher.value, undefined)
			// 	}
			// }
			this._doWatchNewExpr(expr, call)
		} else {
			// let watcher = this.watcherList[expr]
			// if (watcher) {
			// 	let value = watcher.value
			// 	call(value, value)
			// }
		}
	}
	/**
	 * 不监听某个接口
	 * @param expr 
	 */
	protected doUnWatchExprOnce(expr: string) {
		this.watchingExprs[expr] = this.watchingExprs[expr] ?? 0
		this.watchingExprs[expr]--
		assert(this.watchingExprs[expr] >= 0)
	}

}
