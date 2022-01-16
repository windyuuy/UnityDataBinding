import { DataHub } from "./DataHub"

export interface ISubDataHub {

	oid: number

	setRealDataHub(realDataHub?: DataHub): void
	rawObj?: Object
	realDataHub?: DataHub

	bindDataHost(data?: object): void

}
