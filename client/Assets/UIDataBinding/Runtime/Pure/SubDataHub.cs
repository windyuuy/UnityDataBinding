
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	using number = System.Double;

	public class SubDataHub : ISubDataHub
	{
		static number _oidAcc = 0;
		public number Oid { get; set; } = SubDataHub._oidAcc++;
		public number index = 0;

		public DataSourceHub RealDataHub { get; set; }
		public Object RawObj { get; set; }

		public void SetRealDataHub(DataSourceHub realDataHub)
		{
			this.RealDataHub = realDataHub;
		}

		public void observeData(object data)
		{
			Debug.Assert(this.RealDataHub != null);
			if (this.RealDataHub != null)
			{
				this.RealDataHub.ObserveData(data);
			}
		}

		public void unsetDataHost()
		{
			if (this.RealDataHub != null)
			{
				this.RealDataHub.UnsetDataHost();
			}
		}

		public void BindDataHost(object data)
		{
			if (data == null)
			{
				this.unsetDataHost();
			}
			else
			{
				this.observeData(data);
			}
		}

		protected ISEventCleanInfo2<object, object> parentHostWatcher;
		protected DataBindHub parentHub;
		public ISEventCleanInfo2<object, object> bindFromParentHub(DataBindHub parentHub, string subKey)
		{
			this.unbindFromParentHub();


			var watcher = parentHub.EaseWatchExprValue(subKey, (value, oldValue) =>
			{
				if (value != null)
				{
					this.observeData(value);
				}
				else
				{
					this.unsetDataHost();
				}
			});
			this.parentHostWatcher = watcher;
			this.parentHub = parentHub;
			return watcher;
		}
		public void unbindFromParentHub()
		{
			var parentHub = this.parentHub;
			if (parentHub != null && this.parentHostWatcher != null)
			{
				var key = this.parentHostWatcher.key;
				var callback = this.parentHostWatcher.callback;

				parentHub.EaseUnWatchExprValue(key, callback);
				this.parentHostWatcher = null;
				this.parentHub = null;
			}
		}

	}
}
