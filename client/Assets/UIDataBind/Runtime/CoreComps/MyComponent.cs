using UnityEngine;
using System;
using System.Collections.Generic;

namespace DataBind.UIBind
{
	public class MyComponent : MonoBehaviour, ISerializationCallbackReceiver
	{
#if UNITY_EDITOR
		protected static bool IsRuntimeMode = false;
		[UnityEditor.InitializeOnEnterPlayMode]
		static void OnEnterPlayMode()
		{
			IsRuntimeMode = true;
			Application.wantsToQuit += () =>
			{
				IsRuntimeMode = false;
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

		public MyComponent() : base()
		{
			if (IsRuntimeMode)
			{
				this.IsCreating = this.MarkCreating();
			}
		}

		public virtual NodeLifeComp NodeLife
		{
			get
			{
				return this.GetComponent<NodeLifeComp>();
			}
		}

		protected static readonly List<MyComponent> CreatingList = new List<MyComponent>();
		protected bool IsCreating = false;
		protected bool MarkCreating()
		{
			MyComponent.CreatingList.Add(this);
			return true;
		}

		protected virtual void OnPreDestroy()
		{

		}

		protected virtual void OnDestroy()
		{
			this.IsCreating = false;
			this.OnPreDestroy();
		}

		protected virtual void Awake()
		{
			if (this.IsCreating)
			{
				this.IsCreating = false;
				this.HandlePreload();
			}
		}
		public virtual void HandleLoaded()
		{
			if (this.IsCreating)
			{
				this.IsCreating = false;
				this.HandlePreload();
			}
		}

		protected static bool IsPreloading = false;
		protected virtual void HandlePreload()
		{
			var loading = false;
			if (!IsPreloading)
			{
				IsPreloading = true;
				loading = true;
			}
			this.OnPreload();
			if (loading)
			{
				IsPreloading = false;
				loading = false;
				this.AfterPreload();
			}
		}
		protected virtual void OnPreload()
		{

		}
		protected virtual void AfterPreload()
		{
			NodeLifeComp.HandleHierarchyChanging();
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
				if (needAttach && (!this.IsAttached))
				{
					this.IsAttached = true;
					this.OnAttach();
				}
				else if ((!needAttach) && this.IsAttached)
				{
					this.IsAttached = false;
					this.OnDeattach();
				}
			}
			else
			{
				if (this.IsAttachCalled && (!this.IsAttached))
				{
					this.IsAttached = true;
					this.OnAttach();
				}
				else if ((!this.IsAttachCalled) && this.IsAttached)
				{
					this.IsAttached = false;
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
			return this.IsAttachCalled && this.EnabledInHierarchy;
			// && this.node.parent?.activeInHierarchy
		}
		protected bool IsAttachCalled = false;

		protected virtual void OnNotifyAttachedAlready()
		{
			IsAttachCalled = true;
		}
		protected bool IsAttached = false;
		protected virtual void OnEnable()
		{
			this.UpdateAttach();
		}
		public virtual void OnRequireAttach()
		{
			this.IsAttachCalled = true;
			this.UpdateAttach();
		}
		protected virtual void OnDisable()
		{
			this.UpdateAttach();
		}
		public virtual void OnRequireDeattach()
		{
			this.IsAttachCalled = false;
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
