using System;
using UnityEngine;
using UnityEngine.Events;

namespace DataBind.UIBind
{
	[AddComponentMenu("DataDrive/ActionBind")]
	public class ActionBindComp : DataBindBaseComp
	{
		[Serializable]
		public class ActionEvent : UnityEvent<object>
		{
		}

		public string key = "";
		[SerializeField] protected ActionEvent onAction;

		protected override void OnBindItems()
		{
			CheckAction();
		}

		private bool CheckAction()
		{
			if (string.IsNullOrEmpty(this.key))
			{
				return false;
			}

			this.WatchValueChange<bool>(this.key, (newValue, oldValue) =>
			{
				// onAction.Invoke(newValue);
				var count = onAction.GetPersistentEventCount();
				for (var i = 0; i < count; i++)
				{
					((MonoBehaviour)onAction.GetPersistentTarget(i)).SendMessage(onAction.GetPersistentMethodName(i),
						newValue);
				}
			});
			return true;
		}
	}
}