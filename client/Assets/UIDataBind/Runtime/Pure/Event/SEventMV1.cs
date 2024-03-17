using System.Collections.Generic;
using System.Linq;

namespace DataBind.UIBind
{
	public class SEventMV1<T1> : ISEventInputMV1<T1>, ISEventOutputMV1<T1>
	{
		protected readonly Dictionary<string, SimpleEventMV1<T1>> Events = new Dictionary<string, SimpleEventMV1<T1>>();
		protected readonly SimpleEventMV2<string, T1> AnyEvent = new SimpleEventMV2<string, T1>();

		public string[] Keys
		{
			get
			{
				return this.Events.Keys.ToArray();
			}
		}

		public ISEventCleanInfo1<T1> On(string key, EventHandlerMV1<T1> callback)
		{
			this.Events.TryAdd(key, new SimpleEventMV1<T1>());
			var event1 = this.Events[key];
			if (event1 != null)
			{
				event1.On(callback);
			}
			return new ISEventCleanInfo1<T1>()
			{
				Key = key,
				Callback = callback,
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
				Key = key,
				Callback = call,
			};
		}

		public void Off(string key, EventHandlerMV1<T1> callback)
		{
			if (callback == null)
			{
				this.Events.Remove(key);
			}
			else
			{
				var event1 = this.Events[key];
				if (event1 != null)
				{
					event1.Off(callback);
				}
			}
		}

		public void Emit(string key, T1 v1)
		{
			this.AnyEvent.Emit(key, v1);

			if (this.Events.TryGetValue(key, out var event1))
			{
				if (event1 != null)
				{
					event1.Emit(v1);
				}
			}
		}

		public EventHandlerMV2<string, T1> OnAnyEvent(EventHandlerMV2<string, T1> callback)
		{
			return this.AnyEvent.On(callback);
		}

		public EventHandlerMV2<string, T1> OnceAnyEvent(EventHandlerMV2<string, T1> callback)
		{
			return this.AnyEvent.Once(callback);
		}

		public void OffAnyEvent(EventHandlerMV2<string, T1> callback)
		{
			this.AnyEvent.Off(callback);
		}

	}
}