using System.Collections.Generic;
using System.Linq;

namespace gcc.layer
{
	using Debug = UnityEngine.Debug;

	// TODO: 支持图层依赖
	/**
	 * 基于tag的图层顺序管理
	 */
	public class LayerOrderManager
	{
		/**
		 * tag的顺序map
		 */
		public readonly Dictionary<string, int> TagsOrderMap = new Dictionary<string, int>();

		public virtual int[] GetTagOrderRange()
		{
			var maxOrder = TagsOrderMap.Values.Max();
			var minOrder = TagsOrderMap.Values.Min();
			return new int[2]
			{
				minOrder,
				maxOrder,
			};
		}

		/**
		 * 设置tag初始依赖顺序
		 */
		public void SetupTagOrders(string[] tags)
		{
			for (int index = 0; index < tags.Length; index++)
			{
				var tag = tags[index];
				this.TagsOrderMap[tag] = index;
			}
		}

		private readonly List<(int order, string key)> _reOrderMap = new();

		/**
		 * 设置tag依赖关系
		 * - 所有tag必须是单向依赖关系
		 * @param tag
		 * @param dependTags
		 * @param force 强制展开回环依赖
		 */
		public void SetTagOrders(string tag, string[] dependTags, bool force = false)
		{
			if (TagsOrderMap.ContainsKey(tag))
			{
				foreach (var depTag in dependTags)
				{
					this.TagsOrderMap.TryAdd(depTag, 0);
				}
			}
			else
			{
				var tag0 = dependTags[0];
				TagsOrderMap.TryAdd(tag0, 0);
				var orderMax = TagsOrderMap[tag0];
				foreach (var depTag in dependTags)
				{
					if (!this.TagsOrderMap.TryGetValue(depTag, out var order))
					{
						order = 0;
						TagsOrderMap.Add(depTag, order);
					}

					if (order > orderMax)
					{
						orderMax = order;
					}
				}

				this.TagsOrderMap.Add(tag, orderMax + 1);
			}

			// 展开依赖
			{
				var curOrder = this.TagsOrderMap[tag];
				foreach (var depTag in dependTags)
				{
					var dOrder = this.TagsOrderMap[depTag];
					if (curOrder == dOrder)
					{
						curOrder = dOrder + 1;
					}
					else if (curOrder < dOrder)
					{
						if (force)
						{
							curOrder = dOrder + 1;
						}
						else
						{
							Debug.LogError("无法展开回环依赖");
						}
					}
				}

				this.TagsOrderMap[tag] = curOrder;
			}

			// 缩紧索引空间
			RetrenchOrderSpace();
		}

		private void RetrenchOrderSpace()
		{
			var tagsOrderMap = this.TagsOrderMap;
			if (tagsOrderMap.Count > 0)
			{
				var reOrderMap = this._reOrderMap;
				var needClear = reOrderMap.Count > 0;
				if (needClear)
				{
					reOrderMap.Clear();
				}

				foreach (var key in this.TagsOrderMap.Keys)
				{
					var order = this.TagsOrderMap[key];
					// reOrderMap[order] = order;
					reOrderMap.Add((order, key));
				}

				reOrderMap.Sort((a, b) => a.order - b.order);
				var order0 = reOrderMap[0].order;
				var curIndex = 0;
				foreach (var (order, key) in reOrderMap)
				{
					if (order != order0)
					{
						order0 = order;
						curIndex++;
					}

					tagsOrderMap[key] = curIndex;
				}

				if (needClear)
				{
					reOrderMap.Clear();
				}
			}
		}

		/**
		 * 设置tag依赖关系
		 * - 所有tag必须是单向依赖关系
		 * @param tag
		 * @param dependTags
		 * @param force 强制展开回环依赖
		 */
		public void SetTagOrder(string tag, string dependTag, bool force = false)
		{
			this.SetTagOrders(tag, new string[] { dependTag }, force);
		}

		/**
		 * 获取图层顺序
		 * @param tags
		 * @returns
		 */
		public int GetOrder(ICollection<string> tags)
		{
			if (tags.Count > 0)
			{
				var order = tags.Max(tag => { return this.TagsOrderMap.GetValueOrDefault(tag, 0); });
				return order;
			}
			else
			{
				return 0;
			}
		}

		public int GetOrder(string tag)
		{
			var order = this.TagsOrderMap.GetValueOrDefault(tag, 0);
			return order;
		}
	}
}