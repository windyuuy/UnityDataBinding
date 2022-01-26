
using System;
using System.Linq;
using System.Collections.Generic;

namespace UI.DataBinding
{
	public delegate void EventHandlerMV1<T1>(T1 msg1);

	/**
	 * simple event with dispatching multi value
	 */
	public class SimpleEventMV1<T1>
	{
		protected event EventHandlerMV1<T1> _callbacks;
		public EventHandlerMV1<T1> on(EventHandlerMV1<T1> callback)
		{
			this._callbacks += callback;
			return callback;
		}

		public EventHandlerMV1<T1> once(EventHandlerMV1<T1> callback)
		{
			EventHandlerMV1<T1> call = null;
			call = (msg1) =>
			{
				this.off(call);
				callback(msg1);
			};
			this._callbacks += call;
			return call;
		}

		public void off(EventHandlerMV1<T1> callback)
		{
			this._callbacks -= callback;
		}

		public void emit(T1 msg1)
		{
			if (this._callbacks != null)
			{
				this._callbacks(msg1);
			}
		}

		public void clear()
		{
			this._callbacks = null;
		}
	}

	public interface ISEventInputMV1<T1>
	{
		void emit(string key, T1 value1);
	}

	public class ISEventCleanInfo1<T1>
	{
		public string key;
		public EventHandlerMV1<T1> callback;
	}

	public interface ISEventOutputMV1<T1>
	{
		ISEventCleanInfo1<T1> on(string key, EventHandlerMV1<T1> callback);
		ISEventCleanInfo1<T1> once(string key, EventHandlerMV1<T1> callback);
		void off(string key, EventHandlerMV1<T1> callback);
	}

	public class SEventMV1<T1> : ISEventInputMV1<T1>, ISEventOutputMV1<T1>
	{
		protected Dictionary<string, SimpleEventMV1<T1>> _events = new Dictionary<string, SimpleEventMV1<T1>>();
		protected SimpleEventMV2<string, T1> _anyEvent = new SimpleEventMV2<string, T1>();

		public string[] keys
		{
			get
			{
				return this._events.Keys.ToArray();
			}
		}

		public ISEventCleanInfo1<T1> on(string key, EventHandlerMV1<T1> callback)
		{
			if (this._events[key] == null)
			{
				this._events[key] = new SimpleEventMV1<T1>();
			}
			var event1 = this._events[key];
			if (event1 != null)
			{
				event1.on(callback);
			}
			return new ISEventCleanInfo1<T1>()
			{
				key = key,
				callback = callback,
			};
		}

		public ISEventCleanInfo1<T1> once(string key, EventHandlerMV1<T1> callback)
		{
			EventHandlerMV1<T1> call = null;
			call = (p1) =>
			{
				this.off(key, call);
				callback(p1);
			};
			this.on(key, call);
			return new ISEventCleanInfo1<T1>()
			{
				key = key,
				callback = call,
			};
		}

		public void off(string key, EventHandlerMV1<T1> callback)
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

		public void emit(string key, T1 v1)
		{
			this._anyEvent.emit(key, v1);

			if (this._events[key] != null)
			{
				var event1 = this._events[key];
				if (event1 != null)
				{
					event1.emit(v1);
				}
			}
		}

		public EventHandlerMV2<string, T1> onAnyEvent(EventHandlerMV2<string, T1> callback)
		{
			return this._anyEvent.on(callback);
		}

		public EventHandlerMV2<string, T1> onceAnyEvent(EventHandlerMV2<string, T1> callback)
		{
			return this._anyEvent.once(callback);
		}

		public void offAnyEvent(EventHandlerMV2<string, T1> callback)
		{
			this._anyEvent.off(callback);
		}

	}
}
