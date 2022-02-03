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
				this._isCreating = this.markCreating();
			}
		}

		public virtual CCNodeLife ccNodeLife
		{
			get
			{
				return this.GetComponent<CCNodeLife>();
			}
		}

		protected static List<CCMyComponent> _creatingList = new List<CCMyComponent>();
		protected bool _isCreating = false;
		protected virtual bool markCreating()
		{
			CCMyComponent._creatingList.Add(this);
			return true;
		}

		protected virtual void onPreDestroy()
		{

		}

		protected virtual void OnDestroy()
		{
			this._isCreating = false;
			this.onPreDestroy();
		}

		protected virtual void Awake()
		{
			if (this._isCreating)
			{
				this._isCreating = false;
				this.handlePreload();
			}
		}
		public virtual void HandleLoaded()
		{
			if (this._isCreating)
			{
				this._isCreating = false;
				this.handlePreload();
			}
		}

		protected static bool isPreloading = false;
		protected virtual void handlePreload()
		{
			var loading = false;
			if (!isPreloading)
			{
				isPreloading = true;
				loading = true;
			}
			this.onPreload();
			if (loading)
			{
				isPreloading = false;
				loading = false;
				this.afterPreload();
			}
		}
		protected virtual void onPreload()
		{

		}
		protected virtual void afterPreload()
		{
			CCNodeLife.HandleHierachyChanging();
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
		protected virtual void OnEnable()
		{
			this.updateAttach();
		}
		public virtual void onRequireAttach()
		{
			this.isAttachCalled = true;
			this.updateAttach();
		}
		protected virtual void OnDisable()
		{
			this.updateAttach();
		}
		public virtual void onRequireDeattach()
		{
			this.isAttachCalled = false;
			this.updateAttach();
		}

		[HideInInspector]
		public virtual bool IsPrefab { get; set; }
		public virtual void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			this.IsPrefab = UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this);
#else
			this.IsPrefab = false;
#endif
		}

		public virtual void OnAfterDeserialize()
		{
		}
	}
}
