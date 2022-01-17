import { assert } from "cc";
import { EmptyTable, SEventMV, SimpleEventMV, TStrMap } from "../../basic";
import { IDataBindHub, IDataBindHubTree } from "./IDataBindHub";

export type TWatchExprCall = (expr: string, call: (value: any, oldValue: any) => void) => void
export type TExprCall = (value: any, oldValue: any) => void

export class DataBindHub implements IDataBindHub, IDataBindHubTree {

	protected static oidAcc = 0
	oid: number = ++DataBindHub.oidAcc

	rawObj?: Object

	protected watchingExprs: TStrMap<number> = EmptyTable()

	parents: IDataBindHubTree[] = []
	get parent(): IDataBindHubTree | undefined {
		return this.parents[0]
	}
	set parent(value: IDataBindHubTree | undefined) {
		if (this.parent !== value) {
			if (this.parent) {
				let parent = this.parent
				this.parents.remove(parent)
				parent.removeBindHub(this)
			}
			if (value != null) {
				value.addBindHub(this)
			}
		}
	}
	bindHubs: DataBindHub[] = []

	removeParents() {
		while (this.parents.length > 0) {
			this.parent = undefined
		}
	}
	removeChildren() {
		while (this.bindHubs.length > 0) {
			let bindHub = this.bindHubs[0]
			this.removeBindHub(bindHub)
		}
	}

	protected _onWatchNewExprCall!: (expr: string, call: (value: any, oldValue: any) => void) => void
	protected _onUnWatchExprCall!: (expr: string) => void
	protected _onValueChanged!: (expr: string, value: any, oldValue: any) => void
	constructor() {
		this._onWatchNewExprCall = (expr: string, call: (value: any, oldValue: any) => void) => {
			this.doWatchNewExpr(expr, call)
		}
		this._onUnWatchExprCall = (expr: string) => {
			this.doUnWatchExprOnce(expr)
		}
		this._onValueChanged = (expr: string, value: any, oldValue: any) => {
			this.emitExprValueChanged(expr, value, oldValue)
		}
	}

	addBindHub(bindHub: DataBindHub) {
		if (!this.bindHubs.contains(bindHub)) {
			this.bindHubs.push(bindHub)
			assert(bindHub.parents.length == 0)
			bindHub.parents.push(this)
			bindHub.onWatchNewExpr(this._onWatchNewExprCall)
			bindHub.onUnWatchExpr(this._onUnWatchExprCall)
			this.onAnyValueChanged(bindHub._onValueChanged)
			for (let expr in bindHub["watchingExprs"]) {
				const call = (value: any, oldValue: any) => {
					this.emitExprValueChanged(expr, value, oldValue)
				}
				this.doWatchNewExpr(expr, call)
				this.syncExprValue(expr, call)
			}
		}
	}

	removeBindHub(bindHub: DataBindHub) {
		if (this.bindHubs.contains(bindHub)) {
			let watchingExprs = bindHub["watchingExprs"]
			for (let expr in watchingExprs) {
				if (watchingExprs[expr] > 0) {
					this.doUnWatchExprOnce(expr)
				}
			}
			bindHub.offWatchNewExpr(this._onWatchNewExprCall)
			bindHub.offUnWatchExpr(this._onUnWatchExprCall)
			this.offAnyValueChanged(bindHub._onValueChanged)
			bindHub.parents.remove(this)
			this.bindHubs.remove(bindHub)
		}
	}

	protected newExprEvent: SimpleEventMV<any> = new SimpleEventMV()
	protected removeExprEvent: SimpleEventMV<any> = new SimpleEventMV()

	/**
	 * 监听未监听过的接口
	 * @param expr 
	 */
	onWatchNewExpr(call: TWatchExprCall) {
		return this.newExprEvent.on(call)
	}
	offWatchNewExpr(call: TWatchExprCall) {
		return this.newExprEvent.off(call)
	}
	/**
	 * 不再监听某个接口
	 * @param expr 
	 */
	onUnWatchExpr(call: TWatchExprCall) {
		return this.removeExprEvent.on(call)
	}
	offUnWatchExpr(call: TWatchExprCall) {
		return this.removeExprEvent.off(call)
	}

	/**
	 * 立即同步表达式的值
	 * @param expr 
	 * @param call 
	 * @returns 
	 */
	syncExprValue(expr: string, call: TExprCall) {
		return this.parents.forEach(parent => parent.syncExprValue(expr, call))
	}

	/**
	 * 监听未监听过的接口
	 * @param expr 
	 */
	doWatchNewExpr<T>(expr: string, call: (value: T, oldValue: T) => void) {
		this.watchingExprs[expr] = this.watchingExprs[expr] ?? 0
		this.watchingExprs[expr]++

		if (this.watchingExprs[expr] == 1) {
			this.newExprEvent.emit(expr, call)
		}
	}
	/**
	 * 不监听某个接口
	 * @param expr 
	 */
	doUnWatchExprOnce(expr: string) {
		this.watchingExprs[expr] = this.watchingExprs[expr] ?? 0
		this.watchingExprs[expr]--

		if (this.watchingExprs[expr] == 0) {
			this.removeExprEvent.emit(expr)
		}
	}

	onAnyValueChanged<T>(call: (expr: string, value: T, oldValue: T) => void) {
		return this.listeners.onAnyEvent(call)
	}
	offAnyValueChanged<T>(call: (expr: string, value: T, oldValue: T) => void) {
		return this.listeners.offAnyEvent(call)
	}

	protected listeners: SEventMV<any> = new SEventMV()
	watchExprValue<T>(expr: string, call: (value: T, oldValue: T) => void) {
		return this.listeners.on(expr, call)
	}
	unWatchExprValue<T>(expr: string, call: (value: T, oldValue: T) => void) {
		return this.listeners.off(expr, call)
	}
	protected emitExprValueChanged<T>(expr: string, value: T, oldValue: T) {
		this.listeners.emit(expr, value, oldValue)
	}

	clear() {
		this.removeParents()
		this.removeChildren()
	}

	easeWatchExprValue<T>(expr: string, call: (value: T, oldValue: T) => void) {
		const bindHub = this

		let watcher = bindHub.watchExprValue(expr, call)
		if (watcher) {
			bindHub.doWatchNewExpr(expr, call)
			bindHub.syncExprValue(expr, call)
		}
		return watcher
	}

	easeUnWatchExprValue<T>(expr: string, call: (value: T, oldValue: T) => void) {
		const bindHub = this
		bindHub.unWatchExprValue(expr, call)

		bindHub.doUnWatchExprOnce(expr)
	}

}
