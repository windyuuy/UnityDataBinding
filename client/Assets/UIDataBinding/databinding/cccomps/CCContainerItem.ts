import { ccclass } from "../../convenient";
import { ContainerItem } from "../pure/ContainerItem";
import { CCSubDataHub } from "./CCSubDataHub";

@ccclass("CCContainerItem")
export class CCContainerItem extends CCSubDataHub {
	protected _subDataHub: ContainerItem = new ContainerItem();
	public get containerItem(): ContainerItem {
		return this._subDataHub;
	}
	public set containerItem(value: ContainerItem) {
		this._subDataHub = value;
	}

}
