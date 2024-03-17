using System.Collections.Generic;
using System.Linq;

namespace DataBind.UIBind
{
	public delegate void EventHandlerMV2<T1, T2>(T1 msg1, T2 msg2);

	/**
	 * simple event with dispatching multi value
	 */
	public class SimpleEventMV2<T1, T2>
	{
		protected event EventHandlerMV2<T1, T2> Callbacks;
		public EventHandlerMV2<T1, T2> On(EventHandlerMV2<T1, T2> callback)
		{
			this.Callbacks += callback;
			return callback;
		}

		public EventHandlerMV2<T1, T2> Once(EventHandlerMV2<T1, T2> callback)
		{
			EventHandlerMV2<T1, T2> call = null;
			call = (msg1, msg2) =>
			{
				this.Off(call);
				callback(msg1, msg2);
			};
			this.Callbacks += call;
			return call;
		}

		public void Off(EventHandlerMV2<T1, T2> callback)
		{
			this.Callbacks -= callback;
		}

		public void Emit(T1 msg1, T2 msg2)
		{
			if (this.Callbacks != null)
			{
				this.Callbacks(msg1, msg2);
			}
		}

		public void Clear()
		{
			this.Callbacks = null;
		}

		public IEnumerable<T> PeekListeningObject<T>()
		{
			if (this.Callbacks == null)
			{
				yield break;
			}
			
			foreach (var delegate1 in this.Callbacks.GetInvocationList())
			{
				if (delegate1.Target is T target)
				{
					yield return target;
				}
				else
				{
					// yield return (T)delegate1.Target;
					var values =delegate1.Target.GetType().GetFields().Select(field => field.GetValue(delegate1.Target));
					foreach (var value in values)
					{
						if (value is T target2)
						{
							yield return target2;
							break;
						}
					}
				}
			}
		}
	}

}
