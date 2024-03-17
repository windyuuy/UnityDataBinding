using System;
using System.Collections.Generic;
using System.Linq;

namespace DataBind.UIBind
{
	public interface ISEventInputMV2<T1, T2>
	{
		void Emit(string key, T1 value1, T2 value2);
	}

	public class ISEventCleanInfo2<T1, T2>
	{
		public string Key;
		public EventHandlerMV2<T1, T2> Callback;
	}

	public interface ISEventOutputMV2<T1, T2>
	{
		ISEventCleanInfo2<T1, T2> On(string key, EventHandlerMV2<T1, T2> callback);
		ISEventCleanInfo2<T1, T2> Once(string key, EventHandlerMV2<T1, T2> callback);
		void Off(string key, EventHandlerMV2<T1, T2> callback);
	}
	
	public class SEventMV2<T1, T2> : ISEventInputMV2<T1, T2>, ISEventOutputMV2<T1, T2>
	{
		protected readonly Dictionary<string, SimpleEventMV2<T1, T2>> Events = new Dictionary<string, SimpleEventMV2<T1, T2>>();
		protected readonly SimpleEventMV3<string, T1, T2> AnyEvent = new SimpleEventMV3<string, T1, T2>();

		public string[] Keys
		{
			get
			{
				return this.Events.Keys.ToArray();
			}
		}

		public IEnumerable<(string key, T target)> PeekNamedListeningObject<T>()
		{
			foreach (var target in this.AnyEvent.PeekListeningObject<T>())
			{
				yield return (null, target);
			}

			foreach (var simpleEventMv2 in this.Events)
			{
				var event1 = simpleEventMv2.Value;
				foreach (var obj in event1.PeekListeningObject<T>())
				{
					yield return (simpleEventMv2.Key, obj);
				}
			}
		}
		
		public ISEventCleanInfo2<T1, T2> On(string key, EventHandlerMV2<T1, T2> callback)
		{
			this.Events.TryAdd(key, new SimpleEventMV2<T1, T2>());
			var event1 = this.Events[key];
			if (event1 != null)
			{
				event1.On(callback);
			}
			return new ISEventCleanInfo2<T1, T2>()
			{
				Key = key,
				Callback = callback,
			};
		}

		public ISEventCleanInfo2<T1, T2> Once(string key, EventHandlerMV2<T1, T2> callback)
		{
			throw new Exception("");
			EventHandlerMV2<T1, T2> call = null;
			call = (p1, p2) =>
			{
				this.Off(key, call);
				callback(p1, p2);
			};
			this.On(key, call);
			return new ISEventCleanInfo2<T1, T2>()
			{
				Key = key,
				Callback = call,
			};
		}

		public void Off(string key, EventHandlerMV2<T1, T2> callback)
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

		public void Emit(string key, T1 v1, T2 v2)
		{
			this.AnyEvent.Emit(key, v1, v2);

			if (this.Events.TryGetValue(key, out var event1))
			{
				if (event1 != null)
				{
					event1.Emit(v1, v2);
				}
			}
		}

		public EventHandlerMV3<string, T1, T2> OnAnyEvent(EventHandlerMV3<string, T1, T2> callback)
		{
			return this.AnyEvent.On(callback);
		}

		public EventHandlerMV3<string, T1, T2> OnceAnyEvent(EventHandlerMV3<string, T1, T2> callback)
		{
			return this.AnyEvent.Once(callback);
		}

		public void OffAnyEvent(EventHandlerMV3<string, T1, T2> callback)
		{
			this.AnyEvent.Off(callback);
		}

	}
}