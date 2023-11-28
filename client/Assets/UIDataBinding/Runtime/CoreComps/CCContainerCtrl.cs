using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Ext;
using Game.Diagnostics.IO;
using UnityEngine;
using Console = Game.Diagnostics.IO.Console;

namespace DataBinding.UIBind
{
	/**
	 * 确定CCContainerBind的具体绑定行为
	 */
	public class CCContainerCtrl : CCMyComponent, ICCContainerCtrl
	{

		/**
		 * 如果子节点没有添加 DialogChild, 那么强制为所有子节点添加 DialogChild
		 */
		[Rename("自动收容子节点")]
		public bool bindChildren = true;

		[Rename("使用容器选项覆盖")]
		public bool overrideWithContainerOptions = true;

		public virtual void Integrate()
		{
			var ccContainerBind = this.GetComponent<CCContainerBind>();
			if (ccContainerBind)
			{
				if (this.overrideWithContainerOptions)
				{
					this.bindChildren = ccContainerBind.bindChildren;
				}
				if (this.bindChildren)
				{
					this.ForEachChildren(child =>
					{
						var ccDataHost = child.GetOrAddComponent<CCDataHost>();
						ccDataHost.Integrate();
					});
				}
			}
		}

		protected EventHandlerMV2<object, object> watcher;
		protected List<object> oldList = new List<object>();
		public virtual void Relate()
		{
			var ccContainerBind = this.GetComponent<CCContainerBind>();
			if (ccContainerBind != null)
			{
				if (this.overrideWithContainerOptions)
				{
					this.bindChildren = ccContainerBind.bindChildren;
				}
				this.watcher = ccContainerBind.ContainerBind.WatchList((object newValue, object oldValue) =>
				{
					var dataSources = ((System.Collections.IList)newValue);
					var parent = this.transform;
					var children = parent.GetChildren();
					var childrenCount0 = children.Length;
					var maxCount = dataSources.Count;
					var lastI = 0;
					var childIndex = 0;
					if (childrenCount0 == 0)
					{
						// 不存在
						Console.Warn("没有子节点, 无法满足预期数量的数据项数.");
					}
					else
					{
						var tempNode = children[childrenCount0 - 1];
						for (var i = 0; i < maxCount; childIndex++)
						{
							var child = children.TryGet(childIndex);
							if (childIndex == maxCount)
							{
								if (i == lastI)
								{
									// 不足
									Console.Warn("可能无法满足预期数量的数据项数.");
								}
							}
							if (childIndex >= childrenCount0)
							{
								if (child == null)
								{
									// child = instantiate(tempNode)
									// child.parent = parent
									child = this.CreateItemNode(tempNode, parent);
								}
							}
							// 使用最近的节点作为模板
							tempNode = child;
							// 默认刷到的节点全部显示
							child.gameObject.SetActive(true);
							lastI = i;
							if (this.bindChildren)
							{
								var ccContainerItem = child.GetOrAddComponent<CCContainerItem>();
								ccContainerItem.Integrate();
							}
							// child为节点（不是数据源），需要遍历更新节点上表层数据源
							DataBindHubUtils.ForeachSurf<CCContainerItem>(child, (ccItem) =>
							{
								ccItem.ContainerItem.Index = i++;
								var itemHost = dataSources[(int)ccItem.ContainerItem.Index];
								var itemHost1 = VM.Utils.implementStdHost(itemHost);
								ccItem.BindDataHost(itemHost1,$"N|{ccItem.ContainerItem.Index}");
							});
						}
						// 没刷到的节点全隐藏
						for (var j = childIndex; j < childrenCount0; j++)
						{
							var child = children[j];
							if (child)
							{
								child.gameObject.SetActive(false);
							}
						}
					}
				});
			}
		}

		public virtual void Derelate()
		{
			var ccContainerBind = this.GetComponent<CCContainerBind>();
			if (ccContainerBind)
			{
				if (this.watcher != null)
				{
					ccContainerBind.ContainerBind.UnWatchList(this.watcher);
				}
			}
		}

		/**
		 * 创建子节点项
		 */
		protected virtual Transform CreateItemNode(Transform tempNode, Transform parent)
		{
			var child = Instantiate(tempNode.gameObject, parent);
			child.name = tempNode.name;
			//child.transform.SetParent(parent);
			return child.transform;
		}

		protected override void OnPreDestroy()
		{
			this.Derelate();
		}

	}
}
