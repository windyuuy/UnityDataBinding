/*
 * @Author: your name
 * @Date: 2021-08-25 10:27:32
 * @LastEditTime: 2021-09-08 10:00:35
 * @LastEditors: Please set LastEditors
 * @Description: In User Settings Edit
 * @FilePath: \client\assets\script\ui\core\DialogChild.ts
 */
import { CCDialogComp } from "../CCDialogComp";
import { ccclass, property } from "../convenient";
import { CCSubDataHub } from "./cccomps/CCSubDataHub";
import { DataBindHubHelper, ICCDialogChild } from "./cccomps/DataBindHubHelper";
import { DialogHelper } from "./cccomps/DialogHelper";
import { DataBindHub } from "./pure/DataBindHub";
import { SubDataHub } from "./pure/SubDataHub";


@ccclass("CCDialogChild")
export class CCDialogChild extends CCSubDataHub implements ICCDialogChild {
	@property({
		displayName: "承接父容器数据源",
	})
	private _autoExtendDataSource: boolean = false;
	@property({
		displayName: "承接父容器数据源",
	})
	public get autoExtendDataSource(): boolean {
		return this._autoExtendDataSource;
	}
	public set autoExtendDataSource(value: boolean) {
		this._autoExtendDataSource = value;
	}

	@property({
		displayName: "绑定子项数据源",
	})
	private _subKey: string = ""
	@property({
		displayName: "绑定子项数据源",
	})
	public get subKey(): string {
		return this._subKey;
	}
	public set subKey(value: string) {
		this._subKey = value;
	}

	protected _subDataHub = new SubDataHub()

	observeData(data: object) {
		this._subDataHub.observeData(data)
		// this.integrate()
	}

	unsetDataHost() {
		this._subDataHub.unsetDataHost()
	}

	integrate() {
		DataBindHubHelper.onAddDialogChild(this)
	}

	/**
	 * 集成
	 * - 遍历所有浅层子hub, 设置父节点为自身
	 */
	relate() {
		DataBindHubHelper.onRelateDialogChild(this)
	}

	derelate() {
		DataBindHubHelper.onDerelateDialogChild(this)
	}

	bindFromParentHub(parentHub: DataBindHub) {
		let subKey = this.subKey || "&this"
		return this._subDataHub.bindFromParentHub(parentHub, subKey)
	}
	unbindFromParentHub() {
		this._subDataHub.unbindFromParentHub()
	}

	private _dialog!: CCDialogComp;
	/**
	 * 寻找寄宿的对话框
	 * @param withoutCache 
	 * @returns 
	 */
	findDialog(withoutCache: boolean = false): CCDialogComp {
		if (!withoutCache) {
			if (this._dialog) {
				return this._dialog
			}
		}
		this._dialog = DialogHelper.findDialogComp(this.node)!
		return this._dialog
	}
}
