namespace DataBind.UIBind
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
}
