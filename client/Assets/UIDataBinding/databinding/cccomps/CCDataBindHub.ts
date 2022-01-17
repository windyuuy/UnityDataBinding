import { ccclass } from "../../convenient";
import { DataBindHub } from "../pure/DataBindHub";
import { CCMyComponent } from "./CCMyComponent";
import { DataBindHubHelper } from "./DataBindHubHelper";
/**
 * 数据绑定集线器
 */
@ccclass("CCDataBindHub")
export class CCDataBindHub extends CCMyComponent {
	dataBindHub: DataBindHub = new DataBindHub()

	protected needAttach() {
		return this.isAttachCalled && this.enabled && !!this.node.parent?.activeInHierarchy
	}

	integrate() {
		DataBindHubHelper.onAddDataBindHub(this)
	}

	deintegrate() {
		this.dataBindHub.clear()
	}

	/**
	 * 集成
	 * - 遍历所有浅层子hub, 设置父节点为自身
	 */
	relate() {
		DataBindHubHelper.onRelateDataBindHub(this)
	}

	derelate() {
		DataBindHubHelper.onDerelateDataBindHub(this)
	}

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

}
