

using System.Collections.Generic;
using System.Linq;

namespace gcc.layer
{

    using console = UnityEngine.Debug;
    using number = System.Double;
    using boolean = System.Boolean;

    // TODO: 支持图层依赖
    /**
	 * 基于tag的图层顺序管理
	 */
    public class LayerOrderMG
    {
        /**
		 * tag的顺序map
		 */
        protected Dictionary<string, int> tagsOrderMap = new Dictionary<string, int>();

        public virtual Dictionary<string, int> TagsOrderMap => tagsOrderMap;
        public virtual number[] GetTagOrderRange()
        {
            var maxOrder = tagsOrderMap.Values.Max();
            var minOrder = tagsOrderMap.Values.Min();
            return new number[2]{
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
                this.tagsOrderMap[tag] = index - 1;
            }
        }

        protected List<int> ReOrderMap = new List<int>();
        /**
		 * 设置tag依赖关系
		 * - 所有tag必须是单向依赖关系
		 * @param tag 
		 * @param dependTags 
		 * @param force 强制展开回环依赖
		 */
        public void SetTagOrders(string tag, string[] dependTags, boolean force = false)
        {
            if (this.tagsOrderMap.ContainsKey(tag) == false)
            {
                this.tagsOrderMap[tag] = 0;
            }
            foreach (var dtag in dependTags)
            {
                if (this.tagsOrderMap.ContainsKey(dtag) == false)
                {
                    this.tagsOrderMap[dtag] = 0;
                }
            }

            // 展开依赖
            {
                var curOrder = this.tagsOrderMap[tag];
                foreach (var dtag in dependTags)
                {
                    var dOrder = this.tagsOrderMap[dtag];
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
                            console.LogError("无法展开回环依赖");
                        }
                    }
                }
                this.tagsOrderMap[tag] = curOrder;
            }

            // 缩紧索引空间
            {
                var reOrderMap = this.ReOrderMap;
                reOrderMap.Clear();
                foreach (var key in this.tagsOrderMap.Keys)
                {
                    var order = this.tagsOrderMap[key];
                    reOrderMap[order] = order;
                }

                var maxOrder0 = 0;
                for (var i = reOrderMap.Count - 1; i >= 0; i--)
                {
                    var order = reOrderMap[i];
                    if (order > maxOrder0)
                    {
                        maxOrder0 = order;
                    }
                }

                var maxOrder = 0;
                var curOrder = 0;
                while (maxOrder <= maxOrder0)
                {
                    if (reOrderMap.Count > maxOrder)
                    {
                        var oldOrder = reOrderMap[maxOrder];
                        reOrderMap[maxOrder] = curOrder;
                        curOrder++;
                    }
                    maxOrder++;
                }

                foreach (var key in this.tagsOrderMap.Keys)
                {
                    var order = this.tagsOrderMap[key];
                    this.tagsOrderMap[key] = reOrderMap[order];
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
        public void SetTagOrder(string tag, string dependTag, boolean force = false)
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
                var order = tags.Max(tag =>
                {
                    return this.tagsOrderMap.GetValueOrDefault(tag, 0);
                });
                return order;
            }
            else
            {
                return 0;
            }
        }

        public int GetOrder(string tag)
        {
            var order= this.tagsOrderMap.GetValueOrDefault(tag, 0);
            return order;
        }

    }

}
