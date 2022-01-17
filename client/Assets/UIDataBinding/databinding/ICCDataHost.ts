
export interface ICCDataHost {

	/**
	 * 检测数据
	 * @param data 
	 */
	observeData(data: Object): void

	/**
	 * 初始化数据绑定组件
	 */
	initAllDataBindComps(): void
}
