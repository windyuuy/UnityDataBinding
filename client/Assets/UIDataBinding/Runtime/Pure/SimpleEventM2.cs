
using System;
using System.Linq;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	public delegate void EventHandlerMV2<T1, T2>(T1 msg1, T2 msg2);

	/**
	 * simple event with dispatching multi value
	 */
	public class SimpleEventMV2<T1, T2>
	{
		protected event EventHandlerMV2<T1, T2> _callbacks;
		public EventHandlerMV2<T1, T2> on(EventHandlerMV2<T1, T2> callback)
		{
			this._callbacks += callback;
			return callback;
		}

		public EventHandlerMV2<T1, T2> once(EventHandlerMV2<T1, T2> callback)
		{
			EventHandlerMV2<T1, T2> call = null;
			call = (msg1, msg2) =>
			{
				this.off(call);
				callback(msg1, msg2);
			};
			this._callbacks += call;
			return call;
		}

		public void off(EventHandlerMV2<T1, T2> callback)
		{
			this._callbacks -= callback;
		}

		public void emit(T1 msg1, T2 msg2)
		{
			if (this._callbacks != null)
			{
				this._callbacks(msg1, msg2);
			}
		}

		public void clear()
		{
			this._callbacks = null;
		}
	}

	public interface ISEventInputMV2<T1, T2>
	{
		void emit(string key, T1 value1, T2 value2);
	}

	public class ISEventCleanInfo2<T1, T2>
	{
		public string key;
		public EventHandlerMV2<T1, T2> callback;
	}

	public interface ISEventOutputMV2<T1, T2>
	{
		ISEventCleanInfo2<T1, T2> on(string key, EventHandlerMV2<T1, T2> callback);
		ISEventCleanInfo2<T1, T2> once(string key, EventHandlerMV2<T1, T2> callback);
		void off(string key, EventHandlerMV2<T1, T2> callback);
	}

	public class SEventMV2<T1, T2> : ISEventInputMV2<T1, T2>, ISEventOutputMV2<T1, T2>
	{
		protected Dictionary<string, SimpleEventMV2<T1, T2>> _events = new Dictionary<string, SimpleEventMV2<T1, T2>>();
		protected SimpleEventMV3<string, T1, T2> _anyEvent = new SimpleEventMV3<string, T1, T2>();

		public string[] keys
		{
			get
			{
				return this._events.Keys.ToArray();
			}
		}

		public ISEventCleanInfo2<T1, T2> on(string key, EventHandlerMV2<T1, T2> callback)
		{
			if (this._events.ContainsKey(key) == false)
			{
				this._events[key] = new SimpleEventMV2<T1, T2>();
			}
			var event1 = this._events[key];
			if (event1 != null)
			{
				event1.on(callback);
			}
			return new ISEventCleanInfo2<T1, T2>()
			{
				key = key,
				callback = callback,
			};
		}

		public ISEventCleanInfo2<T1, T2> once(string key, EventHandlerMV2<T1, T2> callback)
		{
			EventHandlerMV2<T1, T2> call = null;
			call = (p1, p2) =>
			{
				this.off(key, call);
				callback(p1, p2);
			};
			this.on(key, call);
			return new ISEventCleanInfo2<T1, T2>()
			{
				key = key,
				callback = call,
			};
		}

		public void off(string key, EventHandlerMV2<T1, T2> callback)
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

		public void emit(string key, T1 v1, T2 v2)
		{
			this._anyEvent.emit(key, v1, v2);

			if (this._events.ContainsKey(key))
			{
				var event1 = this._events[key];
				if (event1 != null)
				{
					event1.emit(v1, v2);
				}
			}
		}

		public EventHandlerMV3<string, T1, T2> onAnyEvent(EventHandlerMV3<string, T1, T2> callback)
		{
			return this._anyEvent.on(callback);
		}

		public EventHandlerMV3<string, T1, T2> onceAnyEvent(EventHandlerMV3<string, T1, T2> callback)
		{
			return this._anyEvent.once(callback);
		}

		public void offAnyEvent(EventHandlerMV3<string, T1, T2> callback)
		{
			this._anyEvent.off(callback);
		}

	}
}
