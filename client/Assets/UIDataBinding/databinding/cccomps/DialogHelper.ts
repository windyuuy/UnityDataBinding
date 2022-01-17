import { Node } from "cc";
import { CCDialogComp } from "../../CCDialogComp";

export class TDialogHelper {

	/**
	 * 获取当前组件所在节点的host
	 */
	public findDialogComp(curNode: Node): CCDialogComp | null {
		let node: Node | null = curNode

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

}

export const DialogHelper = new TDialogHelper()
