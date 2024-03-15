
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace DataBind.UIBind
{
	[AddComponentMenu("DataDrive/NodeLife")]
	public class NodeLifeComp : MyComponent
	{
		protected Transform LastParent = null;
		protected static NodeLifeComp AlteringComp = null;
		private const bool IsLogEnabled = false;
		protected virtual void OnBeforeTransformParentChanged()
		{
			if (IsLogEnabled)
			{
				Debug.Log("OnBeforeTransformParentChanged:" + this.name);
			}
			if (AlteringComp != null)
			{
				return;
			}
			AlteringComp = this;
			LastParent = this.transform.parent;
		}
		protected virtual void OnTransformParentChanged()
		{
			if (IsLogEnabled)
			{
				Debug.Log("OnTransformParentChanged:" + this.name);
			}

			// TODO: 处理父节点没有CCNodeLife托管的情况
			if (AlteringComp != this)
			{
				return;
			}
			AlteringComp = null;

			if (!this.IsPrefab)
			{
				NewlyLoaded.Add(this);
			}
			HandleHierarchyChanging();
		}

		public static void HandleHierarchyChanging()
		{
			var anyThingDirty = true;

			while (anyThingDirty)
			{
				var creatingList = MyComponent.CreatingList;
				anyThingDirty = creatingList.Count > 0;
				while (creatingList.Count > 0)
				{
					var compCreating = creatingList[0];
					creatingList.RemoveAt(0);
					if (!compCreating.IsPrefab)
					{
						compCreating.HandleLoaded();
					}
				}

				anyThingDirty = anyThingDirty | NewlyLoaded.Count > 0;
				while (NewlyLoaded.Count > 0)
				{
					var compLifecycle = NewlyLoaded[0];
					NewlyLoaded.RemoveAt(0);
					if (!compLifecycle.IsPrefab)
					{
						compLifecycle.HandleParentChanged();
					}
				}
			}
		}
		protected virtual void HandleParentChanged()
		{
			var oldParent = LastParent;
			var newParent1 = this.transform.parent;
			LastParent = newParent1;

			if (oldParent != newParent1)
			{
				this.OnParentChanged(newParent1, oldParent);
			}
			else
			{
				var ts = this.transform;
				var surfParent = DataBindHubHelper.SeekSurfParent<NodeLifeComp>(ts);
				if (surfParent != null)
				{
					this.OnAttach();
				}
				else
				{
					this.OnDeattach();
				}
			}
		}
		// protected virtual void OnUpdate()
		// {
		// 	this.handleHierachyChanging();
		// }

		protected override void OnPreload()
		{
			this.Integrate();
		}

        public static readonly List<NodeLifeComp> NewlyLoaded = new List<NodeLifeComp>();
		protected bool IsLoaded = false;
		protected virtual void Integrate()
		{
			if (this.IsLoaded)
			{
				return;
			}
			this.IsLoaded = true;

			// this["_onParentChanged"](this.node.parent, null)
			NodeLifeComp.NewlyLoaded.Add(this);
		}

		/// <summary>
		/// 仅观测
		/// </summary>
		protected Transform NewParent;
		protected virtual void OnParentChanged(Transform newParent0, Transform oldParent)
		{
			this.NewParent = newParent0;
			// Console.Warn("parent-change:", this.name, newParent?.name, oldParent?.name)
			if (newParent0 == null)
			{
				this.OnDeattach();
			}
			else
			{
				this.OnAttach();
			}
		}

		protected override void OnAttach()
		{
			this.GetComponents<MyComponent>().ForEach(comp =>
			{
				if (comp != this)
				{
					comp.OnRequireAttach();
				}
			});
		}

		protected override void OnDeattach()
		{
			this.GetComponents<MyComponent>().ForEach(comp =>
			{
				if (comp != this)
				{
					comp.OnRequireDeattach();
				}
			});
		}

		/**
		 * 节点activeInHierachy变化时, 通知不可见的子节点跟随变更
		 */
		protected virtual void UpdateInactiveChildrenAttach()
		{
			if (!EnableLazyAttach)
			{
				return;
			}
			this.ForEachChildren(child =>
			{
				if (!child.gameObject.activeSelf)
				{
					child.GetComponents<MyComponent>().ForEach(comp =>
					{
						if (comp != this)
						{
							comp.UpdateAttach();
						}
					});
				}
			});
		}
		protected override void OnEnable()
		{
			this.UpdateInactiveChildrenAttach();
		}
		protected override void OnDisable()
		{
			this.UpdateInactiveChildrenAttach();
		}

	}
}
