import { instantiate, Node } from "cc";
import { ccclass, property } from "../../convenient";
import { CCContainerBinding } from "../CCContainerBinding";
import { CCDataHost } from "../CCDataHost";
import { DataBindHubUtils } from "./DataBindHubUtils";
import { CCContainerItem } from "./CCContainerItem";
import { CCMyComponent } from "./CCMyComponent";
import { ICCContainerCtrl } from "./DataBindHubHelper";

/**
 * 确定CCContainerBind的具体绑定行为
 */
@ccclass("CCContainerCtrl")
export class CCContainerCtrl extends CCMyComponent implements ICCContainerCtrl {

	/**
	 * 如果子节点没有添加 DialogChild, 那么强制为所有子节点添加 DialogChild
	 */
	@property({
		displayName: "自动收容子节点",
	})
	bindChildren: boolean = true

	@property({
		displayName: "使用容器选项覆盖",
	})
	overrideWithContainerOptions: boolean = true

	integrate() {
		let ccContainerBind = this.getComponent(CCContainerBinding)
		if (ccContainerBind) {
			if (this.overrideWithContainerOptions) {
				this.bindChildren = ccContainerBind.bindChildren
			}
			if (this.bindChildren) {
				this.node.children.forEach(child => {
					let ccDataHost = child.getOrAddComponent(CCDataHost)
					ccDataHost.integrate()
				})
			}
		}
	}

	protected watcher?: fsync.event.EventHandlerMV<any>
	protected oldList: any[] = []
	relate() {
		let ccContainerBind = this.getComponent(CCContainerBinding)
		if (ccContainerBind) {
			if (this.overrideWithContainerOptions) {
				this.bindChildren = ccContainerBind.bindChildren
			}
			this.watcher = ccContainerBind.containerBind.watchList((dataSources: any[]) => {
				let parent = this.node
				let children = parent.children
				let childrenCount0 = children.length
				let maxCount = dataSources.length
				let lastI = 0
				let childIndex = 0
				if (childrenCount0 == 0) {
					// 不存在
					console.warn("没有子节点, 无法满足预期数量的数据项数.")
				} else {
					let tempNode = children[childrenCount0 - 1]
					for (let i = 0; i < maxCount; childIndex++) {
						let child = children[childIndex]
						if (childIndex == maxCount) {
							if (i == lastI) {
								// 不足
								console.warn("可能无法满足预期数量的数据项数.")
							}
						}
						if (childIndex >= childrenCount0) {
							if (child == null) {
								// child = instantiate(tempNode)
								// child.parent = parent
								child = this.createItemNode(tempNode, parent)
							}
						}
						// 使用最近的节点作为模板
						tempNode = child
						// 默认刷到的节点全部显示
						child.active = true
						lastI = i
						if (this.bindChildren) {
							let ccContainerItem = child.getOrAddComponent(CCContainerItem)
							ccContainerItem.integrate()
						}
						DataBindHubUtils.foreachSurf(child, CCContainerItem, (ccItem) => {
							ccItem.containerItem.index = i++
							let itemHost = dataSources[ccItem.containerItem.index]
							let itemHost1 = vm.implementHost(itemHost)
							ccItem.bindDataHost(itemHost1)
						})
					}
					// 没刷到的节点全隐藏
					for (let j = childIndex; j < childrenCount0; j++) {
						let child = children[j]
						if (child) {
							child.active = false
						}
					}
				}
			})
		}
	}

	derelate() {
		let ccContainerBind = this.getComponent(CCContainerBinding)
		if (ccContainerBind) {
			if (this.watcher) {
				ccContainerBind.containerBind.unWatchList(this.watcher)
			}
		}
	}

	/**
	 * 创建子节点项
	 */
	protected createItemNode(tempNode: Node, parent: Node) {
		let child = instantiate(tempNode)
		child.parent = parent
		return child
	}

	onPreDestroy() {
		this.derelate()
	}

}
