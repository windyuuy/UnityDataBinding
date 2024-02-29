using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gcc.layer;
using UI.UISys.GameUILayer.Runtime.Input;
using UnityEngine;
using UnityEngine.Serialization;
using console = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace UISys.Runtime
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(TransformTagsComp), typeof(TransformCoverComp))]
	public class UILayer : MonoBehaviour, ILayer, ITransformCoverHandler
	{
		// [SerializeField] [HideInInspector] protected string[] layerTags = new string[0];
		[SerializeField] protected TransformTagsComp tagsComp;

		public virtual string[] LayerTags
		{
			get => tagsComp == null ? Array.Empty<string>() : tagsComp.Tags;
			set => tagsComp.Tags = value;
		}

		private ILayerRoot _layerRoot;

		public virtual ILayerRoot LayerRoot
		{
			get => _layerRoot;
		}

		public virtual TLayerManager LayerManager => LayerRoot.LayerManager;
		
		//#region layer bundle manager

		private UILayerLifecycleDelegate _lifecycleDelegate;
		public string Uri { get; set; }
		public string ResUri { get; set; }
		public virtual ILayerLifecycleInnerCall Lifecycle => _lifecycleDelegate;
		public virtual Task OpeningTask { get; set; }

		public UILayer() : base()
		{
			this.__initConstructor();
		}

		private void __initConstructor()
		{
			this.InitLifecycle();
#if UNITY_EDITOR
			this.InitDefaultProps();
#endif
		}

		protected virtual void InitLifecycle()
		{
			this._lifecycleDelegate = new UILayerLifecycleDelegate
			{
				Layer = this,
			};
		}

		protected virtual void InitDefaultProps()
		{
		}

		public virtual void OnTransformShield()
		{
			this.LayerManager.ShieldLayer(this.Uri);
		}

		public virtual void OnTransformExpose()
		{
			this.LayerManager.ExposeLayer(this.Uri);
		}

		protected void PauseLayer()
		{
			console.LogWarning("[ui] dialog pauseLayer:" + this.name);
			this.gameObject.SetActive(false);
		}

		protected void ResumeLayer()
		{
			// 重新启用渲染被显示的对话框
			Logs.UILogger.Warn("dialog resumeLayer:" + this.name);
			this.gameObject.SetActive(true);
		}

		private TransformCoverComp _transformCoverComp;

		/**
		 * 初次创建调用
		 */
		internal virtual async Task __callOnCreate(object data = null)
		{
			if (this.tagsComp == null)
			{
				this.tagsComp = this.GetComponent<TransformTagsComp>();
			}

			if (_transformCoverComp == null)
			{
				_transformCoverComp = this.GetComponent<TransformCoverComp>();
			}

			if (_transformCoverComp != null)
			{
				_transformCoverComp.Register(this);
			}

			UIInputMG.Shared.TryRegister(this);
			this.IntegrateDataBind();
			await this.OnInit(data);
		}

		/**
		 * 创建对话框完成时调用
		 * @param data
		 */
		protected virtual Task OnInit(object data = null)
		{
			return Task.CompletedTask;
		}

		internal virtual void __callOnDispose()
		{
			this.OnBeforeDestroy();
			this.ClearCoroutines();

			if (_transformCoverComp != null)
			{
				_transformCoverComp.UnRegister(this);
				_transformCoverComp = null;
			}

			this._clearNodePool();
		}

		/**
		 * 调用对话框destroy之前调用
		 */
		protected virtual void OnBeforeDestroy()
		{
		}

		/// <summary>
		/// 可以在此处准备数据
		/// </summary>
		/// <returns></returns>
		protected virtual Task OnPrepare()
		{
			return Task.CompletedTask;
		}

		/**
		 * 初次创建调用
		 */
		internal virtual Task __callOnPrepare()
		{
			var transformParent = this.transform.parent;
			if (transformParent != null)
			{
				var uiLayerRoot = transformParent.GetComponent<UILayerRoot>();
				if (uiLayerRoot != null)
				{
					this._layerRoot = uiLayerRoot.LayerRootConfig;
				}
				else
				{
					this._layerRoot = null;
				}
			}
			else
			{
				this._layerRoot = null;
			}

			return OnPrepare();
		}

		/// <summary>
		/// 可以在此处准备数据
		/// </summary>
		/// <returns></returns>
		protected virtual void OnReady()
		{
		}

		/**
		 * 初次创建调用
		 */
		internal virtual void __callOnReady()
		{
			OnReady();
		}

		internal virtual void __callOnExpose()
		{
			this.ResumeLayer();
			this.OnExpose();
		}

		/**
		 * 每次由隐藏变为显示调用
		 */
		protected virtual void OnExpose()
		{
		}

		/**
		 * 强制隐藏而不关闭对话框
		 */
		internal virtual void __callOnShield()
		{
			this.OnShield();
			this.PauseLayer();
		}

		/**
		 * 每次由显示变为隐藏调用
		 */
		protected virtual void OnShield()
		{
		}

		/**
		 * 关闭对话框=\
		 */
		public virtual Task<ILayer> Close()
		{
			return this.LayerManager.CloseLayer(this.Uri);
		}

		internal virtual async Task __callOnClosing()
		{
			await this.OnClosingAsync();
		}

		protected virtual Task OnClosingAsync()
		{
			return Task.CompletedTask;
		}

		internal virtual void __callOnClosed()
		{
			// console.warn("closeLayer:", this.dialogModel.uri)
			this.StopAllMusics();
			this.OnClosed();
		}

		/**
		 * 关闭调用
		 */
		protected virtual void OnClosed()
		{
		}

		internal virtual async Task __callOnOpening()
		{
			await this.OnOpeningAsync();
		}

		protected virtual Task OnOpeningAsync()
		{
			return Task.CompletedTask;
		}

		internal virtual void __callOnOpened()
		{
			this.OnOpened();
		}

		/**
		 * 强制关闭并销毁对话框自身
		 * @returns
		 */
		public virtual Task Dispose()
		{
			return LayerManager.DestroyLayer(new DestroyLayerParam(this.Uri));
		}

		//#region

		//#region node pool
		/**
		 * 清理使用的节点
		 */
		private void _clearNodePool()
		{
		}

		//#endregion

		//#region data host
		protected virtual void IntegrateDataBind()
		{
		}

		/**
		 * 观测数据
		 * @param data
		 */
		public virtual void ObserveData(Object data, bool updateChildren = true)
		{
		}
		//#endregion

		// #region event binding
		public virtual void DoClose()
		{
			this.Close();
		}
		// #endregion

		//#region audio

		protected virtual void OnOpened()
		{
		}

		//#endregion


		//#region UILog
		protected lang.libs.Logger _logger;

		// 使用get避免没人用浪费内存
		/**
		 * 对话框日志管理
		 */
		public virtual lang.libs.Logger Logger
		{
			get
			{
				return this._logger ?? (
					this._logger = Logs.UILogger
						.Clone()
						.AppendTags(new string[]
						{
							"dialog",
							this.name
						})
				);
			}
		}

		/**
		 * 打印普通日志
		 * @param args
		 */
		public virtual void Log(params object[] args)
		{
			this.Logger.Log(args);
		}

		// debug(...args: any[]) {
		// 	this.logger.debug(...args)
		// }
		// info(...args: any[]) {
		// 	this.logger.info(...args)
		// }
		/**
		 * 打印警告日志
		 * @param args
		 */
		public virtual void Warn(params object[] args)
		{
			this.Logger.Warn(args);
		}

		/**
		 * 打印错误日志
		 * @param args
		 */
		public virtual void Error(params object[] args)
		{
			this.Logger.Error(args);
		}

		/**
		 * 播放游戏结算界面音效
		 * @param key
		 */
		// public virtual void PlayGameResultEffect(string key)
		// {
		//     // AudioManager.playCommonEffect(key, this.node)
		// }

		//#endregion

		#region Coroutine

		protected virtual void ClearCoroutines()
		{
		}

		#endregion

		#region Audios

		protected virtual void StopAllMusics()
		{
		}

		#endregion

		#region HandleInput

		public virtual bool EnableOnKeyback
		{
			get
			{
				if (this is IWithKeyback handler)
				{
					var ret = UIInputMG.Shared.IsOnKeybackEnabled(handler);
					return ret;
				}

				return false;
			}
			set
			{
				if (this is IWithKeyback handler)
				{
					if (value)
					{
						UIInputMG.Shared.RegisterOnKeyback(handler);
					}
					else
					{
						UIInputMG.Shared.UnregisterOnKeyback(handler);
					}
				}
			}
		}

		#endregion
		//
		// private void OnEnable()
		// {
		// 	Debug.Log("uilayer-OnEnable");
		// }
		//
		// private void OnDisable()
		// {
		// 	Debug.Log("uilayer-OnDisable");
		// }
	}
}