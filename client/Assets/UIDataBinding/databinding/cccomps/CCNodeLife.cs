
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UI.DataBinding
{
	public class CCNodeLife : CCMyComponent
	{
		protected static Transform lastParent = null;
		protected static CCNodeLife alteringComp = null;
		protected virtual void OnBeforeTransformParentChanged()
		{
			Debug.Log("OnBeforeTransformParentChanged:" + this.name);
			if (alteringComp != null)
			{
				return;
			}
			alteringComp = this;
			lastParent = this.transform.parent;
		}
		protected virtual void OnTransformParentChanged()
		{
			Debug.Log("OnTransformParentChanged:" + this.name);
			if (alteringComp != this)
			{
				return;
			}
			alteringComp = null;

			newlyLoaded.Add(this);
			this.handleHierachyChanging();
		}

		protected virtual void handleHierachyChanging()
		{
			var anyThingDirty = true;

			while (anyThingDirty)
			{
				var creatingList = CCMyComponent._creatingList;
				anyThingDirty = creatingList.Count > 0;
				while (creatingList.Count > 0)
				{
					var comp = creatingList[0];
					creatingList.RemoveAt(0);
					comp.HandleLoaded();
				}

				anyThingDirty = anyThingDirty | newlyLoaded.Count > 0;
				while (newlyLoaded.Count > 0)
				{
					var comp = newlyLoaded[0];
					newlyLoaded.RemoveAt(0);
					ccNodeLife.handleParentChanged();
				}
			}
		}
		protected virtual void handleParentChanged()
		{
			var oldParent = lastParent;
			var newParent = this.transform.parent;
			lastParent = newParent;

			if (oldParent != newParent)
			{
				this._onParentChanged(newParent, oldParent);
			}
		}
		// protected virtual void OnUpdate()
		// {
		// 	this.handleHierachyChanging();
		// }

		protected override void onPreload()
		{
			this.integrate();
		}
		public static List<CCNodeLife> newlyLoaded = new List<CCNodeLife>();
		protected bool isLoaded = false;
		protected virtual void integrate()
		{
			if (this.isLoaded)
			{
				return;
			}
			this.isLoaded = true;

			// this["_onParentChanged"](this.node.parent, null)
			CCNodeLife.newlyLoaded.Add(this);
		}

		/// <summary>
		/// 仅观测
		/// </summary>
		protected Transform _newParent;
		protected virtual void _onParentChanged(Transform newParent, Transform oldParent)
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
					comp.onRequireAttach();
				}
			});
		}

		protected override void onDeattach()
		{
			this.GetComponents<CCMyComponent>().ForEach(comp =>
			{
				if (comp != this)
				{
					comp.onRequireDeattach();
				}
			});
		}

		/**
		 * 节点activeInHierachy变化时, 通知不可见的子节点跟随变更
		 */
		protected virtual void updateInactiveChildrenAttach()
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
