
using System.Linq;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	public delegate void EventHandlerMV1<T1>(T1 msg1);

	/**
	 * simple event with dispatching multi value
	 */
	public class SimpleEventMV1<T1>
	{
		protected event EventHandlerMV1<T1> Callbacks;
		public EventHandlerMV1<T1> On(EventHandlerMV1<T1> callback)
		{
			this.Callbacks += callback;
			return callback;
		}

		public EventHandlerMV1<T1> Once(EventHandlerMV1<T1> callback)
		{
			EventHandlerMV1<T1> call = null;
			call = (msg1) =>
			{
				this.Off(call);
				callback(msg1);
			};
			this.Callbacks += call;
			return call;
		}

		public void Off(EventHandlerMV1<T1> callback)
		{
			this.Callbacks -= callback;
		}

		public void Emit(T1 msg1)
		{
			if (this.Callbacks != null)
			{
				this.Callbacks(msg1);
			}
		}

		public void Clear()
		{
			this.Callbacks = null;
		}
	}

	public interface ISEventInputMV1<T1>
	{
		void Emit(string key, T1 value1);
	}

	public class ISEventCleanInfo1<T1>
	{
		public string key;
		public EventHandlerMV1<T1> callback;
	}

	public interface ISEventOutputMV1<T1>
	{
		ISEventCleanInfo1<T1> On(string key, EventHandlerMV1<T1> callback);
		ISEventCleanInfo1<T1> Once(string key, EventHandlerMV1<T1> callback);
		void Off(string key, EventHandlerMV1<T1> callback);
	}

	public class SEventMV1<T1> : ISEventInputMV1<T1>, ISEventOutputMV1<T1>
	{
		protected readonly Dictionary<string, SimpleEventMV1<T1>> events = new Dictionary<string, SimpleEventMV1<T1>>();
		protected readonly SimpleEventMV2<string, T1> anyEvent = new SimpleEventMV2<string, T1>();

		public string[] Keys
		{
			get
			{
				return this.events.Keys.ToArray();
			}
		}

		public ISEventCleanInfo1<T1> On(string key, EventHandlerMV1<T1> callback)
		{
			this.events.TryAdd(key, new SimpleEventMV1<T1>());
			var event1 = this.events[key];
			if (event1 != null)
			{
				event1.On(callback);
			}
			return new ISEventCleanInfo1<T1>()
			{
				key = key,
				callback = callback,
			};
		}

		public ISEventCleanInfo1<T1> Once(string key, EventHandlerMV1<T1> callback)
		{
			EventHandlerMV1<T1> call = null;
			call = (p1) =>
			{
				this.Off(key, call);
				callback(p1);
			};
			this.On(key, call);
			return new ISEventCleanInfo1<T1>()
			{
				key = key,
				callback = call,
			};
		}

		public void Off(string key, EventHandlerMV1<T1> callback)
		{
			if (callback == null)
			{
				this.events.Remove(key);
			}
			else
			{
				var event1 = this.events[key];
				if (event1 != null)
				{
					event1.Off(callback);
				}
			}
		}

		public void Emit(string key, T1 v1)
		{
			this.anyEvent.emit(key, v1);

			if (this.events.TryGetValue(key, out var event1))
			{
				if (event1 != null)
				{
					event1.Emit(v1);
				}
			}
		}

		public EventHandlerMV2<string, T1> OnAnyEvent(EventHandlerMV2<string, T1> callback)
		{
			return this.anyEvent.On(callback);
		}

		public EventHandlerMV2<string, T1> OnceAnyEvent(EventHandlerMV2<string, T1> callback)
		{
			return this.anyEvent.Once(callback);
		}

		public void OffAnyEvent(EventHandlerMV2<string, T1> callback)
		{
			this.anyEvent.Off(callback);
		}

	}
}
