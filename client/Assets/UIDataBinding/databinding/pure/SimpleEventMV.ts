
namespace UI.DataBinding
{
	public delegate void EventHandlerMV<T>(params T[] message);

	/**
	 * simple event with dispatching multi value
	 */
	public class SimpleEventMV<T>
	{
		protected _callbacks: EventHandlerMV<T>[] = []
		on(callback: EventHandlerMV<T>) : EventHandlerMV<T> {
			this._callbacks.push(callback)
			return callback
	}

	once(callback: EventHandlerMV<T>) : EventHandlerMV<T> {
			let call = (...args) => {
				this.off(call)
				return callback(...args)
			}
			this._callbacks.push(call)
			return call
		}

	off(callback: EventHandlerMV<T>)
	{
		// this._callbacks.remove(callback)
		lang.helper.ArrayHelper.remove(this._callbacks, callback)
		}

	emit(...value: T[])
	{
		this._callbacks.concat().forEach((callback) =>
		{
			callback(...value)
			})
		}

	clear()
	{
		this._callbacks.clear()
		}
}

public interface ISEventInputMV<T>
{
	emit(key: string, ...value: T[])
	}

public interface ISEventOutputMV<T>
{
	on(key: string, callback: EventHandlerMV<T>) : { key: string, callback: EventHandlerMV<T> }
	once(key: string, callback: EventHandlerMV<T>) : { key: string, callback: EventHandlerMV<T> }
	off(key: string, callback: EventHandlerMV<T>)
	}

public class SEventMV<T> implements ISEventInputMV<T>, ISEventOutputMV<T>{
		protected _events: {[key: string]: SimpleEventMV<T> } = Object.create(null)
		protected _anyEvent: SimpleEventMV < T > = new SimpleEventMV<T>()

		get keys()
{
	return Object.keys(this._events)
		}

on(key: string, callback: EventHandlerMV<T>): { key: string, callback: EventHandlerMV<T> }
{
	if (!this._events[key])
	{
		this._events[key] = new SimpleEventMV<T>()
			}
	const event = this._events[key]
			if (event) {
				event.on(callback)
			}
	return { key, callback }
}

once(key: string, callback: EventHandlerMV<T>): { key: string, callback: EventHandlerMV<T> }
{
	const call: EventHandlerMV < T > = (...evt) => {
		this.off(key, call)
				callback(...evt)
			}
	this.on(key, call)
			return { key, callback: call }
}

off(key: string, callback: EventHandlerMV<T>) {
	if (callback == undefined)
	{
		delete this._events[key]
			}
	else
	{
		const event = this._events[key]
				if (event) {
					event.off(callback)
				}
	}
}

emit(key: string, ...value: T[]) {
	this._anyEvent.emit(key as any as T, ...value)

			if (this._events[key])
	{
		const event = this._events[key]
				if (event) {
					event.emit(...value)
				}
	}
}

onAnyEvent(callback: EventHandlerMV<T>): EventHandlerMV<T> {
	return this._anyEvent.on(callback)
		}

onceAnyEvent(callback: EventHandlerMV<T>): EventHandlerMV<T> {
	return this._anyEvent.once(callback)
		}

offAnyEvent(callback: EventHandlerMV<T>) {
	return this._anyEvent.off(callback)
		}

	}
}
