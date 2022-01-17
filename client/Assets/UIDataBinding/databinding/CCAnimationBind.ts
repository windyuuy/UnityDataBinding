import { PlaySpineAnimSettings, TSpineAnimCallback, TSpineAnimEvent } from "./PlaySpineAnimSettings";
import { ccclass, menu, property } from "../convenient";
import { CCDataBindBase } from "./CCDataBindBase";
import { Animation, AnimationClip } from "cc";

/**
 * 音频绑定配置
 */
@ccclass('CCAnimationBind')
@menu("DataDrive/CCAnimationBind")
export class CCAnimationBind extends CCDataBindBase {
	@property({
		displayName: "播放动画配置",
	})
	kAnimConfig: string = ""

	@property({
		displayName: "播放动画名",
	})
	kAnimName: string = ""

	@property({
		displayName: "是否播放",
	})
	kIsLoop: string = ""

	@property({
		displayName: "播放轨道",
	})
	kTrackIndex: string = ""

	@property({
		displayName: "动画事件回调",
	})
	kCallback: string = ""

	/**
	 * 状态是否改变
	 */
	kIsDirty: string = ""

	/**
	 * 更新显示状态
	 */
	protected onBindItems() {
		this.checkSpine()
	}

	config?: PlaySpineAnimSettings
	animName: string = ""
	isLoop: boolean = false
	trackIndex: number = 0
	callback?: TSpineAnimCallback = undefined
	onStop!: Function

	protected checkSpine() {

		if (this.kAnimConfig) {
			if (!this.kAnimName) {
				this.kAnimName = this.kAnimConfig + ".animName"
			}
			if (!this.kIsLoop) {
				this.kIsLoop = this.kAnimConfig + ".isLoop"
			}
			if (!this.kTrackIndex) {
				this.kTrackIndex = this.kAnimConfig + ".trackIndex"
			}

			if (!this.kCallback) {
				this.kCallback = this.kAnimConfig + ".callback"
			}

			if (!this.kIsDirty) {
				this.kIsDirty = this.kAnimConfig + ".isDirty"
			}
		}

		this.watchValueChange(this.kAnimConfig, (value: PlaySpineAnimSettings) => {
			this.config = value
		})

		this.watchValueChange(this.kTrackIndex, (value: number) => {
			this.trackIndex = value
		})

		this.watchValueChange(this.kIsLoop, (value: boolean) => {
			if (this.animName) {
				this.isLoop = value
				let sk = this.getComponent(Animation)
				if (sk) {
					let state = sk.getState(this.animName)
					if (state) {
						if (this.isLoop) {
							state.wrapMode = AnimationClip.WrapMode.Loop
						} else {
							state.wrapMode = AnimationClip.WrapMode.Normal
						}
					}
				}
			}
		})

		this.watchValueChange(this.kAnimConfig + ".onStop", (value: Function) => {
			this.onStop = value
		})

		let onFinishCallback = () => {
			if (this.isLoop) {
				return
			}

			let animName = this.animName

			if (this.onStop && this.config) {
				this.onStop.apply(this.config)
			}

			let anim = this.getComponent(Animation)
			this.callback && this.callback(animName, TSpineAnimEvent.Finished, anim!)
		}
		this.watchValueChange(this.kCallback, (value: TSpineAnimCallback) => {
			if (typeof (value) == "function") {
				this.callback = value
			}
		})

		this.watchValueChange(this.kAnimName, (value: string) => {
			this.animName = value
		})

		this.watchValueChange(this.kIsDirty, (value: number) => {
			if (this.animName) {
				let anim = this.getComponent(Animation)
				if (anim) {
					anim.targetOff(anim)
					anim.once(Animation.EventType.FINISHED, onFinishCallback, anim)
					anim.play(this.animName)
					let state = anim.getState(this.animName)
					if (state) {
						if (this.isLoop) {
							state.wrapMode = AnimationClip.WrapMode.Loop
						} else {
							state.wrapMode = AnimationClip.WrapMode.Normal
						}
					}
				}
			} else {
				let anim = this.getComponent(Animation)
				if (anim) {
					anim.stop()
				}
			}
		})

		return true
	}

}
