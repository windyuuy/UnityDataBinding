using UnityEngine;
using UnityEngine.Serialization;

namespace DataBinding.UIBind
{
	/// <summary>
	/// 支持调用 ObserveData 设置独立的数据源，与父数据源隔离
	/// 也支持从父数据源直接延续，增强灵活性
	/// </summary>
	public class CCDialogChild : CCSubDataHub, ICCDialogChild
	{
		[FormerlySerializedAs("_autoExtendDataSource")]
		[Rename("承接父容器数据源")]
		[SerializeField]
		protected bool autoExtendDataSource = false;

		// 承接父容器数据源
		public virtual bool AutoExtendDataSource
		{
			get
			{
				return this.autoExtendDataSource;
			}
			set
			{
				this.autoExtendDataSource = value;
			}
		}

		[FormerlySerializedAs("_subKey")]
		[Rename("绑定子项数据源")]
		[SerializeField]
		private string subKey = "";

		public override string SubKey
		{
			get
			{
				return this.subKey;
			}
			set
			{
				this.subKey = value;
			}
		}

		protected override ISubDataHub subDataHub { get; set; } = new SubDataHub();
		protected virtual SubDataHub RSubDataHub
		{
			get => (SubDataHub)subDataHub;
			set => subDataHub = value;
		}

		/// <summary>
		/// 设置独立的数据源
		/// </summary>
		/// <param name="data"></param>
		public void ObserveData(object data)
		{
			this.RSubDataHub.ObserveData(data);
		}

		public void UnsetDataHost()
		{
			this.RSubDataHub.UnsetDataHost();
		}

		public override void Integrate()
		{
			DataBindHubHelper.OnAddDialogChild(this);
		}

		/**
		 * 集成
		 * - 遍历所有浅层子hub, 设置父节点为自身
		 */
		public override void Relate()
		{
			DataBindHubHelper.OnRelateDialogChild(this);
		}

		public override void Derelate()
		{
			DataBindHubHelper.OnDerelateDialogChild(this);
		}

		public virtual ISEventCleanInfo2<object, object> BindFromParentHub(DataBindHub parentHub)
		{
			var subKey1 = string.IsNullOrEmpty(this.SubKey) ? "&this" : this.SubKey;
			return this.RSubDataHub.BindFromParentHub(parentHub, subKey1);
		}
		public virtual void UnbindFromParentHub()
		{
			this.RSubDataHub.UnbindFromParentHub();
		}

		// private CCDialogComp _dialog;
		// /**
		//  * 寻找寄宿的对话框
		//  * @param withoutCache 
		//  * @returns 
		//  */
		// public CCDialogComp findDialog(bool withoutCache = false)
		// {
		// 	if (!withoutCache)
		// 	{
		// 		if (this._dialog)
		// 		{
		// 			return this._dialog;
		// 		}
		// 	}
		// 	this._dialog = DialogHelper.findDialogComp(this.transform);
		// 	return this._dialog;
		// }
	}
}

