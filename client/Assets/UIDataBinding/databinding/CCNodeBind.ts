import { _decorator } from 'cc';
import * as cc from "cc"
import { CCDataBindBase } from "./CCDataBindBase";
import { GameLog } from '../GameLog';
const { ccclass, property, menu } = _decorator;

@ccclass("CCNodeBind")
@menu("DataDrive/CCNodeBind")
export class CCNodeBind extends CCDataBindBase {
    @property(
        {
            type: [cc.CCString],
            displayName: "观测数据"
        }
    )
    observeKeys: string[] = [];

    protected rawData: Map<string, any> = new Map<string, any>();

    protected onBindItems() {
        //更新数据
        if (this.observeKeys.length > 0) {
            for (let key of this.observeKeys) {
                this.watchValueChange<number>(key, (newValue) => {
                    this.rawData.set(key, newValue);
                })
            }
        }
    }

    public getData(key: string) {
        let value = this.rawData.get(key);
        if (value == undefined) {
            GameLog.warn(key + "  is undefined");
        }
        return value;
    }

}