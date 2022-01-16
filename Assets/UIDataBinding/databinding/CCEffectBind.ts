import { CCDialog } from "../CCDialogComp";
import { ccclass, menu, property } from "../convenient";
import { ICCReuse } from "../ICCReuse";
import { CCDataBindBase } from "./CCDataBindBase";

/**
 * 音频绑定配置
 */
@ccclass('CCEffectBind')
@menu("DataDrive/CCEffectBind")
export class CCEffectBind extends CCDataBindBase {
	@property({
		displayName: "播放音源名",
	})
	audioName: string = ""

	@property({
		multiline: true,
		displayName: "是否播放",
	})
	kNeedPlay: string = ""

	@property({
		displayName: "只播放一次",
	})
	playOnceOnly: boolean = true

	protected playOnce: boolean = false

	/**
	 * 更新显示状态
	 */
	protected onBindItems() {
		if (this.kNeedPlay && this.audioName) {
			this.checkAudio()
		}
	}

	protected dialog!: CCDialog
	onLoad() {
		this.dialog = this.findDialogComp()!
		this.dialog.node.on(ICCReuse.Reuse, () => {
			this.onShow()
		}, this)
	}

	onDestroy() {
		this.dialog.node.targetOff(this)
	}

	onShow() {
		this.playOnce = false
	}

	protected checkAudio() {
		let onChangeValue = (value: boolean) => {
			if (value) {
				// 仅播放一次过滤
				if (this.playOnceOnly && this.playOnce) {
					return
				}

				this.playOnce = true
				this.dialog.playEffect(this.audioName)
			}
		}
		this.watchValueChange<boolean>(this.kNeedPlay, (newValue) => {
			onChangeValue(newValue)
		})
		return true
	}

}
