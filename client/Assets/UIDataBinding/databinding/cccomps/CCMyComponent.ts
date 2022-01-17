import { Component } from "cc";
import { ccclass } from "../../convenient";

/**
 * 启用延迟链接
 */
export const EnableLazyAttach = true

@ccclass("CCMyComponent")
export class CCMyComponent extends Component {

	protected static _creatingList: CCMyComponent[] = []
	protected _isCreating: boolean = this.markCreating()
	protected markCreating() {
		CCMyComponent._creatingList.push(this)
		return true
	}

	_deleteAttr(attr: keyof this) {
		delete (this as any)[attr]
	}

	protected onPreDestroy() {

	}

	_onPreDestroy() {
		this._isCreating = false
		this.onPreDestroy()
		super._onPreDestroy && super._onPreDestroy()
	}

	onCreate?() {
		this.onPreload()
	}
	protected __preload() {
		if (this._isCreating) {
			this.onPreload()
		}
	}

	protected onPreload() {

	}

	protected updateAttach() {
		if (EnableLazyAttach) {
			let needAttach = this.needAttach()
			if (needAttach && (!this.isAttached)) {
				this.isAttached = true
				this.onAttach && this.onAttach()
			} else if ((!needAttach) && this.isAttached) {
				this.isAttached = false
				this.onDeattach && this.onDeattach()
			}
		} else {
			if (this.isAttachCalled && (!this.isAttached)) {
				this.isAttached = true
				this.onAttach && this.onAttach()
			} else if ((!this.isAttachCalled) && this.isAttached) {
				this.isAttached = false
				this.onDeattach && this.onDeattach()
			}
		}
	}
	protected needAttach(): boolean {
		return this.isAttachCalled && this.enabledInHierarchy
		// && this.node.parent?.activeInHierarchy
	}
	protected isAttachCalled: boolean = false
	protected isAttached: boolean = false
	protected onEnable() {
		this.updateAttach()
	}
	protected onAfterAttach() {
		this.isAttachCalled = true
		this.updateAttach()
	}
	protected onDisable() {
		this.updateAttach()
	}
	protected onAfterDeattach() {
		this.isAttachCalled = false
		this.updateAttach()
	}

}
