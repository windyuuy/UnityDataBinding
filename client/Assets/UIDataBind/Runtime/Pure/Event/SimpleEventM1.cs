namespace DataBind.UIBind
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
		public string Key;
		public EventHandlerMV1<T1> Callback;
	}

	public interface ISEventOutputMV1<T1>
	{
		ISEventCleanInfo1<T1> On(string key, EventHandlerMV1<T1> callback);
		ISEventCleanInfo1<T1> Once(string key, EventHandlerMV1<T1> callback);
		void Off(string key, EventHandlerMV1<T1> callback);
	}
}
