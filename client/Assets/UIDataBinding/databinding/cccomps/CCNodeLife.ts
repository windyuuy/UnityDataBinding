import { BaseNode, Node, Scene } from "cc";
import { EDITOR } from "cc/env";
import { ccclass } from "../../convenient";
import { CCMyComponent, EnableLazyAttach } from "./CCMyComponent";

let foreachChildren = (node: BaseNode, newParent: BaseNode | null, parent0: BaseNode | null) => {
	let nodeLife = (node as any)["ccNodeLife"] as CCNodeLife
	if (nodeLife && nodeLife.node == node) {
		nodeLife["_onParentChanged"](newParent, parent0)
	} else {
		node.children.forEach(child => {
			foreachChildren(child, newParent, parent0)
		})
	}
}

export function isInScene(node: BaseNode | null): boolean {
	while (node != null) {
		if (node instanceof Scene) {
			return true
		}
		node = node.parent
	}
	return false
}

if (!EDITOR) {
	let proto = BaseNode.prototype
	let old_setParent = proto.setParent
	let setParentIndex = 0
	proto.setParent = function (value: Node | Scene | null, keepWorldTransform = false) {
		setParentIndex++
		const oldParent = this["_parent"];
		// let nodeLife = (this as any)["ccNodeLife"] as CCNodeLife
		let ret = old_setParent.call(this, value, keepWorldTransform)
		const needUpdate = value !== oldParent
		if (needUpdate) {

			// 弥补节点
			if (value) {
				let creatingList = CCMyComponent["_creatingList"]
				if (!this.activeInHierarchy) {
					this.components.forEach(comp => {
						if (comp instanceof CCMyComponent && comp["_isCreating"]) {
							creatingList.push(comp)
						}
					})
				}
				while (creatingList.length > 0) {
					let comp = creatingList.shift()
					if (comp && comp["_isCreating"] && comp.node
						&& comp.node.isChildOf(value)
					) {
						comp["_isCreating"] = false
						comp.onCreate && comp.onCreate!()
					}
				}
			}
		}
		setParentIndex--

		if (needUpdate
			&& setParentIndex == 0
		) {
			if (value == null) {
				foreachChildren(this, value, oldParent)
			} else {
				// 弥补反序列时, set parent 事件丢失的问题
				foreachChildren(this, value, oldParent)
				let newlyLoaded = CCNodeLife.newlyLoaded
				while (newlyLoaded.length > 0) {
					let ccNodeLife = newlyLoaded.shift()
					if (ccNodeLife && ccNodeLife["_newParent"] !== value) {
						ccNodeLife["_onParentChanged"](ccNodeLife.node.parent, null)
					}
				}
			}
		}

		return ret
	}
}

export function getNodePath(node: Node) {
	let paths: string[] = []
	let curNode: Node | null = node
	while (curNode) {
		paths.unshift(curNode.name)
		curNode = curNode.parent
	}
	let pathsString = paths.join("/")
	return pathsString
}

@ccclass("CCNodeLife")
// @_decorator.disallowMultiple
export class CCNodeLife extends CCMyComponent {
	// protected __preload?(): void {
	// 	this.node.on("parent-changed", this._onParentChanged, this, true)
	// 	this.node.on("active-in-hierarchy-changed", () => {
	// 		console.log("active-in-hierarchy-changed", this.node.name)
	// 	})
	// 	this.node.on("sibling-order-changed", () => {
	// 		console.log("sibling-order-changed", this.node.name)
	// 	})
	// 	this.onEnable = function () {
	// 		if (this.node.parent != null) {
	// 			this.onAttach && this.onAttach()
	// 			this.onEnable = undefined as any
	// 		}
	// 	}
	// }

	protected onPreload() {
		this.integrate()
	}
	static newlyLoaded: CCNodeLife[] = []
	protected isLoaded: boolean = false
	protected integrate() {
		if (this.isLoaded) {
			return
		}
		this.isLoaded = true;

		(this.node as any)["ccNodeLife"] = this;
		// this["_onParentChanged"](this.node.parent, null)
		CCNodeLife.newlyLoaded.push(this)
	}

	protected _newParent?: BaseNode
	protected _onParentChanged(newParent: BaseNode | null, oldParent: BaseNode | null) {
		this._newParent = newParent as any
		// console.warn("parent-change:", this.name, newParent?.name, oldParent?.name)
		if (newParent == null) {
			if (this.onDeattach) {
				this.onDeattach()
			}
		} else {
			if (this.onAttach) {
				this.onAttach()
			}
		}
	}

	onAttach() {
		this.node.components.forEach(comp => {
			if (comp != this) {
				if ((comp as any).onAfterAttach) {
					(comp as any).onAfterAttach()
				}
			}
		})
	}

	onDeattach() {
		this.node.components.forEach(comp => {
			if (comp != this) {
				if ((comp as any).onAfterDeattach) {
					(comp as any).onAfterDeattach()
				}
			}
		})
	}

	/**
	 * 节点activeInHierachy变化时, 通知不可见的子节点跟随变更
	 */
	protected updateInactiveChildrenAttach() {
		if (!EnableLazyAttach) {
			return
		}
		this.node.children.forEach(child => {
			if (!child.active) {
				child.components.forEach(comp => {
					if (comp != this) {
						if ((comp as any).updateAttach) {
							(comp as any).updateAttach()
						}
					}
				})
			}
		})
	}
	onEnable() {
		this.updateInactiveChildrenAttach()
	}
	onDisable() {
		this.updateInactiveChildrenAttach()
	}

}
