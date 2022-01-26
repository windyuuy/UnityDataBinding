
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UI.DataBinding
{
	public class CCNodeLife : CCMyComponent
	{
		protected override void onPreload()
		{
			this.integrate();
		}
		public static List<CCNodeLife> newlyLoaded = new List<CCNodeLife>();
		protected bool isLoaded = false;
		protected void integrate()
		{
			if (this.isLoaded)
			{
				return;
			}
			this.isLoaded = true;

			// this["_onParentChanged"](this.node.parent, null)
			CCNodeLife.newlyLoaded.Add(this);
		}

		protected Transform _newParent;
		protected void _onParentChanged(Transform newParent, Transform oldParent)
		{
			this._newParent = newParent;
			// console.warn("parent-change:", this.name, newParent?.name, oldParent?.name)
			if (newParent == null)
			{
				this.onDeattach();
			}
			else
			{
				this.onAttach();
			}
		}

		protected override void onAttach()
		{
			this.GetComponents<CCMyComponent>().ForEach(comp =>
			{
				if (comp != this)
				{
					comp.onAfterAttach();
				}
			});
		}

		protected override void onDeattach()
		{
			this.GetComponents<CCMyComponent>().ForEach(comp =>
			{
				if (comp != this)
				{
					comp.onAfterDeattach();
				}
			});
		}

		/**
		 * 节点activeInHierachy变化时, 通知不可见的子节点跟随变更
		 */
		protected void updateInactiveChildrenAttach()
		{
			if (!EnableLazyAttach)
			{
				return;
			}
			this.ForEachChildren(child =>
			{
				if (!child.gameObject.activeSelf)
				{
					child.GetComponents<CCMyComponent>().ForEach(comp =>
					{
						if (comp != this)
						{
							comp.updateAttach();
						}
					});
				}
			});
		}
		protected override void onEnable()
		{
			this.updateInactiveChildrenAttach();
		}
		protected override void onDisable()
		{
			this.updateInactiveChildrenAttach();
		}

	}
}
