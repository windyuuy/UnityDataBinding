
import { ccclass } from "../../convenient";
import { ISubDataHub } from "../pure/ISubDataHub";
import { CCMyComponent } from "./CCMyComponent";
import { DataBindHubHelper, ICCSubDataHub } from "./DataBindHubHelper";

@ccclass("CCSubDataHub")
export class CCSubDataHub extends CCMyComponent implements ICCSubDataHub {
	protected _subDataHub!: ISubDataHub

	// ccDataHost!: CCDataHost

	get dataHost(): vm.IHost | undefined {
		// return this.ccDataHost.dataHost
		return this._subDataHub.realDataHub?.dataHost
	}

	get subDataHub() {
		return this._subDataHub
	}

	// bindDataHost(dataHost: vm.IHost) {
	// this.ccDataHost.dataHub.setDataHost(dataHost)
	// }

	protected onPreload() {
		this.integrate()
	}
	protected onPreDestroy() {
		this.deintegrate()
	}

	onAttach() {
		this.relate()
	}

	onDeattach() {
		this.derelate()
	}

	integrate() {
		DataBindHubHelper.onAddSubDataHub(this)
	}

	deintegrate() {
		this.derelate()
	}

	relate() {
	}

	derelate() {
	}

	bindDataHost(data: object) {
		this._subDataHub.bindDataHost(data)
	}

}
