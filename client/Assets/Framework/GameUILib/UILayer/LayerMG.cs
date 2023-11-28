
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RSG;

namespace gcc.layer
{
	using number = System.Double;

	using TLayerUri = System.String;
	using Node = UnityEngine.Transform;
	using boolean = System.Boolean;
	using Prefab = UnityEngine.GameObject;
	using console = UnityEngine.Debug;
	using TLayerOrder = System.Int32;

	/// <summary>
	/// 图层管理配置
	/// </summary>
	public class LayerMGConfig
	{
		/// <summary>
		/// 根组件
		/// </summary>
		public BLayerRoot rootComp;
	}

	/**
	 * 图层管理
	 * - TODO: 支持多实例
	 * - TODO: 支持多组件, 自由管理层级
	 */
	public class LayerMG
	{
		protected Dictionary<string, System.Type/*Component*/> LayerClassMap = new Dictionary<string, System.Type/*Component*/>();
		public void RegisterLayerClass(string key, System.Type/*Component*/ cls)
		{
			this.LayerClassMap[key] = cls;
		}
		public System.Type GetLayerClass(string key)
		{
			return this.LayerClassMap[key];
			//return key;
		}

		protected TagFilter TagFilter = new TagFilter();
		public LayerOrderMG LayerOrderMG = new LayerOrderMG();

		protected BLayerRoot SharedLayerMGComp;
		public Node LayerRoot
		{
			get
			{
				return this.SharedLayerMGComp.transform;

			}
		}
		public BLayerRoot LayerRootComp
		{
			get
			{
				return this.SharedLayerMGComp;
			}
		}

		/**
		 * 加载核心图层配置
		 * @returns 
		 */
		public void LoadMGConfig(LayerMGConfig config)
		{
			this.SharedLayerMGComp = config.rootComp;
		}

		public PPromise<LayerModel> PreloadLayer(string p0, object data = null)
		{
			var p = new ShowLayerParam(p0, data);
			return this.PreloadLayer(p);
		}
		/// <summary>
		/// 预加载资源, 并创建该图层节点, 但不展示
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public PPromise<LayerModel> PreloadLayer(ShowLayerParam p)
		{
			p.HideLoading = p.HideLoading ?? true;

			var task = PPromise.createPPromise<LayerModel>((resolve, reject) =>
			{
				this.GetOrCreateLayer(p).Then((layerModel) =>
				{
					// const node = layerModel.node
					// if (!node.active) {
					// 	node.active = true
					// }
					// if (node.parent == null) {
					// 	node.parent = this.MGConfig.layerRoot
					// }
					resolve(layerModel);
				}, (reason) =>
				{
					reject(reason);
				});
			});
			task.IsWithProgress = true;

			var call = respool.MyNodePool.onProgress(p.ResUri, (count, total) =>
			{
				task.notifyProgress(count, total);
			});
			task.Then((v) =>
			{
				respool.MyNodePool.offProgress(p.ResUri, call);
			}, (reason) =>
			{
				respool.MyNodePool.offProgress(p.ResUri, call);
			});

			return task;
		}

		protected Dictionary<string, IPromise<LayerModel>> loadingLayerTasks = new Dictionary<string, IPromise<LayerModel>>();
		public IPromise<LayerModel> CreateLayer(string p0, LayerModel layerModel0 = null)
		{
			var p = new ShowLayerParam(p0);
			return this.CreateLayer(p, layerModel0);
		}
		public IPromise<LayerModel> CreateLayer(ShowLayerParam p, LayerModel dialogModel0 = null)
		{

			if (this.loadingLayerTasks.ContainsKey(p.Uri))
			{
				return this.loadingLayerTasks[p.Uri]=this.loadingLayerTasks[p.Uri].Then((dialogModel) =>
				{
					replaceModelAttr(dialogModel, p);
					return dialogModel;
				});
			}

			var showLoading = true != p.HideLoading;
			var task = new Promise<LayerModel>((resolve, reject) =>
			{
				if (showLoading == true)
				{
					this.ShowLoading();
				}
				var resUri = p.ResUri;
				respool.MyNodePool.load(resUri, LayerRoot, (dialogNode, err) =>
				{
					if (showLoading)
					{
						this.CloseLoading();
					}

					if (err != null)
					{
						reject(err);
					}
					else
					{
						ILayerInnerCall dialog = dialogNode.GetComponent(this.GetLayerClass("CCLayerComp")) as ILayerInnerCall;
						if (dialog == null)
						{
							var cls = this.GetLayerClass("CCLayerComp");
							dialog = dialogNode.gameObject.AddComponent(cls) as ILayerInnerCall;
							//throw new System.Exception("not implemented");
						}
						dialogNode.gameObject.SetActive(false);

						{
							if (dialogModel0 != null)
							{
								dialog.LayerModel.Destroy();
								dialog.LayerModel = dialogModel0;
							}
							var dialogModel = dialog.LayerModel;
							dialogModel.Comp = dialog as ILayerInnerCall;
							dialogModel.Node = dialogNode;
							dialogModel.TagFilter = this.TagFilter;
							dialogModel.LayerOrderMG = this.LayerOrderMG;
							{
								if (p.Tags != null)
								{
									dialogModel.Tags = new List<TLayerUri>(p.Tags);
								}
								dialogModel.Uri = p.Uri;
								dialogModel.ResUri = p.ResUri;
								dialogModel.Data = p.Data;
							}

							// 记录到层列表
							this._layerList.Add(dialogModel);

							dialog.LayerRootComp = this.LayerRootComp;
							dialog.Lifecycle.__callOnCreate(dialogModel.Data);
							dialogModel.State = LayerState.Inited;

							resolve(dialogModel);
							this.loadingLayerTasks.Remove(p.Uri);
						}

					}
				});
			});

			this.loadingLayerTasks.Add(p.Uri, task);
			return task;
		}

		protected LayerModel _findLayer(string uri)
		{
			var list = this._layerList;
			return list.Find(d => d.Uri == uri);
		}

		public LayerModel FindLayer(string uri)
		{
			return this._findLayer(uri);
		}

		/// <summary>
		/// 界面是否已经打开
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public virtual bool IsOpen(string uri)
		{
			if (_loadingLayers.Contains(uri))
			{
				return true;
			}
			
			var layer = this._findLayer(uri);
			if (layer == null)
			{
				return false;
			}
			return layer.IsOpen;
		}

		/**
		 * 寻找对应资源ID的对话框
		 * @param resUri 
		 * @returns 
		 */
		public LayerModel[] FindLayersByResUri(string resUri)
		{
			return this._layerList.FindAll(d => d.ResUri == resUri).ToArray();
		}

		protected void replaceModelAttr(LayerModel dialogModel, ShowLayerParam p)
		{
			if ((!p.ReuseData) || p.Data != null)
			{
				dialogModel.Data = p.Data;
			}
			if (p.Tags != null)
			{
				dialogModel.Tags = new List<TLayerUri>(p.Tags);
			}
		}

		Promise<LayerModel> GetOrCreateLayer(ShowLayerParam p)
		{
			return new Promise<LayerModel>((resolve, reject) =>
			{
				var isLoaded = !this.loadingLayerTasks.ContainsKey(p.Uri);
				if (isLoaded)
				{
					LayerModel dialogModel = this._findLayer(p.Uri);
					
					if (dialogModel != null)
					{
						if (dialogModel.IsResReleased)
						{
							this.CreateLayer(p).Then((dialogModel) =>
							{
								replaceModelAttr(dialogModel, p);
								resolve(dialogModel);
							}, (reason) =>
							{
								reject(reason);
							});

						}
						else
						{
							replaceModelAttr(dialogModel, p);
							resolve(dialogModel);
						}
					}
					else
					{
						// recreate
						this.CreateLayer(p).Then((dialogModel) =>
						{
							resolve(dialogModel);
						}, (reason) =>
						{
							reject(reason);
						});
					}
				}
				else
				{
					// recreate
					this.CreateLayer(p).Then((dialogModel) =>
					{
						resolve(dialogModel);
					}, (reason) =>
					{
						reject(reason);
					});
				}
			});
		}

		protected HashSet<ILoadingHandler> _loadingDelegate = new HashSet<ILoadingHandler>();

		public void AddLoadingHandler(ILoadingHandler delegatex)
		{
			this._loadingDelegate.Add(delegatex);
		}
		public void RemoveLoadingHandler(ILoadingHandler delegatex)
		{
			this._loadingDelegate.Remove(delegatex);
		}

		/**
		 * 展示加载界面
		 */
		public void ShowLoading(string key = null)
		{
			foreach (var d in this._loadingDelegate)
			{
				try
				{
					d.onShowLoading(key);
				}
				catch (System.Exception e)
				{
					console.LogError(e);
				}
			}
		}

		/**
		 * 关闭加载界面
		 */
		public void CloseLoading(string key = null)
		{
			foreach (var d in this._loadingDelegate)
			{
				try
				{
					// if (d.onHideLoading)
					{
						d.onHideLoading(key);
					}
					// else
					{
						d.onCloseLoading(key);
					}
				}
				catch (System.Exception e)
				{
					console.LogError(e);
				}
			}
		}

		public ILayerInnerCall GetCompOfLayer(Node layer)
		{
			return layer.GetComponent(this.GetLayerClass("CCLayerComp")) as ILayerInnerCall;
		}

		public LayerModel GetTopCover(int index = 0)
		{
			var topIndex = 0;
			var layerModel=ForEachLayers(layerModel =>
			{
				if (layerModel.IsOpen && layerModel.IsCover)
				{
					if (topIndex == index)
					{
						return true;
					}
					else
					{
						topIndex++;
					}
				}

				return false;
			});

			return layerModel;
		}

		public int GetCoverOrder(string uri)
		{
			var coverOrder = -1;
			ForEachLayers(layerModel =>
			{
				if (layerModel.Uri == uri)
				{
					coverOrder++;
					return true;
				}
				else if (layerModel.IsCover)
				{
					coverOrder++;
				}

				return false;
			});
			return coverOrder;
		}

		private void RefreshLayerState()
		{
			//this.refreshing = true;

			var block = false;
			List<LayerModel> tInit = new List<LayerModel>();
			List<LayerModel> tPause = new List<LayerModel>();
			List<LayerModel> tResume = new List<LayerModel>();
			var layerRoot = this.LayerRoot;
			for (var i = layerRoot.childCount - 1; i >= 0; i--)
			{
				// var v = children[i];
				var v = layerRoot.GetChild(i);
				if (!UEUtils.isValid(v, true))
				{
					continue;
				}
				// TODO: 需要支持一个节点上挂载多个???
				var dialogComp = this.GetCompOfLayer(v);
				if (dialogComp != null && dialogComp.LayerModel != null && dialogComp.LayerModel.IsValid)
				{
					var dialog = dialogComp.LayerModel;
					if (dialog.State == LayerState.Inited && dialog.IsOpen)
					{
						// dialog.State = LayerState.Shield;
						tInit.Add(dialog);
					}

					if (block)
					{
						dialog.BeCovered = true;
					}
					else
					{
						dialog.BeCovered = false;
						if (dialog.IsOpen)
						{
							block = dialog.IsCover;
						}
					}

					if (dialog.IsOpen && dialog.BeCovered == false)
					{
						if (dialog.State != LayerState.Exposed)
						{
							tResume.Add(dialog);
						}
					}
					else if (
						// dialog.isOpen == false ||
						dialog.BeCovered
					)
					{
						if (dialog.IsOpen && dialog.State == LayerState.Exposed)
						{
							tPause.Add(dialog);
						}
					}

				}
			}

			tInit.ForEach(v =>
			{
				v.Comp.Lifecycle.__callOnEnter();
				// 此处才引用到 Shield
				v.State = LayerState.Shield;
			});


			tPause.ForEach(v =>
			{
				v.State = LayerState.Shield;
			});
			tResume.ForEach(v =>
			{
				v.State = LayerState.Exposed;
			});
			tPause.ForEach(v =>
			{
				v.Comp.Lifecycle.__callOnShield();
			});
			tResume.ForEach(v =>
			{
				v.Comp.Lifecycle.__callOnExposed();
			});


			//this.refreshing = false;
		}

		private void PostLayerChange(Node dialogNode)
		{
			this.RefreshLayerState();
			this._layerList.ForEach(layer =>
			{
				var comp = layer.Comp;


				// if (comp.__callOnAnyFocusChanged)
				{
					var focus = dialogNode == comp.Node;


					comp.Lifecycle.__callOnAnyFocusChanged(focus);
				}
			});
		}

		/**
		 * 记录的layer列表
		 */
		protected List<LayerModel> _layerList = new List<LayerModel>();
		/**
		 * 记录加载中的列表
		 */
		protected List<string> _loadingLayers = new List<string>();
		protected TLayerOrder _showLayerOrderAcc = 0;
		/**
		 * 整理显示顺序
		 */
		protected void SortShowLayerOrders()
		{
			if (this._loadingLayers.Count > 0)
			{
				return;
			}

			var ls = this._layerList.ToArray();
			Array.Sort(ls, (a, b) =>
			 {
				 return a.ShowLayerOrder - b.ShowLayerOrder;
			 });
			ls.ForEach((v, i) =>
			{
				v.ShowLayerOrder = i;
			});
			this._showLayerOrderAcc = this._layerList.Count;
		}

		/// <summary>
		/// 获取已经打开的所有图层的层级范围(不一定连续)
		/// </summary>
		/// <returns></returns>
		public virtual System.Range GetOpenLayerOrderRange()
		{
			var ls = this._layerList.Where(l => l.IsOpen).Select(l => l.Node.GetSiblingIndex()).ToArray();
			var min = ls.Min();
			var max = ls.Max();
			var range = new System.Range(min, max);
			return range;
		}

		/// <summary>
		/// 获取已经打开的所有图层的层级顺序列表
		/// </summary>
		/// <returns></returns>
		public virtual int[] GetOpenLayerOrders()
		{
			var ls = this._layerList.Where(l => l.IsOpen).Select(l => l.Node.GetSiblingIndex()).ToArray();
			return ls;
		}

		public virtual int GetLayerOrder(string p)
		{
			var layer = this._findLayer(p);
			return layer.Node.GetSiblingIndex();
		}

		#region simplify api
		public Promise<LayerModel> DestroyLayer<T>() where T : class, IShowLayerParams
		{
			var uri = IShowLayerParams.GetUri(typeof(T));
			return this.CloseLayer(uri);
		}
		// public Promise<LayerModel> HideLayer<T>() where T : class, IShowLayerParams
		// {
		// 	var uri = IShowLayerParams.GetUri(typeof(T));
		// 	return this.HideLayer(uri);
		// }
		public Promise<LayerModel> CloseLayer<T>() where T : class, IShowLayerParams
		{
			var uri = IShowLayerParams.GetUri(typeof(T));
			var p = new CloseLayerParam(uri);
			return this.CloseLayer(p);
		}
		public Promise<LayerModel> WaitLayerShown(string p0)
		{
			return new Promise<LayerModel>((resolve, reject) =>
			{
				IEnumerator Detect()
				{
					LayerModel layerModel = null;
					yield return new UnityEngine.WaitUntil(() =>
					{
						layerModel = _findLayer(p0);
						return layerModel == null ? false : layerModel.IsOpen;
					});
					resolve(layerModel);
				}

				LoomMG.sharedLoom.StartCoroutine(Detect());
			});
		}
		public Promise<LayerModel> WaitLayerClosed(string p0)
		{
			return new Promise<LayerModel>((resolve, reject) =>
			{
				IEnumerator Detect()
				{
					LayerModel layerModel = null;
					yield return new UnityEngine.WaitUntil(() =>
					{
						layerModel = _findLayer(p0);
						return layerModel == null ? true : !layerModel.IsOpen;
					});
					resolve(layerModel);
				}

				LoomMG.sharedLoom.StartCoroutine(Detect());
			});
		}
		public Promise<LayerModel> WaitLayerInvisible(string p0)
		{
			return new Promise<LayerModel>((resolve, reject) =>
			{
				IEnumerator Detect()
				{
					LayerModel layerModel = null;
					yield return new UnityEngine.WaitUntil(() =>
					{
						layerModel = _findLayer(p0);
						return layerModel == null ? true : !layerModel.IsOpen || layerModel.BeCovered;
					});
					resolve(layerModel);
				}

				LoomMG.sharedLoom.StartCoroutine(Detect());
			});
		}
		public Promise<LayerModel> WaitLayerVisible(string p0)
		{
			return new Promise<LayerModel>((resolve, reject) =>
			{
				IEnumerator Detect()
				{
					LayerModel layerModel = null;
					yield return new UnityEngine.WaitUntil(() =>
					{
						layerModel = _findLayer(p0);
						return layerModel == null ? false : layerModel.IsOpen && !layerModel.BeCovered;
					});
					resolve(layerModel);
				}

				LoomMG.sharedLoom.StartCoroutine(Detect());
			});
		}
		public Promise<LayerModel> ShowLayer<T>() where T : class, IShowLayerParams
		{
			var p0 = System.Activator.CreateInstance<T>();
			IShowLayerParams.SetUri(p0.Uri, typeof(T));
			var p = new ShowLayerParam(p0.Uri, p0, p0.ResUri);
			return this.ShowLayer(p);
		}
		public Promise<LayerModel> ShowLayer<T>(T p0) where T : IShowLayerParams
		{
			IShowLayerParams.SetUri(p0.Uri, typeof(T));
			var p = new ShowLayerParam(p0.Uri, p0, p0.ResUri);
			return this.ShowLayer(p);
		}
		#endregion simplify api

		public Promise<LayerModel> ShowLayer(string p0, object data)
		{
			var p = new ShowLayerParam(p0, data);
			return this.ShowLayer(p);
		}
		public Promise<LayerModel> ShowLayer(string uri, object data, string resUri)
		{
			var p = new ShowLayerParam(uri, data, resUri);
			return this.ShowLayer(p);
		}
		public Promise<LayerModel> ShowLayer(string p0)
		{
			var p = new ShowLayerParam(p0);
			return this.ShowLayer(p);
		}
		protected Dictionary<TLayerUri, Promise<LayerModel>> _showingTasks = new Dictionary<TLayerUri, Promise<LayerModel>>();
		public Promise<LayerModel> FindShowLayerTask(string uri)
		{
			if (_showingTasks.TryGetValue(uri, out var task))
			{
				return task;
			}
			else
			{
				var layer = this._findLayer(uri);
				if (layer.IsOpen)
				{
					return new Promise<LayerModel>((resolve, reject) => resolve(layer));
				}
				else
				{
					return null;
				}
			}
		}
		public Promise<LayerModel> ShowLayer(ShowLayerParam p)
		{
			this._loadingLayers.Add(p.Uri);
			var showLayerOrder = this._showLayerOrderAcc++;


			var task = new Promise<LayerModel>((resolve, reject) =>
			{
				this.GetOrCreateLayer(p).Then(async (LayerModel dialogModel) =>
				{
					try
					{
						dialogModel.ShowLayerOrder = showLayerOrder;
						if (dialogModel.IsOpen)
						{
							// 调整图层顺序
							UpdateLayerOrder(dialogModel);

							this.PostLayerChange(dialogModel.Node);

							var layerComp0 = dialogModel.Comp;
							if (layerComp0 != null)
							{
								await layerComp0.Lifecycle.__callOnOverlapShow(p);
							}

							resolve(dialogModel);
							return;
						}

						dialogModel.IsOpen = true;

						if (dialogModel.IsShowing)
						{
							resolve(dialogModel);
							return;
						}

						dialogModel.IsShowing = true;

						var layerComp = dialogModel.Comp;
						if (layerComp != null)
						{
							layerComp.LayerRootComp = this.LayerRootComp;
							await layerComp.Lifecycle.__callOnOpening();

							// 调整图层顺序
							UpdateLayerOrder(dialogModel);

							if (!dialogModel.Node.gameObject.activeSelf)
							{
								dialogModel.Node.gameObject.SetActive(true);
							}

							this.PostLayerChange(dialogModel.Node);

							await layerComp.Lifecycle.__callOnShow();
							if (p.OpenInstant)
							{
								this.PostLayerChange(dialogModel.Node);
								dialogModel.IsShowing = false;
								layerComp.Lifecycle.__callOnOpened();
								resolve(dialogModel);
							}
							else
							{
								layerComp.Lifecycle.__callDoOpen(() =>
								{
									this.PostLayerChange(dialogModel.Node);
									dialogModel.IsShowing = false;
									layerComp.Lifecycle.__callOnOpened();
									resolve(dialogModel);
								}, (reason) =>
								{
									reject(reason);
								});
							}
						}
						else
						{
							reject(new System.Exception("no such component"));
						}
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogException(ex);
						throw ex;
					}
				}).Catch((reason) =>
				{
					UnityEngine.Debug.LogError(reason);
					reject(reason);
				});
			});

			this._showingTasks[p.Uri] = task;

			System.Action<LayerModel> onTaskDone = (LayerModel dialogModel) =>
			{
				this._loadingLayers.Remove(p.Uri);
				this.SortShowLayerOrders();
			};
			task.Then(onTaskDone, (err) =>
			{
				onTaskDone(null);
			}, (e) =>
			{
				UnityEngine.Debug.LogError(e);
			}).Finally(() =>
			{
				if (this._showingTasks.ContainsKey(p.Uri))
				{
					this._showingTasks.Remove(p.Uri);
				}
			});

			return task;
		}

		protected void UpdateLayerOrder(LayerModel dialogModel)
		{
			var node = dialogModel.Node;
			// 更新图层顺序
			{
				var parent = this.LayerRoot;
				var order = dialogModel.GetTagOrder();
				var resUriOrder = dialogModel.GetUriOrder();
				var uri = dialogModel.Uri;
				var showLayerOrder0 = dialogModel.ShowLayerOrder;
				var idx = -1;
				foreach (var a in this._layerList)
				{
					if (a.Uri == uri)
					{
						continue;
					}

					var v = a.Node;
					if (a.IsOpen && v.parent)
					{
						var vOrder = a.GetTagOrder();
						var showLayerOrder1 = a.ShowLayerOrder;
						if (vOrder < order
						    || (vOrder == order
						        && (
							        a.GetUriOrder() < resUriOrder
							        || showLayerOrder0 >= showLayerOrder1
						        )
						    )
						   )
						{
							var vIdx = v.GetSiblingIndex();
							if (vIdx > idx)
							{
								idx = vIdx;
							}
						}
					}
				}

				if (node.parent == parent)
				{
					if (node.GetSiblingIndex() <= idx)
					{
						idx = idx - 1;
					}
				}
				else
				{
					//node.parent = parent;
					node.SetParent(parent, false);
				}

				node.SetSiblingIndex(idx + 1);
			}
		}

		// public Promise<LayerModel> HideLayer(string uri)
		// {
		// 	var p = this._findLayer(uri);
		// 	if (p != null)
		// 	{
		// 		return this.HideLayer(p);
		// 	}
		// 	else
		// 	{
		// 		return new Promise<LayerModel>((resolve, reject) => resolve(null));
		// 	}
		// }
		// public Promise<LayerModel> HideLayer(LayerModel uri)
		// {
		// 	var p = new CloseLayerParam(uri.Uri, false);
		// 	return this._closeLayer(uri,p);
		// }

		public Promise<LayerModel> CloseLayer(LayerModel uri0, boolean destroyOnClose = false)
		{
//#if UNITY_IOS && !UNITY_EDITOR
//			destroyOnClose = true;
//#endif
			var p = new CloseLayerParam(uri0.Uri, destroyOnClose);
			return this._closeLayer(uri0, p);
		}
		public Promise<LayerModel> CloseLayer(string uri0, boolean destroyOnClose = false)
		{
//#if UNITY_IOS && !UNITY_EDITOR
//			destroyOnClose = true;
//#endif
			var p = new CloseLayerParam(uri0, destroyOnClose);
			return this._closeLayer(null, p);
		}
		public Promise<LayerModel> CloseLayer(CloseLayerParam uri0)
		{
			return this._closeLayer(null, uri0);
		}
		protected Promise<LayerModel> _closeLayer(LayerModel layerModel, CloseLayerParam p)
		{
			var uri = p.Uri;
			var instant = p.CloseInstant;
			var destroyOnClose = p.DestroyOnClose;

			Promise<LayerModel> waitClose(LayerModel layerModel)
			{
				return new Promise<LayerModel>(async (resolve, reject) =>
				{
					if (layerModel == null)
					{
						console.LogWarning("no layer:" + uri);
						resolve(layerModel);
					}
					else if (!layerModel.IsValid)
					{
						console.LogWarning("invalid layer:" + uri);
						resolve(layerModel);
					}
					else if (!layerModel.IsOpen)
					{
						console.LogWarning("layer not open:" + uri);
						if (layerModel.DestroyOnClose)
						{
							this._doDestroy(layerModel);
						}
						resolve(layerModel);
					}
					else
					{
						layerModel.IsOpen = false;


						layerModel.IsShowing = false;
						if (!layerModel.Node)
						{
							this.doClose(layerModel);
							if (layerModel.DestroyOnClose)
							{
								this._doDestroy(layerModel);
							}
							resolve(layerModel);
						}
						else
						{
							var layerComp = layerModel.Comp;
							if (layerComp != null)
							{
								await layerComp.Lifecycle.__callOnClosing();
								await layerComp.Lifecycle.__callOnHide();
								if (instant)
								{
									layerComp.Lifecycle.__callOnBeforeClosed();
									this.doClose(layerModel);
									layerComp.Lifecycle.__callOnClosed();
									if (layerModel.DestroyOnClose)
									{
										this._doDestroy(layerModel);
									}
									resolve(layerModel);
								}
								else
								{
									layerComp.Lifecycle.__callDoClose(() =>
									{
										layerComp.Lifecycle.__callOnBeforeClosed();
										this.doClose(layerModel);

										layerComp.Lifecycle.__callOnClosed();

										if (layerModel.DestroyOnClose)
										{
											this._doDestroy(layerModel);
										}
										resolve(layerModel);
									}, (reason) =>
									{
										reject(reason);
									});
								}
							}
							else
							{
								this.doClose(layerModel);
								this._doDestroy(layerModel);
								resolve(layerModel);
							}
						}
					}
				});
			};

			if (this.loadingLayerTasks.ContainsKey(p.Uri))
			{
				return new Promise<LayerModel>((resolve, reject) =>
				{
					this.GetOrCreateLayer(new ShowLayerParam(p.Uri,true)).Then((dialogModel1) =>
					{
						layerModel = dialogModel1;
						if (layerModel != null)
						{
							layerModel.DestroyOnClose = destroyOnClose;
						}
						waitClose(layerModel).Then((dialogModel) =>
						{
							resolve(dialogModel);
						}, (reason) =>
						{
							reject(reason);
						});


					}, (reason) =>
					{
						reject(reason);
					});
				});
			}
			else
			{
				layerModel = this._findLayer(p.Uri);
				if (layerModel != null)
				{
					layerModel.DestroyOnClose = destroyOnClose;
				}
				return waitClose(layerModel);
			}

			System.GC.Collect();
		}

		/**
		 * 关闭所有对话框
		 */
		public IPromise<IEnumerable<LayerModel>> CloseAllLayers()
		{
			var tasks = this._layerList.ToArray().Select(d => this.CloseLayer(d));
			var task = Promise<LayerModel>.All(tasks);
			return task;
		}

		public IPromise<LayerModel> DestroyLayer(LayerModel layerModel)
		{
			if (layerModel == null)
			{
				console.LogError($"layerModel is null");
				var cc = (LayerModel)System.Convert.ChangeType(null, typeof(LayerModel));
				var ret = RSG.Promise<LayerModel>.Resolved(cc);
				return ret;
			}
			return this.CloseLayer(layerModel.Uri, true);
		}
		
		public IPromise<LayerModel> DestroyLayer(string uri)
		{
			return this.CloseLayer(uri, true);
		}
		
		public IPromise<IEnumerable<LayerModel>> DestroyAllLayers()
		{
			var tasks = _layerList.ToArray().Select(d => DestroyLayer(d));
			var task = Promise<LayerModel>.All(tasks);
			return task;
		}

		private void doClose(LayerModel v)
		{
			this.PostLayerChange(v.Node);
			if (v.Node && UEUtils.isValid(v.Node, true))
			{
				var root = v.Node;
				root.gameObject.SetActive(false);
			}
			else
			{
				v.IsCancelShowing = true;
			}
		}

		private void _doDestroy(LayerModel v)
		{
			if (v.IsValid && v.Node && UEUtils.isValid(v.Node, true))
			{
				console.Log("do destroy dialog:" + v.Uri);
				var root = v.Node;
				root.gameObject.SetActive(false);
				{
					v.Comp.Lifecycle.__callOnBeforeDestroy();
					respool.MyNodePool.put(root, true);
					// root.parent = null
					// root.destroy()
					this._layerList.Remove(v);
					v.Destroy();
				}
			}
		}

		public LayerModel ForEachLayers(Func<LayerModel,bool> func)
		{
			var layerRoot = this.LayerRoot;
			for (var i = layerRoot.childCount - 1; i >= 0; i--)
			{
				var v = layerRoot.GetChild(i);
				if (!UEUtils.isValid(v, true))
				{
					continue;
				}

				// TODO: 需要支持一个节点上挂载多个???
				var dialogComp = this.GetCompOfLayer(v);
				if (dialogComp != null && dialogComp.LayerModel != null && dialogComp.LayerModel.IsValid)
				{
					var layerModel = dialogComp.LayerModel;
					if (func(layerModel))
					{
						return layerModel;
					}
				}
			}

			return null;
		}

		/**
		 * 尝试释放对话框资源
		 * @param uri 
		 */
		public boolean TryReleaseLayerRes(string uri)
		{
			var p = new ReleaseLayerResParam(uri);
			return this.TryReleaseLayerRes(p);
		}
		public boolean TryReleaseLayerRes(ReleaseLayerResParam p)
		{

			if (this.loadingLayerTasks.ContainsKey(p.uri))
			{
				// TODO: 正在加载中的资源暂时不支持释放
				return false;
			}
			var dialogModel = this._findLayer(p.uri);
			if (dialogModel.IsResReleased)
			{
				// 重复释放, 算释放成功
				return true;
			}
			else
			{
				dialogModel.IsResReleased = true;


				// var prefab = respool.MyNodePool.getPrefab(dialogModel.resUri);


				if (UEUtils.isValid(dialogModel.Node, true))
				{
					// UnityEngine.GameObject.Destroy(dialogModel.node.gameObject);
					respool.MyNodePool.destroyNode(dialogModel.Node);
				}
				else
				{
					console.LogError("prefab destroyed already!!");
				}

				// var canRelease = prefab.refCount == 0;
				// if (canRelease)
				// {
				// 	dialogModel.comp.Lifecycle.__callOnBeforeRelease();
				// 	respool.MyNodePool.tryReleaseAsset(p.resUri);
				// }
				return true;
			}
		}

	}

}
