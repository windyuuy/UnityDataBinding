using DataBinding.UIBind;
using UISys.Runtime;
using UnityEngine;

namespace Framework.UISys.GameUILayer.Runtime
{
	public class UIActionBind : CCDataBindBase
	{
		[Rename("触发表达式")] [SerializeField] protected string key = "";
		[SerializeField] protected UIActionsComp uiActions;

		protected override void OnBindItems()
		{
			checkUIAction();
		}

		public virtual bool checkUIAction()
		{
			if (string.IsNullOrEmpty(this.key))
			{
				return false;
			}

			if (uiActions == null)
			{
				uiActions = this.GetComponent<UIActionsComp>();
			}

			if (uiActions == null)
			{
				return false;
			}

			this.WatchValueChange<bool>(this.key,
				(newValue, oldValue) =>
				{
					uiActions.RunActionsWithOnePara(newValue);
				});
			return true;
		}
	}
}