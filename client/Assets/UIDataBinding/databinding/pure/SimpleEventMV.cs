
using System;
using System.Linq;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	public delegate void EventHandlerMV<T>(params T[] message);

	/**
	 * simple event with dispatching multi value
	 */
	public class SimpleEventMV<T>
	{
		protected event EventHandlerMV<T> _callbacks;
		public EventHandlerMV<T> on(EventHandlerMV<T> callback)
		{
			this._callbacks += callback;
			return callback;
		}

		public EventHandlerMV<T> once(EventHandlerMV<T> callback)
		{
			EventHandlerMV<T> call = null;
			call = (args) =>
			{
				this.off(call);
				callback(args);
			};
			this._callbacks += call;
			return call;
		}

		public void off(EventHandlerMV<T> callback)
		{
			this._callbacks -= callback;
		}

		public void emit(params T[] value)
		{
			if (this._callbacks != null)
			{
				this._callbacks(value);
			}
		}

		public void clear()
		{
			this._callbacks = null;
		}
	}

	public interface ISEventInputMV<T>
	{
		void emit(string key, params T[] value);
	}

	public class ISEventCleanInfo<T1>
	{
		public string key;
		public EventHandlerMV<T1> callback;
	}

	public interface ISEventOutputMV<T>
	{
		ISEventCleanInfo<T> on(string key, EventHandlerMV<T> callback);
		ISEventCleanInfo<T> once(string key, EventHandlerMV<T> callback);
		void off(string key, EventHandlerMV<T> callback);
	}

	public class SEventMV<T> : ISEventInputMV<T>, ISEventOutputMV<T>
	{
		protected Dictionary<string, SimpleEventMV<T>> _events = new Dictionary<string, SimpleEventMV<T>>();
		protected SimpleEventMV<T> _anyEvent = new SimpleEventMV<T>();

		public string[] keys
		{
			get
			{
				return this._events.Keys.ToArray();
			}
		}

		public ISEventCleanInfo<T> on(string key, EventHandlerMV<T> callback)
		{
			if (this._events.ContainsKey(key) == false)
			{
				this._events[key] = new SimpleEventMV<T>();
			}
			var event1 = this._events[key];
			if (event1 != null)
			{
				event1.on(callback);
			}
			return new ISEventCleanInfo<T>()
			{
				key = key,
				callback = callback,
			};
		}

		public ISEventCleanInfo<T> once(string key, EventHandlerMV<T> callback)
		{
			EventHandlerMV<T> call = null;
			call = (evt) =>
			{
				this.off(key, call);
				callback(evt);
			};
			this.on(key, call);
			return new ISEventCleanInfo<T>()
			{
				key = key,
				callback = call,
			};
		}

		public void off(string key, EventHandlerMV<T> callback)
		{
			if (callback == null)
			{
				this._events.Remove(key);
			}
			else
			{
				var event1 = this._events[key];
				if (event1 != null)
				{
					event1.off(callback);
				}
			}
		}

		public void emit(string key, params T[] value)
		{
			var pKey = (T)Convert.ChangeType(key, typeof(T));
			var value2 = new T[value.Length + 1];
			Array.Copy(value, 1, value2, 0, value.Length);
			value2[0] = pKey;
			this._anyEvent.emit(value2);

			if (this._events.ContainsKey(key))
			{
				var event1 = this._events[key];
				if (event1 != null)
				{
					event1.emit(value);
				}
			}
		}

		public EventHandlerMV<T> onAnyEvent(EventHandlerMV<T> callback)
		{
			return this._anyEvent.on(callback);
		}

		public EventHandlerMV<T> onceAnyEvent(EventHandlerMV<T> callback)
		{
			return this._anyEvent.once(callback);
		}

		public void offAnyEvent(EventHandlerMV<T> callback)
		{
			this._anyEvent.off(callback);
		}

	}
}
