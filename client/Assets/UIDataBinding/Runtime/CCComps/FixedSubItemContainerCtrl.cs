using EngineAdapter.LinqExt;
using UnityEngine;

namespace DataBind.UIBind
{
	public class FixedSubItemContainerCtrl : MonoBehaviour
	{
		/**
		 * 如果子节点没有添加 DialogChild, 那么强制为所有子节点添加 DialogChild
		 */
		[Rename("自动收容子节点")] public bool bindChildren = true;
		
		protected virtual void InitContainer()
		{
			if (this.bindChildren)
			{
				this.ForEachChildren(child =>
				{
					var ccDataHost = child.GetOrAddComponent<DataHostComp>();
					ccDataHost.Integrate();
				});
			}
		}
		
		protected virtual void OnDataChanged(System.Collections.IList dataSources)
		{
			var parent = this.transform;
			var children = parent.GetChildren();
			var childrenCount0 = children.Length;
			var maxCount = dataSources.Count;
			var lastI = 0;
			var childIndex = 0;
			if (childrenCount0 == 0)
			{
				// 不存在
				Debug.Log("没有子节点, 无法满足预期数量的数据项数.");
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
							Debug.LogWarning("可能无法满足预期数量的数据项数.");
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
						var ccContainerItem = child.GetOrAddComponent<ContainerItemComp>();
						ccContainerItem.Integrate();
					}

					// child为节点（不是数据源），需要遍历更新节点上表层数据源
					DataBindHubUtils.ForeachSurf<ContainerItemComp>(child, (ccItem) =>
					{
						ccItem.ContainerItem.Index = i++;
						var itemHost = dataSources[(int)ccItem.ContainerItem.Index];
						var itemHost1 = VM.Utils.ImplementStdHost(itemHost);
						ccItem.BindDataHost(itemHost1, $"N|{ccItem.ContainerItem.Index}");
					});
				}

				// 没刷到的节点全隐藏
				for (var j = childIndex; j < childrenCount0; j++)
				{
					var child = children[j];
					if (child)
					{
						this.OnRecycleNode(child);
					}
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
			//child.transform.SetParent(parent, false);
			return child.transform;
		}

		protected virtual void OnRecycleNode(Transform child)
		{
			child.gameObject.SetActive(false);
			// GameObject.DestroyImmediate(child.gameObject);
		}
	}
}