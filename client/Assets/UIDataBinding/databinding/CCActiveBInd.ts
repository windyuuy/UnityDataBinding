
import * as cc from "cc";
import { _decorator } from 'cc';
import { CCDataBindBase } from "./CCDataBindBase";
const { ccclass, property, menu } = _decorator;

@ccclass('CCActiveBind')
@menu("DataDrive/CCActiveBind")
export class CCActiveBind extends CCDataBindBase {
	@property({
		displayName: "可见性",
	})
	visible: string = ""

	protected needAttach() {
		if (this.visible) {
			return this.isAttachCalled && this.enabled && !!this.node.parent?.activeInHierarchy
		} else {
			return this.isAttachCalled && this.enabledInHierarchy
		}
	}

	/**
	 * 更新显示状态
	 */
	protected onBindItems() {
		this.checkVisible()
	}

	checkVisible() {
		if (!this.visible) {
			return false
		}
		let node = this.node
		if (!node) {
			return false
		}
		this.watchValueChange<boolean>(this.visible, (newValue) => {
			if (cc.isValid(node, true)) node.active = newValue;
		})
		return true
	}

}
