
using UnityEngine;
using System;
using System.Collections.Generic;

namespace UI.DataBinding
{
	public interface ICCIntegrate
	{
		void integrate();
		void relate();
		void derelate();
	}

	public interface ICCDataBindHub : ICCIntegrate
	{
		DataBindHub dataBindHub { get; set; }
	}

	public interface ICCDataHost : ICCIntegrate
	{
		vm.IHost dataHost { get; }
		DataHub dataHub { get; set; }
	}

	public interface ICCSubDataHub : ICCIntegrate
	{
		vm.IHost dataHost { get; }
		// ccDataHost: ICCDataHost
		ISubDataHub subDataHub { get; }
	}

	public interface ICCDialogChild : ICCSubDataHub
	{
		bool autoExtendDataSource { get; set; }
		ISEventCleanInfo2<object, object> bindFromParentHub(DataBindHub parentHub);
		void unbindFromParentHub();
	}

	public interface ICCDataBindBase : ICCIntegrate
	{
		DataBind dataBind { get; set; }
		void doBindItems();
		void doUnBindItems();
	}

	public interface ICCContainerCtrl : ICCIntegrate
	{

	}

	public interface ICCContainerBinding : ICCIntegrate
	{
		ContainerBind containerBind { get; set; }
		string bindSubExp { get; set; }
	}

	public class DataBindHubHelper
	{
		public static T seekSurfParent<T>(Transform self) where T : MonoBehaviour
		{
			var parent = self.parent;
			var ccParent = parent.GetComponent<T>();
			while (parent && (!ccParent))
			{
				parent = parent.parent;
				ccParent = parent.GetComponent<T>();
			}
			return ccParent as T;
		}

		public static void onAddDataBindHub(ICCDataBindHub self)
		{
			var comp = self as Component;
			self.dataBindHub.rawObj = self;
			var lifeComp = comp.GetOrAddComponent<CCNodeLife>();
			// lifeComp.integrate()
		}

		public static void onRelateDataBindHub(ICCDataBindHub self)
		{
			var comp = self as Component;
			// CCDataBindBase的数据源, 只能是 CCDataBindBase 或者 CCDataHost
			var ccDataHub = comp.GetComponent("CCDataHost") as ICCDataHost;
			if (ccDataHub != null)
			{
				ccDataHub.dataHub.addBindHub(self.dataBindHub);
			}
			else
			{
				var ccParent = seekSurfParent<CCDataBindHub>(comp.transform);
				ccParent?.dataBindHub.addBindHub(self.dataBindHub);
			}

		}

		public static void onDerelateDataBindHub(ICCDataBindHub self)
		{
			var parentHub = self.dataBindHub.parent;
			if (parentHub != null)
			{
				parentHub.removeBindHub(self.dataBindHub);
			}
		}

		public static void onAddDataHub(ICCDataHost self)
		{
			var selfComp = self as Component;
			// CCDataHub会自动检测和附加CCDataBindHub
			var comp = selfComp.GetOrAddComponent<CCDataBindHub>();
			comp.integrate();
		}

		public static void onRemoveDataHub(ICCDataHost self)
		{
			self.dataHub.unsetDataHost();
		}

		public static void onRelateDataHub(ICCDataHost self)
		{
			var selfComp = self as Component;
			var ccDataBindHub = selfComp.GetComponent<CCDataBindHub>();
			if (ccDataBindHub != null)
			{
				var dataBindHub = ccDataBindHub.dataBindHub;
				self.dataHub.addBindHub(dataBindHub);
			}
			// if (self.dataHost) {
			// 	self.dataHub.setDataHost(self.dataHost)
			// }
			self.dataHub.running = true;
		}

		public static void onDerelateDataHub(ICCDataHost self)
		{
			// self.dataHub.unsetDataHost()
			self.dataHub.running = false;

			var selfComp = self as Component;
			var ccDataBindHub = selfComp.GetComponent<CCDataBindHub>();
			if (ccDataBindHub)
			{
				var dataBindHub = ccDataBindHub.dataBindHub;
				self.dataHub.removeBindHub(dataBindHub);
			}
		}

		public static void onAddSubDataHub(ICCSubDataHub self)
		{
			var selfComp = self as Component;
			var lifeComp = selfComp.GetOrAddComponent<CCNodeLife>();
			// lifeComp.integrate()
			var ccDataHost = selfComp.GetOrAddComponent<CCDataHost>();
			ccDataHost.integrate();
			self.subDataHub.setRealDataHub(ccDataHost.dataHub);
			self.subDataHub.rawObj = self;
		}

		public static void onAddDataBind(ICCDataBindBase self)
		{
			var selfComp = self as Component;
			self.dataBind.rawObj = self;
			var lifeComp = selfComp.GetOrAddComponent<CCNodeLife>();
			// lifeComp.integrate()
		}

		public static void onRelateDataBind(ICCDataBindBase self)
		{
			// CCDataBind会自动检测CCDataBindHub,并附着监听
			var selfComp = self as Component;
			var ccDataBindHub = selfComp.GetComponent<CCDataBindHub>() ?? seekSurfParent<CCDataBindHub>(selfComp.transform);
			if (ccDataBindHub)
			{
				self.dataBind.addBindHub(ccDataBindHub.dataBindHub);
			}
			self.doBindItems();
		}

		public static void onDerelateDataBind(ICCDataBindBase self)
		{
			self.doUnBindItems();
			var parent = self.dataBind.bindHub;
			if (parent != null)
			{
				self.dataBind.removeBindHub(parent);
			}
		}

		public static void onAddContainerBind(ICCContainerBinding self)
		{
			var selfComp = self as Component;
			var lifeComp = selfComp.GetOrAddComponent<CCNodeLife>();
			// lifeComp.integrate()
			// container会自动检测和附加CCDataBindHub
			var comp = selfComp.GetOrAddComponent<CCDataBindHub>();
			comp.integrate();


			var ccContainerCtrl = selfComp.GetOrAddComponent<CCContainerCtrl>();
			ccContainerCtrl.integrate();
		}

		public static void onRelateContainerBind(ICCContainerBinding self)
		{
			// container会自动检测和附加CCDataBindHub
			var selfComp = self as Component;
			var ccDataBindHub = selfComp.GetComponent<CCDataBindHub>();
			if (ccDataBindHub)
			{
				self.containerBind.addBindHub(ccDataBindHub.dataBindHub);
			}
			var ccContainerCtrl = selfComp.GetOrAddComponent<CCContainerCtrl>();
			ccContainerCtrl.relate();
			self.containerBind.bindExpr(self.bindSubExp);
		}

		public static void onDerelateContainerBind(ICCContainerBinding self)
		{
			// container会自动检测和附加CCDataBindHub
			var selfComp = self as Component;
			var ccDataBindHub = selfComp.GetComponent<CCDataBindHub>();
			if (ccDataBindHub)
			{
				self.containerBind.removeBindHub(ccDataBindHub.dataBindHub);
			}
			var ccContainerCtrl = selfComp.GetComponent<CCContainerCtrl>();
			if (ccContainerCtrl && Utils.isValid(ccContainerCtrl, true))
			{
				ccContainerCtrl.derelate();
			}

			self.containerBind.unbindExpr();
		}

		public static void onAddDialogChild(ICCDialogChild self)
		{
			onAddSubDataHub(self);
		}
		public static void onRelateDialogChild(ICCDialogChild self)
		{
			if (self.autoExtendDataSource)
			{
				// CCDataBindBase的数据源, 只能是 CCDataBindBase 或者 CCDataHost
				var selfComp = self as Component;
				var ccParent = seekSurfParent<CCDataBindHub>(selfComp.transform);
				var dataHub = ccParent?.dataBindHub;
				if (dataHub != null)
				{
					self.bindFromParentHub(dataHub);
				}
				else
				{
					self.unbindFromParentHub();
				}
			}
		}
		public static void onDerelateDialogChild(ICCDialogChild self)
		{
			self.unbindFromParentHub();
		}

	}
}
