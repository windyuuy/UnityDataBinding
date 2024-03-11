using UnityEngine;
using System;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	public class CCMyComponent : MonoBehaviour, ISerializationCallbackReceiver
	{
#if UNITY_EDITOR
		protected static bool isRuntimeMode = false;
		[UnityEditor.InitializeOnEnterPlayMode]
		static void OnEnterPlayMode()
		{
			isRuntimeMode = true;
			Application.wantsToQuit += () =>
			{
				isRuntimeMode = false;
				return true;
			};
		}
#else
		protected static bool isRuntimeMode=true;
#endif

		/**
		 * 启用延迟链接
		 */
		public static readonly bool EnableLazyAttach = true;

		public CCMyComponent() : base()
		{
			if (isRuntimeMode)
			{
				this.isCreating = this.MarkCreating();
			}
		}

		public virtual CCNodeLife CCNodeLife
		{
			get
			{
				return this.GetComponent<CCNodeLife>();
			}
		}

		protected static readonly List<CCMyComponent> CreatingList = new List<CCMyComponent>();
		protected bool isCreating = false;
		protected virtual bool MarkCreating()
		{
			CCMyComponent.CreatingList.Add(this);
			return true;
		}

		protected virtual void OnPreDestroy()
		{

		}

		protected virtual void OnDestroy()
		{
			this.isCreating = false;
			this.OnPreDestroy();
		}

		protected virtual void Awake()
		{
			if (this.isCreating)
			{
				this.isCreating = false;
				this.HandlePreload();
			}
		}
		public virtual void HandleLoaded()
		{
			if (this.isCreating)
			{
				this.isCreating = false;
				this.HandlePreload();
			}
		}

		protected static bool isPreloading = false;
		protected virtual void HandlePreload()
		{
			var loading = false;
			if (!isPreloading)
			{
				isPreloading = true;
				loading = true;
			}
			this.OnPreload();
			if (loading)
			{
				isPreloading = false;
				loading = false;
				this.AfterPreload();
			}
		}
		protected virtual void OnPreload()
		{

		}
		protected virtual void AfterPreload()
		{
			CCNodeLife.HandleHierarchyChanging();
		}

		protected virtual void OnAttach()
		{

		}

		protected virtual void OnDeattach()
		{

		}

		public virtual void UpdateAttach()
		{
			if (EnableLazyAttach)
			{
				var needAttach = this.NeedAttach();
				if (needAttach && (!this.isAttached))
				{
					this.isAttached = true;
					this.OnAttach();
				}
				else if ((!needAttach) && this.isAttached)
				{
					this.isAttached = false;
					this.OnDeattach();
				}
			}
			else
			{
				if (this.isAttachCalled && (!this.isAttached))
				{
					this.isAttached = true;
					this.OnAttach();
				}
				else if ((!this.isAttachCalled) && this.isAttached)
				{
					this.isAttached = false;
					this.OnDeattach();
				}
			}
		}

		public virtual bool EnabledInHierarchy
		{
			get
			{
				return this.enabled && this.gameObject.activeInHierarchy;
			}
		}
		protected virtual bool NeedAttach()
		{
			return this.isAttachCalled && this.EnabledInHierarchy;
			// && this.node.parent?.activeInHierarchy
		}
		protected bool isAttachCalled = false;

		protected virtual void OnNotifyAttachedAlready()
		{
			isAttachCalled = true;
		}
		protected bool isAttached = false;
		protected virtual void OnEnable()
		{
			this.UpdateAttach();
		}
		public virtual void OnRequireAttach()
		{
			this.isAttachCalled = true;
			this.UpdateAttach();
		}
		protected virtual void OnDisable()
		{
			this.UpdateAttach();
		}
		public virtual void OnRequireDeattach()
		{
			this.isAttachCalled = false;
			this.UpdateAttach();
		}

		[HideInInspector]
		public virtual bool IsPrefab { get; set; }
		public virtual void OnBeforeSerialize()
		{
			if (this == null)
			{
				return;
			}
			
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				this.IsPrefab = UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this)
				                || UnityEditor.PrefabUtility.IsPartOfVariantPrefab(this);
			}
#else
			this.IsPrefab = false;
#endif
		}

		public virtual void OnAfterDeserialize()
		{
		}
	}
}
