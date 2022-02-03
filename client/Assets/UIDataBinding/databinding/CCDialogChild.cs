using UnityEngine;

namespace DataBinding.UIBind
{
	public class CCDialogChild : CCSubDataHub, ICCDialogChild
	{
		[Rename("承接父容器数据源")]
		[SerializeField]
		protected bool _autoExtendDataSource = false;

		// 承接父容器数据源
		public virtual bool autoExtendDataSource
		{
			get
			{
				return this._autoExtendDataSource;
			}
			set
			{
				this._autoExtendDataSource = value;
			}
		}

		[Rename("绑定子项数据源")]
		[SerializeField]
		private string _subKey = "";

		public string subKey
		{
			get
			{
				return this._subKey;
			}
			set
			{
				this._subKey = value;
			}
		}

		protected override ISubDataHub _subDataHub { get; set; } = new SubDataHub();
		protected virtual SubDataHub rSubDataHub
		{
			get => (SubDataHub)_subDataHub;
			set => _subDataHub = value;
		}

		public void observeData(object data)
		{
			this.rSubDataHub.observeData(data);
		}

		public void unsetDataHost()
		{
			this.rSubDataHub.unsetDataHost();
		}

		public override void integrate()
		{
			DataBindHubHelper.onAddDialogChild(this);
		}

		/**
		 * 集成
		 * - 遍历所有浅层子hub, 设置父节点为自身
		 */
		public override void relate()
		{
			DataBindHubHelper.onRelateDialogChild(this);
		}

		public override void derelate()
		{
			DataBindHubHelper.onDerelateDialogChild(this);
		}

		public virtual ISEventCleanInfo2<object, object> bindFromParentHub(DataBindHub parentHub)
		{
			var subKey = string.IsNullOrEmpty(this.subKey) ? "&this" : this.subKey;
			return this.rSubDataHub.bindFromParentHub(parentHub, subKey);
		}
		public virtual void unbindFromParentHub()
		{
			this.rSubDataHub.unbindFromParentHub();
		}

		private CCDialogComp _dialog;
		/**
		 * 寻找寄宿的对话框
		 * @param withoutCache 
		 * @returns 
		 */
		public CCDialogComp findDialog(bool withoutCache = false)
		{
			if (!withoutCache)
			{
				if (this._dialog)
				{
					return this._dialog;
				}
			}
			this._dialog = DialogHelper.findDialogComp(this.transform);
			return this._dialog;
		}
	}
}

