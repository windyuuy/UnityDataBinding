using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace DataBind.UIBind
{
	public class SubDataHub : ISubDataHub
	{
		static long _oidAcc = 0;
		public long Oid { get; set; } = SubDataHub._oidAcc++;
		public long Index = 0;

		public DataSourceHub RealDataHub { get; set; }
		public Object RawObj { get; set; }

		public void SetRealDataHub(DataSourceHub realDataHub)
		{
			this.RealDataHub = realDataHub;
		}

		public void ObserveData(object data)
		{
			Debug.Assert(this.RealDataHub != null);
			if (this.RealDataHub != null)
			{
				this.RealDataHub.ObserveData(data);
			}
		}

		public void UnsetDataHost()
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
				this.UnsetDataHost();
			}
			else
			{
				this.ObserveData(data);
			}
		}

		protected ISEventCleanInfo2<object, object> ParentHostWatcher;
		protected DataBindHub ParentHub;

		public ISEventCleanInfo2<object, object> BindFromParentHub(DataBindHub parentHub, string subKey)
		{
			this.UnbindFromParentHub();


			var watcher = parentHub.EaseWatchExprValue(subKey, (value, oldValue) =>
			{
				if (value != null)
				{
					this.ObserveData(value);
				}
				else
				{
					this.UnsetDataHost();
				}
			});
			this.ParentHostWatcher = watcher;
			this.ParentHub = parentHub;
			return watcher;
		}

		public void UnbindFromParentHub()
		{
			var parentHub = this.ParentHub;
			if (parentHub != null && this.ParentHostWatcher != null)
			{
				var key = this.ParentHostWatcher.Key;
				var callback = this.ParentHostWatcher.Callback;

				parentHub.EaseUnWatchExprValue(key, callback);
				this.ParentHostWatcher = null;
				this.ParentHub = null;
			}
		}

		public List<IDataBindHubTree> Parents { get; } = new();
	}
}