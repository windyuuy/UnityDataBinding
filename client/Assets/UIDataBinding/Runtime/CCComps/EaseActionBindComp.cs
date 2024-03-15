using DataBind.UIBind;
using UnityEngine;
using UnityEngine.Serialization;
using UnityVisualize.Runtime;

namespace DataBind.UIBind
{
	[AddComponentMenu("DataDrive/EaseActionBind")]
	public class EaseActionBindComp : DataBindBaseComp
	{
		[Rename("触发表达式")] [SerializeField] protected string key = "";
		[SerializeField] protected EaseActionsComp actions;

		protected override void OnBindItems()
		{
			CheckUIAction();
		}

		public virtual bool CheckUIAction()
		{
			if (string.IsNullOrEmpty(this.key))
			{
				return false;
			}

			if (actions == null)
			{
				actions = this.GetComponent<EaseActionsComp>();
			}

			if (actions == null)
			{
				return false;
			}

			this.WatchValueChange<bool>(this.key,
				(newValue, oldValue) =>
				{
					actions.RunActionsWithOnePara(newValue);
				});
			return true;
		}
	}
}