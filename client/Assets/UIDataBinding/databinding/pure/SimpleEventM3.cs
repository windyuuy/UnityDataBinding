
using System;
using System.Linq;
using System.Collections.Generic;

namespace UI.DataBinding
{
	public delegate void EventHandlerMV3<T1, T2, T3>(T1 msg1, T2 msg2, T3 msg3);

	/**
	 * simple event with dispatching multi value
	 */
	public class SimpleEventMV3<T1, T2, T3>
	{
		protected event EventHandlerMV3<T1, T2, T3> _callbacks;
		public EventHandlerMV3<T1, T2, T3> on(EventHandlerMV3<T1, T2, T3> callback)
		{
			this._callbacks += callback;
			return callback;
		}

		public EventHandlerMV3<T1, T2, T3> once(EventHandlerMV3<T1, T2, T3> callback)
		{
			EventHandlerMV3<T1, T2, T3> call = null;
			call = (msg1, msg2, msg3) =>
			{
				this.off(call);
				callback(msg1, msg2, msg3);
			};
			this._callbacks += call;
			return call;
		}

		public void off(EventHandlerMV3<T1, T2, T3> callback)
		{
			this._callbacks -= callback;
		}

		public void emit(T1 msg1, T2 msg2, T3 msg3)
		{
			if (this._callbacks != null)
			{
				this._callbacks(msg1, msg2, msg3);
			}
		}

		public void clear()
		{
			this._callbacks = null;
		}
	}

	public interface ISEventInputMV3<T1, T2, T3>
	{
		void emit(string key, T1 value1, T2 value2, T3 value3);
	}

	public class ISEventCleanInfo3<T1, T2, T3>
	{
		public string key;
		public EventHandlerMV3<T1, T2, T3> callback;
	}

	public interface ISEventOutputMV3<T1, T2, T3>
	{
		ISEventCleanInfo3<T1, T2, T3> on(string key, EventHandlerMV3<T1, T2, T3> callback);
		ISEventCleanInfo3<T1, T2, T3> once(string key, EventHandlerMV3<T1, T2, T3> callback);
		void off(string key, EventHandlerMV3<T1, T2, T3> callback);
	}

}
