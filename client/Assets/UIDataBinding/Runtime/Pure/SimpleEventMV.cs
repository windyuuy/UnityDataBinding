
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
		protected event EventHandlerMV<T> Callbacks;
		public EventHandlerMV<T> On(EventHandlerMV<T> callback)
		{
			this.Callbacks += callback;
			return callback;
		}

		public EventHandlerMV<T> Once(EventHandlerMV<T> callback)
		{
			EventHandlerMV<T> call = null;
			call = (args) =>
			{
				this.Off(call);
				callback(args);
			};
			this.Callbacks += call;
			return call;
		}

		public void Off(EventHandlerMV<T> callback)
		{
			this.Callbacks -= callback;
		}

		public void Emit(params T[] value)
		{
			if (this.Callbacks != null)
			{
				this.Callbacks(value);
			}
		}

		public void Clear()
		{
			this.Callbacks = null;
		}
	}

	public interface ISEventInputMV<T>
	{
		void Emit(string key, params T[] value);
	}

	public class ISEventCleanInfo<T1>
	{
		public string Key;
		public EventHandlerMV<T1> Callback;
	}

	public interface ISEventOutputMV<T>
	{
		ISEventCleanInfo<T> On(string key, EventHandlerMV<T> callback);
		ISEventCleanInfo<T> Once(string key, EventHandlerMV<T> callback);
		void Off(string key, EventHandlerMV<T> callback);
	}

	public class SEventMV<T> : ISEventInputMV<T>, ISEventOutputMV<T>
	{
		protected readonly Dictionary<string, SimpleEventMV<T>> _events = new Dictionary<string, SimpleEventMV<T>>();
		protected readonly SimpleEventMV<T> anyEvent = new SimpleEventMV<T>();

		public string[] Keys
		{
			get
			{
				return this._events.Keys.ToArray();
			}
		}

		public ISEventCleanInfo<T> On(string key, EventHandlerMV<T> callback)
		{
			this._events.TryAdd(key, new SimpleEventMV<T>());
			var event1 = this._events[key];
			if (event1 != null)
			{
				event1.On(callback);
			}
			return new ISEventCleanInfo<T>()
			{
				Key = key,
				Callback = callback,
			};
		}

		public ISEventCleanInfo<T> Once(string key, EventHandlerMV<T> callback)
		{
			EventHandlerMV<T> call = null;
			call = (evt) =>
			{
				this.Off(key, call);
				callback(evt);
			};
			this.On(key, call);
			return new ISEventCleanInfo<T>()
			{
				Key = key,
				Callback = call,
			};
		}

		public void Off(string key, EventHandlerMV<T> callback)
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
					event1.Off(callback);
				}
			}
		}

		public void Emit(string key, params T[] value)
		{
			var pKey = (T)Convert.ChangeType(key, typeof(T));
			var value2 = new T[value.Length + 1];
			Array.Copy(value, 1, value2, 0, value.Length);
			value2[0] = pKey;
			this.anyEvent.Emit(value2);

			if (this._events.TryGetValue(key, out var event1))
			{
				if (event1 != null)
				{
					event1.Emit(value);
				}
			}
		}

		public EventHandlerMV<T> OnAnyEvent(EventHandlerMV<T> callback)
		{
			return this.anyEvent.On(callback);
		}

		public EventHandlerMV<T> OnceAnyEvent(EventHandlerMV<T> callback)
		{
			return this.anyEvent.Once(callback);
		}

		public void OffAnyEvent(EventHandlerMV<T> callback)
		{
			this.anyEvent.Off(callback);
		}

	}
}
