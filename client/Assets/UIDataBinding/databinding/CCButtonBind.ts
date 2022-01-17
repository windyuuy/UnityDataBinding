
import { _decorator, Component, Node } from 'cc';
import * as cc from "cc"
import { CCDataBindBase } from "./CCDataBindBase";
import { CCSimpleBindClickFuncInfo } from "./CCSimpleBind";
const { ccclass, property, menu } = _decorator;

@ccclass('CCButtonBind')
@menu("DataDrive/CCButtonBind")
export class CCButtonBind extends CCDataBindBase {
	@property({
		displayName: "可交互",
	})
	kInteractive: string = ""

	@property({
		displayName: "变灰",
	})
	kToGray: string = ""

	@property({
		multiline: true,
		displayName: "点击响应",
		type: [CCSimpleBindClickFuncInfo],
	})
	clickTriggers: CCSimpleBindClickFuncInfo[] = []

	/**
	 * 更新显示状态
	 */
	protected onBindItems() {
		this.checkInteractive()
		this.checkToGray()
		this.checkClick()
	}

	checkInteractive() {
		if (!this.kInteractive) {
			return false
		}
		let node = this.node
		if (!node) {
			return false
		}
		this.watchValueChange<boolean>(this.kInteractive, (newValue) => {
			if (cc.isValid(node, true)) this.setInteractive(newValue)
		})
		return true
	}

	protected setInteractive(b: boolean) {
		let button = this.node.getComponent(cc.Button)
		if (button) {
			button.interactable = b
		}
	}

	checkToGray() {
		if (!this.kToGray) {
			return false
		}
		let node = this.node
		if (!node) {
			return false
		}
		this.watchValueChange<boolean>(this.kToGray, (newValue) => {
			if (cc.isValid(node, true)) this.setToGray(newValue)
		})
		return true
	}

	isGray: boolean = false
	protected setToGray(b: boolean) {
		this.isGray = b
		let button = this.node.getComponent(cc.Button)
		let sprite = this.node.getComponent(cc.Sprite)
		if (button && sprite) {
			if (b) {
				sprite.color = button.disabledColor
			} else {
				sprite.color = cc.color(255, 255, 255)
			}
		}
	}

	target: cc.Button | null = null
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
		// let button = this.getComponent(cc.Button)
		// if (!button) {
		// 	return false
		// }
		this.target = this.getComponent(cc.Button)
		if (this._clickEventHandler == null) {
			let handler = this._clickEventHandler = (target: cc.Button) => {
				this.target = target
				this._clickHandleFuncs.forEach((clickHandleFunc, index) => {
					if (typeof (clickHandleFunc) == "function") {
						clickHandleFunc.call(this, this.dataBind, index)
					} else {
						console.error(`call invalid func type of ${this.clickTriggers[index]} -> ${clickHandleFunc},`, clickHandleFunc)
					}
				})
			}
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
					if (cc.isValid(this)) this._clickHandleFuncs[index] = value;
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

}
