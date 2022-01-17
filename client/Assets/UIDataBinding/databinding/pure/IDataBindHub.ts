import { TExprCall, TWatchExprCall } from "./DataBindHub";

export interface IDataBindHubTree {
	addBindHub(bindHub: IDataBindHub): void
	removeBindHub(bindHub: IDataBindHub): void
	/**
	 * 立即同步表达式的值
	 * @param expr
	 * @param call
	 * @returns
	 */
	syncExprValue(expr: string, call: TExprCall): void
}

export interface IDataBindHub extends IDataBindHubTree {
	parents: IDataBindHubTree[]
	parent: IDataBindHubTree | undefined

	/**
	 * 监听未监听过的接口
	 * @param expr 
	 */
	onWatchNewExpr(call: TWatchExprCall): fsync.event.EventHandlerMV<any>
	offWatchNewExpr(call: TWatchExprCall): void
	/**
	 * 不再监听某个接口
	 * @param expr 
	 */
	onUnWatchExpr(call: TWatchExprCall): fsync.event.EventHandlerMV<any>
	offUnWatchExpr(call: TWatchExprCall): void

	onAnyValueChanged<T>(call: (expr: string, value: T, oldValue: T) => void): fsync.event.EventHandlerMV<any>
	offAnyValueChanged<T>(call: (expr: string, value: T, oldValue: T) => void): void

	watchExprValue<T>(expr: string, call: (value: T, oldValue: T) => void): {
		key: string;
		callback: fsync.event.EventHandlerMV<any>;
	}
	unWatchExprValue<T>(expr: string, call: (value: T, oldValue: T) => void): void
}
