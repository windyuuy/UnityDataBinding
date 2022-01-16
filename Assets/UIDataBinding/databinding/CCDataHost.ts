
import { _decorator, Component, Node } from 'cc';
import { CCMyComponent } from "./cccomps/CCMyComponent";
import { DataBindHubHelper } from "./cccomps/DataBindHubHelper";
import { DataHub } from "./pure/DataHub";
const { ccclass, property } = _decorator;

@ccclass('CCDataHost')
export class CCDataHost extends CCMyComponent {

	get dataHost(): vm.IHost | undefined {
		return this.dataHub.dataHost
	}

	dataHub: DataHub = new DataHub()


	onAfterAttach() {
		this.onAttach()
	}
	onAfterDeattach() {
		this.onDeattach()
	}
	onEnable() {
		this.dataHub.running = true
	}
	onDisable() {
		this.dataHub.running = false
	}

	observeData(data: object) {
		this.dataHub.observeData(data)
	}

	setDataHost(dataHost?: vm.IHost) {
		this.dataHub.setDataHost(dataHost)
	}

	unsetDataHost() {
		this.dataHub.unsetDataHost()
	}

	protected __preload() {
		this.integrate()
	}
	protected onPreDestroy() {
		this.derelate()
		DataBindHubHelper.onRemoveDataHub(this)
	}

	onAttach() {
		this.relate()
	}

	onDeattach() {
		this.derelate()
	}

	/**
	 * 集成
	 * - 遍历所有浅层子hub, 设置父节点为自身
	 */
	integrate() {
		this.dataHub.rawObj = this
		DataBindHubHelper.onAddDataHub(this)
	}

	relate() {
		DataBindHubHelper.onRelateDataHub(this)
	}

	derelate() {
		DataBindHubHelper.onDerelateDataHub(this)
	}

}
