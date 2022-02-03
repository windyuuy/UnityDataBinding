
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	using number = System.Double;

	public class SubDataHub : ISubDataHub
	{
		static number oidAcc = 0;
		public number oid { get; set; } = SubDataHub.oidAcc++;
		public number index = 0;

		public DataSourceHub realDataHub { get; set; }
		public Object rawObj { get; set; }

		public void setRealDataHub(DataSourceHub realDataHub)
		{
			this.realDataHub = realDataHub;
		}

		public void observeData(object data)
		{
			Debug.Assert(this.realDataHub != null);
			if (this.realDataHub != null)
			{
				this.realDataHub.observeData(data);
			}
		}

		public void unsetDataHost()
		{
			if (this.realDataHub != null)
			{
				this.realDataHub.unsetDataHost();
			}
		}

		public void bindDataHost(object data)
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


			var watcher = parentHub.easeWatchExprValue(subKey, (value, oldValue) =>
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

				parentHub.easeUnWatchExprValue(key, callback);
				this.parentHostWatcher = null;
				this.parentHub = null;
			}
		}

	}
}
