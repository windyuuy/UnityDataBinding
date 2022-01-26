using UnityEngine;
using System;
using System.Collections.Generic;

namespace UI.DataBinding
{
	public class CCMyComponent : MonoBehaviour
	{
		public CCMyComponent() : base()
		{
			this._isCreating = this.markCreating();
		}

		public virtual CCNodeLife ccNodeLife
		{
			get
			{
				return this.GetComponent<CCNodeLife>();
			}
		}

		/**
		 * 启用延迟链接
		 */
		public static readonly bool EnableLazyAttach = true;
		protected static List<CCMyComponent> _creatingList = new List<CCMyComponent>();
		protected bool _isCreating = false;
		protected virtual bool markCreating()
		{
			CCMyComponent._creatingList.Add(this);
			return true;
		}

		// public void _deleteAttr(string attr)
		// {
		// 	delete(this as any)[attr];
		// }

		protected virtual void onPreDestroy()
		{

		}

		protected virtual void OnDestroy()
		{
			this._isCreating = false;
			this.onPreDestroy();
		}

		public virtual void onCreate()
		{
			this.onPreload();
		}
		protected virtual void __preload()
		{
			if (this._isCreating)
			{
				this.onPreload();
			}
		}

		protected virtual void onPreload()
		{

		}

		protected virtual void onAttach()
		{

		}

		protected virtual void onDeattach()
		{

		}

		public virtual void updateAttach()
		{
			if (EnableLazyAttach)
			{
				var needAttach = this.needAttach();
				if (needAttach && (!this.isAttached))
				{
					this.isAttached = true;
					this.onAttach();
				}
				else if ((!needAttach) && this.isAttached)
				{
					this.isAttached = false;
					this.onDeattach();
				}
			}
			else
			{
				if (this.isAttachCalled && (!this.isAttached))
				{
					this.isAttached = true;
					this.onAttach();
				}
				else if ((!this.isAttachCalled) && this.isAttached)
				{
					this.isAttached = false;
					this.onDeattach();
				}
			}
		}

		public virtual bool enabledInHierarchy
		{
			get
			{
				return this.enabled && this.gameObject.activeInHierarchy;
			}
		}
		protected virtual bool needAttach()
		{
			return this.isAttachCalled && this.enabledInHierarchy;
			// && this.node.parent?.activeInHierarchy
		}
		protected bool isAttachCalled = false;
		protected bool isAttached = false;
		protected virtual void onEnable()
		{
			this.updateAttach();
		}
		public virtual void onAfterAttach()
		{
			this.isAttachCalled = true;
			this.updateAttach();
		}
		protected virtual void onDisable()
		{
			this.updateAttach();
		}
		public virtual void onAfterDeattach()
		{
			this.isAttachCalled = false;
			this.updateAttach();
		}

	}
}
