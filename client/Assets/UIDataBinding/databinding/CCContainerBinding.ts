
import { Component } from "cc";
import { ccclass, menu, property } from "../convenient";
import { CCMyComponent } from "./cccomps/CCMyComponent";
import { DataBindHubHelper } from "./cccomps/DataBindHubHelper";
import { ContainerBind } from "./pure/ContainerBind";

@ccclass("CCContainerBinding")
@menu("DataDrive/CCContainerBinding")
export class CCContainerBinding extends CCMyComponent {
	@property({
		displayName: "容器数据",
	})
	bindSubExp: string = ""

	/**
	 * 如果子节点没有添加 DialogChild, 那么强制为所有子节点添加 DialogChild
	 */
	@property({
		displayName: "自动收容子节点",
	})
	bindChildren: boolean = true

	containerBind: ContainerBind = new ContainerBind()

	/**
	 * 集成
	 * - 遍历所有浅层子hub, 设置父节点为自身
	 */
	integrate() {
		this.containerBind.rawObj = this
		DataBindHubHelper.onAddContainerBind(this)
	}

	protected isRelate = false
	relate() {
		this.isRelate = true
		DataBindHubHelper.onRelateContainerBind(this)
	}

	derelate() {
		DataBindHubHelper.onDerelateContainerBind(this)
	}

	protected onPreload() {
		this.integrate()
	}
	protected onPreDestroy() {
		this.derelate()
	}

	onAttach() {
		this.relate()
	}

	onDeattach() {
		this.derelate()
	}

}
