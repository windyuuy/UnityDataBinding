﻿using System;
using System.Collections.Generic;
using DataBinding.UIBind;
using UnityEngine;

namespace UIDataBinding.UIATool
{
	public class UIDataBindScaner
	{
		public class Utils
		{
			public static string CombineKeys(string a1, string a2)
			{
				if (string.IsNullOrEmpty(a1))
				{
					return a2;
				}
				else
				{
					return $"{a1}.{a2}";
				}
			}
		}

		public void ForeachNodes(Transform parent, Func<Transform, (bool,long?)> handler, Action<Transform,long?> leave)
		{
			for (var i = 0; i < parent.childCount; i++)
			{
				var child = parent.GetChild(i);
				var (needLeave,oid) = handler(child);
				ForeachNodes(child, handler, leave);
				if (needLeave)
				{
					leave(child,oid);
				}
			}
		}

		public class DataHostHandler
		{
			public static long OidAcc = 0;
			public long Oid = ++OidAcc;
			
			public DataHostHandler Parent;
			public DataHostHandler ParentContainer;
			public CCDataHost DataHost;
			public CCSubDataHub SubDataHost;
			public CCContainerBind ContainerBind;

			public string HostKey = "";
			public DataHostHandler(DataHostHandler parent, CCDataHost dataHost)
			{
				this.Parent = parent;
				this.DataHost = dataHost;

				this.HostKey = "";
			}
			public DataHostHandler(DataHostHandler parent, DataHostHandler containerHandler, CCContainerItem dataHost)
			{
				this.Parent = parent;
				this.SubDataHost = dataHost;

				var subExp = containerHandler?.HostKey;
				var index = dataHost.ContainerItem.Index;
				HostKey = Utils.CombineKeys(subExp,index.ToString());
			}
			public DataHostHandler(DataHostHandler parent, CCDialogChild dataHost)
			{
				this.Parent = parent;
				this.SubDataHost = dataHost;

				if (dataHost.AutoExtendDataSource)
				{
					HostKey = parent?.HostKey??"";
				}
				else
				{
					HostKey = "";
				}
			}
			public DataHostHandler(DataHostHandler parent, CCContainerBind dataHost)
			{
				this.Parent = parent;
				this.ContainerBind = dataHost;

				HostKey = Utils.CombineKeys(parent?.HostKey, dataHost.BindSubExp+"[0]");
			}
		}

		public void HandlerComp(List<string> hostKeys, DataHostHandler dataHostHandler, Transform node)
		{
			var hostKey = dataHostHandler?.HostKey ?? "";
			// handle ui bind comps
			var bindComps = node.GetComponents<CCDataBindBase>();
			for (var i = 0; i < bindComps.Length; i++)
			{
				var bindComp = bindComps[i];
				if (bindComp is CCSimpleBind ccSimpleBind)
				{
					hostKeys.Add(Utils.CombineKeys(hostKey,ccSimpleBind.key));
				}
				else if (bindComp is CCButtonBind ccButtonBind)
				{
					hostKeys.Add(Utils.CombineKeys(hostKey,ccButtonBind.kInteractive));
					hostKeys.Add(Utils.CombineKeys(hostKey,ccButtonBind.kToGray));
					foreach (var ccSimpleBindClickFuncInfo in ccButtonBind.clickTriggers)
					{
						hostKeys.Add(Utils.CombineKeys(hostKey,ccSimpleBindClickFuncInfo.callExpr));
					}
				}
				else if (bindComp is CCActiveBind ccActiveBind)
				{
					hostKeys.Add(Utils.CombineKeys(hostKey,ccActiveBind.visible));
				}
				else if (bindComp is CCToggleBind ccToggleBind)
				{
					hostKeys.Add(Utils.CombineKeys(hostKey,ccToggleBind.kIsChecked));
				}
				else
				{
					// TODO: handle CCDialogChild
					Debug.LogError("invalid comp to scane");
				}
			}
		}
		
		public List<string> ScanBindSentences(Transform root)
		{
			var hostKeys = new List<string>();
			
			DataHostHandler curContainer = null;
			var dataHost0 = root.GetComponent<CCDataHost>();
			DataHostHandler curDataHost = new DataHostHandler(null, dataHost0);
			ForeachNodes(root, (node) =>
			{
				var newHostGened = false;
				var containerBind = node.GetComponent<CCContainerBind>();
				if (containerBind != null)
				{
					curDataHost = new DataHostHandler(curContainer, containerBind);
					newHostGened = true;
					
					curDataHost.ParentContainer = curContainer;
					curContainer = curDataHost;
				}
				else
				{
					var containerItem = node.GetComponent<CCContainerItem>();
					if (containerItem != null)
					{
						curDataHost = new DataHostHandler(curDataHost, curContainer, containerItem);
						newHostGened = true;
					}
					else
					{
						var childDialog = node.GetComponent<CCDialogChild>();
						if (childDialog != null)
						{
							curDataHost = new DataHostHandler(curDataHost, childDialog);
							newHostGened = true;
						}
						else
						{
							var subDataHub = node.GetComponent<CCSubDataHub>();
							if (subDataHub != null)
							{
								// curDataHost = new DataHostHandler(curDataHost, subDataHub);
								throw new NotImplementedException($"invalid subdatahub type: {subDataHub.GetType().FullName}");
							}
						}
					}
				}

				if (newHostGened && curDataHost != null)
				{
					hostKeys.Add(curDataHost.HostKey);
				}
				
				HandlerComp(hostKeys, curDataHost, node);
				
				return (newHostGened, curDataHost?.Oid);
			}, (node, oid) =>
			{
				if (curDataHost == curContainer && curContainer != null)
				{
					curContainer = curContainer.ParentContainer;
				}

				if (oid != curDataHost.Oid)
				{
					throw new Exception($"invalid oid: {oid}");
				}
				curDataHost = curDataHost.Parent;
			});

			return hostKeys;
		}
	}
}