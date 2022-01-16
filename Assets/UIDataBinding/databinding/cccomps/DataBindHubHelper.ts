import { Component, isValid, Node, __private } from "cc";
import { ContainerBind } from "../pure/ContainerBind";
import { DataBind } from "../pure/DataBind";
import { DataBindHub } from "../pure/DataBindHub";
import { DataHub } from "../pure/DataHub";
import { ISubDataHub } from "../pure/ISubDataHub";
import { CCNodeLife } from "./CCNodeLife";

export interface ICCIntegrate {
	integrate(): void
	relate(): void
	derelate(): void
}

export interface ICCDataBindHub extends ICCIntegrate {
	dataBindHub: DataBindHub
}

export interface ICCDataHost extends ICCIntegrate {
	dataHost?: vm.IHost
	dataHub: DataHub
}

export interface ICCSubDataHub extends ICCIntegrate {
	readonly dataHost?: vm.IHost
	// ccDataHost: ICCDataHost
	readonly subDataHub: ISubDataHub
}

export interface ICCDialogChild extends ICCSubDataHub {
	autoExtendDataSource: boolean
	bindFromParentHub(parentHub: DataBindHub): void
	unbindFromParentHub(): void
}

export interface ICCDataBindBase extends ICCIntegrate {
	dataBind: DataBind
	doBindItems(): void
	doUnBindItems(): void
}

export interface ICCContainerCtrl extends ICCIntegrate {

}

export interface ICCContainerBinding extends ICCIntegrate {
	containerBind: ContainerBind
	bindSubExp: string
}

export class TDataBindHubHelper {
	seekSurfParent<T extends Component>(self: Node, cls: __private.Constructor<T>): T
	seekSurfParent<T extends Component>(self: Node, cls: string): T
	seekSurfParent<T extends Component>(self: Node, cls: __private.Constructor<T> | string): T {
		let parent = self.parent
		let ccParent = parent?.getComponent(cls as any)
		while (parent && (!ccParent)) {
			parent = parent.parent
			ccParent = parent?.getComponent(cls as any)
		}
		return ccParent as T
	}

	onAddDataBindHub(self: ICCDataBindHub & Component) {
		self.dataBindHub.rawObj = self
		let lifeComp = self.getOrAddComponent(CCNodeLife)
		// lifeComp.integrate()
	}

	onRelateDataBindHub(self: ICCDataBindHub & Component) {

		// CCDataBindBase的数据源, 只能是 CCDataBindBase 或者 CCDataHost
		let ccDataHub = self.getComponent("CCDataHost") as (ICCDataHost & Component)
		if (ccDataHub) {
			ccDataHub.dataHub.addBindHub(self.dataBindHub)
		} else {
			let ccParent = this.seekSurfParent<ICCDataBindHub & Component>(self.node, "CCDataBindHub")
			ccParent?.dataBindHub.addBindHub(self.dataBindHub)
		}

		// TODO: can skip
		// let parentHub = self.dataBindHub
		// DataBindHubUtils.foreachSurfChildren(self.node, CCDataBindHub, (ccDataBindHub) => {
		// 	parentHub.addBindHub(ccDataBindHub.dataBindHub)
		// })
	}

	onDerelateDataBindHub(self: ICCDataBindHub & Component) {
		let parentHub = self.dataBindHub.parent
		if (parentHub) {
			parentHub.removeBindHub(self.dataBindHub)
		}
	}

	onAddDataHub(self: ICCDataHost & Component) {
		// CCDataHub会自动检测和附加CCDataBindHub
		let comp = self.getOrAddComponent("CCDataBindHub") as (ICCDataBindHub & Component)
		comp.integrate()
	}

	onRemoveDataHub(self: ICCDataHost & Component) {
		self.dataHub.unsetDataHost()
	}

	onRelateDataHub(self: ICCDataHost & Component) {
		let ccDataBindHub = self.getComponent("CCDataBindHub") as (ICCDataBindHub & Component)
		if (ccDataBindHub) {
			let dataBindHub = ccDataBindHub.dataBindHub
			self.dataHub.addBindHub(dataBindHub)
		}
		// if (self.dataHost) {
		// 	self.dataHub.setDataHost(self.dataHost)
		// }
		self.dataHub.running = true
	}

	onDerelateDataHub(self: ICCDataHost & Component) {
		// self.dataHub.unsetDataHost()
		self.dataHub.running = false

		let ccDataBindHub = self.getComponent("CCDataBindHub") as (ICCDataBindHub & Component)
		if (ccDataBindHub) {
			let dataBindHub = ccDataBindHub.dataBindHub
			self.dataHub.removeBindHub(dataBindHub)
		}
	}

	onAddSubDataHub(self: ICCSubDataHub & Component) {
		let lifeComp = self.getOrAddComponent(CCNodeLife)
		// lifeComp.integrate()
		let ccDataHost = self.getOrAddComponent("CCDataHost") as (ICCDataHost & Component)
		ccDataHost.integrate()
		self.subDataHub.setRealDataHub(ccDataHost.dataHub)
		self.subDataHub.rawObj = self
	}

	onAddDataBind(self: ICCDataBindBase & Component) {
		self.dataBind.rawObj = self
		let lifeComp = self.getOrAddComponent(CCNodeLife)
		// lifeComp.integrate()
	}

	onRelateDataBind(self: ICCDataBindBase & Component) {
		// CCDataBind会自动检测CCDataBindHub,并附着监听
		let ccDataBindHub = self.getComponent("CCDataBindHub") as (ICCDataBindHub & Component) ?? this.seekSurfParent<ICCDataBindHub & Component>(self.node, "CCDataBindHub")
		if (ccDataBindHub) {
			self.dataBind.addBindHub(ccDataBindHub.dataBindHub)
		}
		self["doBindItems"]()
	}

	onDerelateDataBind(self: ICCDataBindBase & Component) {
		self.doUnBindItems()
		let parent = self.dataBind.bindHub
		if (parent) {
			self.dataBind.removeBindHub(parent)
		}
	}

	onAddContainerBind(self: ICCContainerBinding & Component) {
		let lifeComp = self.getOrAddComponent(CCNodeLife)
		// lifeComp.integrate()
		// container会自动检测和附加CCDataBindHub
		let comp = self.getOrAddComponent("CCDataBindHub") as (ICCDataBindHub & Component)
		comp.integrate()

		let ccContainerCtrl = self.getOrAddComponent("CCContainerCtrl") as ICCContainerCtrl & Component
		ccContainerCtrl.integrate()
	}

	onRelateContainerBind(self: ICCContainerBinding & Component) {
		// container会自动检测和附加CCDataBindHub
		let ccDataBindHub = self.getComponent("CCDataBindHub") as (ICCDataBindHub & Component)
		if (ccDataBindHub) {
			self.containerBind.addBindHub(ccDataBindHub.dataBindHub)
		}
		let ccContainerCtrl = self.getOrAddComponent("CCContainerCtrl") as ICCContainerCtrl & Component
		ccContainerCtrl.relate()
		self.containerBind.bindExpr(self.bindSubExp)
	}

	onDerelateContainerBind(self: ICCContainerBinding & Component) {
		// container会自动检测和附加CCDataBindHub
		let ccDataBindHub = self.getComponent("CCDataBindHub") as (ICCDataBindHub & Component)
		if (ccDataBindHub) {
			self.containerBind.removeBindHub(ccDataBindHub.dataBindHub)
		}
		let ccContainerCtrl = self.getComponent("CCContainerCtrl") as ICCContainerCtrl & Component
		if (ccContainerCtrl && isValid(ccContainerCtrl, true)) {
			ccContainerCtrl.derelate()
		}

		self.containerBind.unbindExpr()
	}

	onAddDialogChild(self: ICCDialogChild & Component) {
		this.onAddSubDataHub(self)
	}
	onRelateDialogChild(self: ICCDialogChild & Component) {
		if (self.autoExtendDataSource) {
			// CCDataBindBase的数据源, 只能是 CCDataBindBase 或者 CCDataHost
			let ccParent = this.seekSurfParent<ICCDataBindHub & Component>(self.node, "CCDataBindHub")
			let dataHub = ccParent?.dataBindHub
			if (dataHub) {
				self.bindFromParentHub(dataHub)
			} else {
				self.unbindFromParentHub()
			}
		}
	}
	onDerelateDialogChild(self: ICCDialogChild & Component) {
		self.unbindFromParentHub()
	}

}

export const DataBindHubHelper = new TDataBindHubHelper()
