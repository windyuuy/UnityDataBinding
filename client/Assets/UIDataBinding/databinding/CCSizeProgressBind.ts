
import * as cc from "cc";
import { _decorator } from 'cc';
import { CCSizeProgress } from "../widgets/CCSizeProgress";
import { CCDataBindBase } from "./CCDataBindBase";
const { ccclass, property, menu } = _decorator;

@ccclass('CCSizeProgressBind')
@menu("DataDrive/CCSizeProgressBind")
export class CCSizeProgressBind extends CCDataBindBase {
	@property({
		displayName: "进度",
		multiline: true,
	})
	kProgress: string = ""

	/**
	 * 更新显示状态
	 */
	protected onBindItems() {
		this.checkIsProgress()
	}

	checkIsProgress() {
		if (!this.kProgress) {
			return false
		}
		let node = this.node
		if (!node) {
			return false
		}
		this.watchValueChange(this.kProgress, (newValue) => {
			if (cc.isValid(node, true)) this.setProgress(newValue)
		})
		return true
	}

	setProgress(value: any) {
		let comp = this.getComponent(CCSizeProgress)
		if (comp) {
			comp.progress = value
		}
	}

}
