import { DataHub } from "./DataHub"
import { ISubDataHub } from "./ISubDataHub"

export class ContainerItem implements ISubDataHub {
	static oidAcc = 0
	oid: number = ContainerItem.oidAcc++
	index: number = 0

	realDataHub?: DataHub
	rawObj?: Object

	setRealDataHub(realDataHub?: DataHub) {
		this.realDataHub = realDataHub

		if (this.realDataHub) {
			this.bindDataHost(this.dataHost)
		}
	}

	dataHost?: object
	observeData(data: object) {
		this.dataHost = data
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

}
