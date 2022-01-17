
import * as cc from "cc";
import { Component, Node, _decorator } from 'cc';
import { CCDataBindBase } from "./CCDataBindBase";
import { DownloadPicCache } from '../../common/DownloadPicCache';
const { ccclass, property, menu } = _decorator;

@ccclass('CCSimpleBindClickFuncInfo')
export class CCSimpleBindClickFuncInfo {
	@property({
		displayName: "执行",
	})
	callExpr: string = ""
}

@ccclass('CCSimpleBind')
@menu("DataDrive/CCSimpleBind")
export class CCSimpleBind extends CCDataBindBase {
	@property({
		multiline: true,
		displayName: "主属性",
	})
	key: string = ""

	@property({
		displayName: "可见性",
	})
	visible: string = ""

	@property({
		multiline: true,
		displayName: "点击响应",
		type: [CCSimpleBindClickFuncInfo],
	})
	clickTriggers: CCSimpleBindClickFuncInfo[] = []

	@property({
		displayName: "忽略undefined值",
	})
	ignoreUndefinedValue: boolean = true

	target: Component | Node | null = null

	watchValueChange<T>(key: string, call: (value: T, old?: T) => void) {
		return super.watchValueChange<T>(key, (value, old) => {
			if (this.ignoreUndefinedValue) {
				if (value !== undefined) {
					call(value, old)
				}
			} else {
				call(value, old)
			}
		})
	}

	protected needAttach() {
		if (this.visible) {
			// 支持visible设置
			return this.isAttachCalled && this.enabled && !!this.node.parent?.activeInHierarchy
		} else {
			return this.isAttachCalled && this.enabledInHierarchy
		}
	}

	/**
	 * 更新显示状态
	 */
	protected onBindItems() {
		this.checkClick()
		this.checkVisible()

		if (this.key) {
			this.checkLabel();

			// 设立优先级
			this.checkProgressBar() || this.checkSprite() || this.checkToggle();
		}
	}

	onDestroy() {
		super.onDestroy && super.onDestroy()
		this.node.targetOff(this)
	}

	protected _clickEventHandler?: Function
	protected _clickHandleFuncs: Function[] = []
	checkClick() {
		if (this.clickTriggers.length == 0) {
			return false
		}
		let node = this.node
		if (!node) {
			return false
		}
		this.target = this.getComponent(cc.Button)
		// let button = this.getComponent(cc.Button)
		// if (!button) {
		// 	return false
		// }
		if (this._clickEventHandler == null) {
			let handler = this._clickEventHandler = () => {
				this._clickHandleFuncs.forEach((clickHandleFunc, index) => {
					if (typeof (clickHandleFunc) == "function") {
						clickHandleFunc()
					} else {
						console.error(`call invalid func type of ${this.clickTriggers[index]} -> ${clickHandleFunc},`, clickHandleFunc)
					}
				})
			}
			this.node.off("click", this._clickEventHandler, this)
			this.node.on("click", handler, this)
		}
		this._clickHandleFuncs.clear()
		for (let i = 0; i < this.clickTriggers.length; i++) {
			let index = i
			let clickTrigger = this.clickTriggers[index]
			if (!clickTrigger.callExpr) {
				continue
			}
			let onSetValue = (value: Function) => {
				if (typeof (value) == "function") {
					if (cc.isValid(this, true)) this._clickHandleFuncs[index] = value;
				} else {
					console.warn("点击响应需要返回函数类型, 当前返回值: ", value, ", type:", typeof (value))
				}
			}
			this.watchValueChange<Function>(clickTrigger.callExpr, (newValue) => {
				onSetValue(newValue)
			})
		}
		return true
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

	checkLabel() {
		let label: cc.Label | cc.RichText | null = this.node.getComponent(cc.Label);
		label = label || this.node.getComponent(cc.RichText);
		if (!label) {
			return false
		}
		this.target = label
		this.watchValueChange<string>(this.key, (newValue) => {
			if (label) label.string = `${newValue}`;
		})
		return true
	}

	checkToggle() {
		let toggle = this.node.getComponent(cc.Toggle);
		if (!toggle) {
			return false
		}
		this.target = toggle
		this.watchValueChange(this.key, (newValue) => {
			if (toggle) toggle.isChecked = !!newValue;
		})
		return true
	}


	checkProgressBar() {

		var progressComponent = this.node.getComponent(cc.ProgressBar);
		if (!progressComponent) {
			return false
		}
		this.target = progressComponent
		this.watchValueChange<number>(this.key, (newValue) => {
			if (progressComponent) progressComponent.progress = newValue;
		})
		return true
	}

	spriteTextureUrl?: string
	checkSprite() {
		var sprite = this.node.getComponent(cc.Sprite);
		if (!sprite) {
			return false
		}
		this.target = sprite
		this.watchValueChange<string>(this.key, (newValue) => {
			this.spriteTextureUrl = newValue
			if (sprite) {
				this.loadImage(newValue, sprite)
			}
		})
		return true
	}

	protected updateSpriteLayout(sprite: cc.Sprite) {
		if (sprite.type == cc.Sprite.Type.SLICED) {
			let widget = sprite.getComponent(cc.Widget) ?? this.getComponent(cc.Widget)
			if (widget) {
				widget.updateAlignment()
			}
		}
	}

	loadImage(url: string, sprite: cc.Sprite) {
		if (url == "") {
			sprite.spriteFrame = null
			return
		}

		if (typeof (url) != "string") {
			return
		}

		if (url.startsWith("http")) {
			let sp = DownloadPicCache.Inst.get(url);
			if (sp) {
				sprite.spriteFrame = sp;
				this.updateSpriteLayout(sprite)
			} else {
				const onLoadImage = (err: any, assets: cc.ImageAsset) => {
					if (err) {
						console.error(err)
					} else {
						let spriteFrame: cc.SpriteFrame
						if (assets instanceof cc.Texture2D) {
							spriteFrame = new cc.SpriteFrame();
							spriteFrame.texture = assets;
						} else {
							spriteFrame = cc.SpriteFrame.createWithImage(assets)
						}
						sprite.spriteFrame = spriteFrame;

						this.updateSpriteLayout(sprite)
						DownloadPicCache.Inst.put(url, spriteFrame);
					}
				}
				let finalUrl: string = url
				if (url.indexOf(".jpg") < 0 && url.indexOf(".png") < 0 && url.indexOf(".jpeg") < 0) {
					finalUrl = `${url}?file=a.jpg`
				}
				console.log("to load remoteImage:", url, "->", finalUrl)
				cc.assetManager.loadRemote(finalUrl, onLoadImage)
			}
		} else {
			let assets = cc.resources.get(url + "/spriteFrame", cc.SpriteFrame)
			if (assets) {
				sprite.spriteFrame = assets
				this.updateSpriteLayout(sprite)
			} else {
				cc.resources.load(url + "/spriteFrame", cc.SpriteFrame, (err, assets: cc.SpriteFrame) => {
					if (err) {
						console.error(err)
					} else {
						sprite.spriteFrame = assets

						this.updateSpriteLayout(sprite)
					}
				})
			}
		}
	}
}
