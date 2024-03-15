
using UnityEngine;
using System;
using System.Collections.Generic;
using DataBind;

namespace DataBind.UIBind
{
	public interface ICCIntegrate
	{
		void Integrate();
		void Relate();
		void Derelate();
	}

	public interface ICCDataBindHub : ICCIntegrate
	{
		DataBindHub DataBindHub { get; set; }
	}

	public interface ICCDataHost : ICCIntegrate
	{
		IStdHost DataHost { get; }
		DataSourceHub DataHub { get; set; }
	}

	public interface ICCSubDataHub : ICCIntegrate
	{
		IStdHost DataHost { get; }
		// ccDataHost: ICCDataHost
		ISubDataHub DataHub { get; }
	}

	public interface ICCDialogChild : ICCSubDataHub
	{
		bool AutoExtendDataSource { get; set; }
		ISEventCleanInfo2<object, object> BindFromParentHub(DataBindHub parentHub);
		void UnbindFromParentHub();
	}

	public interface ICCDataBindBase : ICCIntegrate
	{
		DataBind DataBind { get; set; }
		void DoBindItems();
		void DoUnBindItems();
	}

	public interface ICCContainerCtrl : ICCIntegrate
	{

	}

	public interface ICCContainerBinding : ICCIntegrate
	{
		ContainerBind ContainerBind { get; set; }
		string BindSubExp { get; set; }
	}

	public class DataBindHubHelper
	{
		public static T SeekSurfParent<T>(Transform self) where T : Component
		{
			var parent = self.parent;
			var ccParent = parent?.GetComponent<T>();
			while (parent && (ccParent == null))
			{
				parent = parent.parent;
				ccParent = parent?.GetComponent<T>();
			}
			return ccParent as T;
		}

		public static void OnAddDataBindHub(ICCDataBindHub self)
		{
			var comp = self as Component;
			self.DataBindHub.RawObj = self;
			var lifeComp = comp.GetOrAddComponent<NodeLifeComp>();
			// lifeComp.integrate()
		}

		public static void OnRelateDataBindHub(ICCDataBindHub self)
		{
			var comp = self as Component;
			// CCDataBindBase的数据源, 只能是 DataBindCompBase 或者 DataHostComp
			var ccDataHub = comp.GetComponent<DataHostComp>() as ICCDataHost;
			if (ccDataHub != null)
			{
				ccDataHub.DataHub.AddBindHub(self.DataBindHub);
			}
			else
			{
				var ccParent = SeekSurfParent<DataBindHubComp>(comp.transform);
				ccParent?.DataBindHub.addBindHub(self.DataBindHub);
			}

		}

		public static void OnDerelateDataBindHub(ICCDataBindHub self)
		{
			var parentHub = self.DataBindHub.Parent;
			if (parentHub != null)
			{
				parentHub.RemoveBindHub(self.DataBindHub);
			}
		}

		public static void OnAddDataHub(ICCDataHost self)
		{
			var selfComp = self as Component;
			// CCDataHub会自动检测和附加CCDataBindHub
			var comp = selfComp.GetOrAddComponent<DataBindHubComp>();
			comp.Integrate();
		}

		public static void OnRemoveDataHub(ICCDataHost self)
		{
			self.DataHub.UnsetDataHost();
		}

		public static void OnRelateDataHub(ICCDataHost self)
		{
			var selfComp = self as Component;
			var ccDataBindHub = selfComp.GetComponent<DataBindHubComp>();
			if (ccDataBindHub != null)
			{
				var dataBindHub = ccDataBindHub.DataBindHub;
				self.DataHub.AddBindHub(dataBindHub);
			}
			// if (self.dataHost) {
			// 	self.dataHub.setDataHost(self.dataHost)
			// }
			self.DataHub.Running = true;
		}

		public static void OnDerelateDataHub(ICCDataHost self)
		{
			// self.dataHub.unsetDataHost()
			self.DataHub.Running = false;

			var selfComp = self as Component;
			var ccDataBindHub = selfComp.GetComponent<DataBindHubComp>();
			if (ccDataBindHub!=null)
			{
				var dataBindHub = ccDataBindHub.DataBindHub;
				self.DataHub.RemoveBindHub(dataBindHub);
			}
		}

		public static void OnAddSubDataHub(ICCSubDataHub self)
		{
			var selfComp = self as Component;
			var lifeComp = selfComp.GetOrAddComponent<NodeLifeComp>();
			// lifeComp.integrate()
			var ccDataHost = selfComp.GetOrAddComponent<DataHostComp>();
			ccDataHost.Integrate();
			self.DataHub.SetRealDataHub(ccDataHost.DataHub);
			self.DataHub.RawObj = self;
		}

		public static void OnAddDataBind(ICCDataBindBase self)
		{
			var selfComp = self as Component;
			self.DataBind.RawObj = self;
			var lifeComp = selfComp.GetOrAddComponent<NodeLifeComp>();
			// lifeComp.integrate()
		}

		public static void OnRelateDataBind(ICCDataBindBase self)
		{
			// CCDataBind会自动检测CCDataBindHub,并附着监听
			var selfComp = self as Component;
			var ccDataBindHub = selfComp.GetComponent<DataBindHubComp>() ?? SeekSurfParent<DataBindHubComp>(selfComp.transform);
			if (ccDataBindHub)
			{
				self.DataBind.AddBindHub(ccDataBindHub.DataBindHub);
			}
			self.DoBindItems();
		}

		public static void OnDerelateDataBind(ICCDataBindBase self)
		{
			self.DoUnBindItems();
			var parent = self.DataBind.BindHub;
			if (parent != null)
			{
				self.DataBind.RemoveBindHub(parent);
			}
		}

		public static void OnAddContainerBind(ICCContainerBinding self)
		{
			var selfComp = self as Component;
			var lifeComp = selfComp.GetOrAddComponent<NodeLifeComp>();
			// lifeComp.integrate()
			// container会自动检测和附加CCDataBindHub
			var comp = selfComp.GetOrAddComponent<DataBindHubComp>();
			comp.Integrate();


			var ccContainerCtrl = selfComp.GetOrAddComponent<ContainerCtrl>();
			// var ccContainerCtrl = selfComp.GetComponent<ContainerCtrl>();
			ccContainerCtrl.Integrate();
		}

		public static void OnRelateContainerBind(ICCContainerBinding self)
		{
			// container会自动检测和附加CCDataBindHub
			var selfComp = self as Component;
			var ccDataBindHub = selfComp.GetComponent<DataBindHubComp>();
			if (ccDataBindHub)
			{
				self.ContainerBind.AddBindHub(ccDataBindHub.DataBindHub);
			}
			var ccContainerCtrl = selfComp.GetOrAddComponent<ContainerCtrl>();
			// var ccContainerCtrl = selfComp.GetComponent<ContainerCtrl>();
			ccContainerCtrl.Relate();
			self.ContainerBind.BindExpr(self.BindSubExp);
		}

		public static void OnDerelateContainerBind(ICCContainerBinding self)
		{
			// container会自动检测和附加CCDataBindHub
			var selfComp = self as Component;
			var ccDataBindHub = selfComp.GetComponent<DataBindHubComp>();
			if (ccDataBindHub)
			{
				self.ContainerBind.RemoveBindHub(ccDataBindHub.DataBindHub);
			}
			var ccContainerCtrl = selfComp.GetComponent<ContainerCtrl>();
			if (ccContainerCtrl && Utils.IsValid(ccContainerCtrl, true))
			{
				ccContainerCtrl.Derelate();
			}

			self.ContainerBind.UnbindExpr();
		}

		public static void OnAddDialogChild(ICCDialogChild self)
		{
			OnAddSubDataHub(self);
		}
		public static void OnRelateDialogChild(ICCDialogChild self)
		{
			if (self.AutoExtendDataSource)
			{
				// CCDataBindBase的数据源, 只能是 DataBindCompBase 或者 DataHostComp
				var selfComp = self as Component;
				var ccParent = SeekSurfParent<DataBindHubComp>(selfComp.transform);
				var dataHub = ccParent?.DataBindHub;
				if (dataHub != null)
				{
					self.BindFromParentHub(dataHub);
				}
				else
				{
					self.UnbindFromParentHub();
				}
			}
		}
		public static void OnDerelateDialogChild(ICCDialogChild self)
		{
			self.UnbindFromParentHub();
		}

	}
}
