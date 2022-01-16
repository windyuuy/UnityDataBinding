import { Component, Node } from "cc";
import { ccclass } from "../../convenient";

@ccclass("CCLifeStat")
export class CCLifeStat extends Component {
	onLoad() {
		this.node.on("parent-changed", (target: Node) => {
			console.log("parent-changed:", target.name, target.active, this.name)
		})
	}

}
