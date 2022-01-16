
/**
 * 用于播放spine动画的设置
 */

import { Animation, sp } from "cc";

export enum TSpineAnimEvent {
	Complete = "Complete",
	Finished = "Finished",
}

export type TSpineAnimCallback = (animName: string, name: TSpineAnimEvent, target: sp.Skeleton | Animation) => void

export class PlaySpineAnimSettings {
	animName: string = "";
	isLoop: boolean = false;
	trackIndex: number = 0;
	// animEvent: fsync.event.SimpleEventMV<any> = new fsync.event.SimpleEventMV()
	callback?: TSpineAnimCallback = undefined
	protected onStop() {
		this.animName = ""
	}

	protected isDirty: number = 0
	public play() {
		this.isDirty++
	}
}
