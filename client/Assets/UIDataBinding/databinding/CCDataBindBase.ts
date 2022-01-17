
import * as cc from "cc";
import { Component, _decorator } from 'cc';
import { CCDialogComp } from "../CCDialogComp";
import { CCMyComponent } from "./cccomps/CCMyComponent";
import { DataBindHubHelper } from "./cccomps/DataBindHubHelper";
import { DataBind } from "./pure/DataBind";
const { ccclass, property } = _decorator;

@ccclass('CCDataBindBase')
export class CCDataBindBase extends CCMyComponent {
	// constructor(...args: any[]) {
	// 	super(...args)
	// }

	dataBind: DataBind = new DataBind()

	watchValueChange<T>(key: string, call: (value: T, old?: T) => void) {
		return this.dataBind.watchExprValue(key, call)
	}
	clearWatchers() {
		this.dataBind.clearWatchers()
	}

	doBindItems() {
		this.onBindItems()
	}
	protected onBindItems() {

	}

	doUnBindItems() {
		this.onUnBindItems()
	}
	protected onUnBindItems() {
		this.clearWatchers()
	}

	/**
	 * 获取当前组件所在节点的host
	 */
	public findDialogComp(): CCDialogComp | null {
		let node: cc.Node | null = this.node;

		try {
			while (node && node.getParent && !(node.getComponent("CCDialogComp"))) {
				node = node.getParent ? node.getParent() : null;
			}
		} catch (error) {

		}
		if (!node) {
			return null
		}

		let dialogComponent = node.getComponent("CCDialogComp") as any as CCDialogComp
		return dialogComponent
	}

	integrate() {
		DataBindHubHelper.onAddDataBind(this)
	}

	deintegrate() {
		this.dataBind.clear()
	}

	relate() {
		DataBindHubHelper.onRelateDataBind(this)
	}

	derelate() {
		DataBindHubHelper.onDerelateDataBind(this)
	}

	protected onPreload() {
		this.integrate()
	}

	protected onPreDestroy() {
		this.deintegrate()
	}

	onAttach() {
		this.relate()
	}

	onDeattach() {
		this.derelate()
	}

}
