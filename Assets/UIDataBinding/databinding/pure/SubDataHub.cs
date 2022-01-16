import { assert } from "cc";
import { DataBindHub } from "./DataBindHub";
import { DataHub } from "./DataHub";
import { ISubDataHub } from "./ISubDataHub";

export class SubDataHub implements ISubDataHub {
	static oidAcc = 0
	oid: number = SubDataHub.oidAcc++
	index: number = 0

	realDataHub?: DataHub
	rawObj?: Object

	setRealDataHub(realDataHub?: DataHub) {
		this.realDataHub = realDataHub
	}

	observeData(data: object) {
		assert(this.realDataHub != undefined)
		if (this.realDataHub) {
			this.realDataHub.observeData(data)
		}
	}

	unsetDataHost() {
		if (this.realDataHub) {
			this.realDataHub.unsetDataHost()
		}
	}

	bindDataHost(data?: object): void {
		if (data === undefined) {
			this.unsetDataHost()
		} else {
			this.observeData(data)
		}
	}

	protected parentHostWatcher?: {
		key: string;
		callback: fsync.event.EventHandlerMV<any>;
	}
	protected parentHub?: DataBindHub
	bindFromParentHub(parentHub: DataBindHub, subKey: string) {
		this.unbindFromParentHub()

		let watcher = parentHub.easeWatchExprValue<object>(subKey, (value, oldValue) => {
			if (typeof (value) == "object") {
				this.observeData(value)
			} else {
				this.unsetDataHost()
			}
		})
		this.parentHostWatcher = watcher
		this.parentHub = parentHub
		return watcher
	}
	unbindFromParentHub() {
		let parentHub = this.parentHub
		if (parentHub && this.parentHostWatcher) {
			let { key, callback, } = this.parentHostWatcher
			parentHub.easeUnWatchExprValue(key, callback)
			this.parentHostWatcher = undefined
			this.parentHub = undefined
		}
	}

}
