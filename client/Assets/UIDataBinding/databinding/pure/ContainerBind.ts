import { EmptyTable, SimpleEventMV, TNumMap } from "../../basic";
import { ContainerItem } from "./ContainerItem";
import { DataBindHub } from "./DataBindHub";

export class ContainerBind {
	bindHub?: DataBindHub
	rawObj?: Object
	addBindHub(bindHub: DataBindHub) {
		this.bindHub = bindHub
	}
	removeBindHub(bindHub: DataBindHub) {
		if (bindHub == undefined || this.bindHub == bindHub) {
			this.bindHub = undefined
		}
	}

	expr!: string
	protected exprWatcher?: {
		key: string;
		callback: fsync.event.EventHandlerMV<any>;
	}

	bindExpr(expr: string) {
		if (this.expr != expr) {
			this.unbindExpr()
		}
		if (expr !== undefined) {
			this.expr = expr
			if (this.bindHub) {
				const bindHub = this.bindHub
				const onValueChanged = (value: any[]) => {
					this.onDataChangedEvent.emit(value)

					const bindList = this.bindList
					for (let oid in bindList) {
						let item = bindList[oid]
						if (item.realDataHub) {
							// 确认是否需要换成 observeData
							item.realDataHub.setDataHost(value[item.index])
						}
					}
				}
				this.exprWatcher = bindHub.watchExprValue(expr, onValueChanged)
				if (this.exprWatcher) {
					bindHub.doWatchNewExpr(expr, onValueChanged)
					bindHub.syncExprValue(expr, onValueChanged)
				}

			}
		}
	}

	unbindExpr() {
		if (this.exprWatcher) {
			if (this.bindHub) {
				const bindHub = this.bindHub
				bindHub.unWatchExprValue(this.exprWatcher.key, this.exprWatcher.callback)
				bindHub.doUnWatchExprOnce(this.expr)
			}
			this.exprWatcher = undefined
		}
	}

	protected bindList: TNumMap<ContainerItem> = EmptyTable()
	bindItem(item: ContainerItem) {
		this.bindList[item.oid] = item
	}

	unbindItem(item: ContainerItem) {
		if (this.bindList[item.oid]) {
			if (item.realDataHub) {
				item.realDataHub.unsetDataHost()
			}
			delete this.bindList[item.oid]
		}
	}

	protected onDataChangedEvent: SimpleEventMV<any> = new SimpleEventMV()

	protected watcherList: fsync.event.EventHandlerMV<any>[] = []

	watchList<T>(call: (value: T[]) => void) {
		let watcher = this.onDataChangedEvent.on(call)
		this.watcherList.push(watcher)
		return watcher
	}

	unWatchList<T>(watcher: fsync.event.EventHandlerMV<any>) {
		this.onDataChangedEvent.off(watcher)
		this.watcherList.remove(watcher)
	}

	unWatchLists() {
		this.watcherList.forEach(watcher => {
			this.onDataChangedEvent.off(watcher)
		})
		this.watcherList.clear()
	}

}
