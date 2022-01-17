
import * as cc from "cc";
import { _decorator } from 'cc';
import { CCDataBindBase } from "./CCDataBindBase";
const { ccclass, property, menu } = _decorator;

@ccclass('CCToggleBind')
@menu("DataDrive/CCToggleBind")
export class CCToggleBind extends CCDataBindBase {
	@property({
		displayName: "是否选中",
		multiline: true,
	})
	kIsChecked: string = ""

	/**
	 * 更新显示状态
	 */
	protected onBindItems() {
		this.checkIsChecked()
	}

	checkIsChecked() {
		if (!this.kIsChecked) {
			return false
		}
		let node = this.node
		if (!node) {
			return false
		}
		this.watchValueChange<boolean>(this.kIsChecked, (newValue) => {
			if (cc.isValid(node, true)) this.setIsChecked(newValue)
		})
		return true
	}

	setIsChecked(newValue: boolean) {
		let toggle = this.getComponent(cc.Toggle)
		if (toggle) {
			toggle.isChecked = newValue
		}
	}

}
