
namespace DataBinding.UIBind
{
	public delegate void EventHandlerMV3<T1, T2, T3>(T1 msg1, T2 msg2, T3 msg3);

	/**
	 * simple event with dispatching multi value
	 */
	public class SimpleEventMV3<T1, T2, T3>
	{
		protected event EventHandlerMV3<T1, T2, T3> Callbacks;
		public EventHandlerMV3<T1, T2, T3> On(EventHandlerMV3<T1, T2, T3> callback)
		{
			this.Callbacks += callback;
			return callback;
		}

		public EventHandlerMV3<T1, T2, T3> Once(EventHandlerMV3<T1, T2, T3> callback)
		{
			EventHandlerMV3<T1, T2, T3> call = null;
			call = (msg1, msg2, msg3) =>
			{
				this.Off(call);
				callback(msg1, msg2, msg3);
			};
			this.Callbacks += call;
			return call;
		}

		public void Off(EventHandlerMV3<T1, T2, T3> callback)
		{
			this.Callbacks -= callback;
		}

		public void Emit(T1 msg1, T2 msg2, T3 msg3)
		{
			if (this.Callbacks != null)
			{
				this.Callbacks(msg1, msg2, msg3);
			}
		}

		public void Clear()
		{
			this.Callbacks = null;
		}
	}

	public interface ISEventInputMV3<T1, T2, T3>
	{
		void Emit(string key, T1 value1, T2 value2, T3 value3);
	}

	public class ISEventCleanInfo3<T1, T2, T3>
	{
		public string Key;
		public EventHandlerMV3<T1, T2, T3> Callback;
	}

	public interface ISEventOutputMV3<T1, T2, T3>
	{
		ISEventCleanInfo3<T1, T2, T3> On(string key, EventHandlerMV3<T1, T2, T3> callback);
		ISEventCleanInfo3<T1, T2, T3> Once(string key, EventHandlerMV3<T1, T2, T3> callback);
		void Off(string key, EventHandlerMV3<T1, T2, T3> callback);
	}

}
