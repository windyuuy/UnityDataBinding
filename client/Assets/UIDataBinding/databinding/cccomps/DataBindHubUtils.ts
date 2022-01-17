import { Component, Node } from "cc";


export class TDataBindHubUtils {
	foreachSurfChildren<T extends Component>(target: Node, cls: new () => T, call: (comp: T) => void) {
		for (let child of target.children) {
			let comps = child.getComponents(cls)
			if (comps.length > 0) {
				comps.forEach(comp => {
					call(comp)
				})
			} else {
				this.foreachSurfChildren(child, cls, call)
			}
		}
	}

	foreachSurf<T extends Component>(target: Node, cls: new () => T, call: (comp: T) => void) {
		let comps = target.getComponents(cls)
		if (comps.length > 0) {
			comps.forEach(comp => {
				call(comp)
			})
		} else {
			for (let child of target.children) {
				this.foreachSurf(child, cls, call)
			}
		}

	}
}

export const DataBindHubUtils = new TDataBindHubUtils()
