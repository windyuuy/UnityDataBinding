
using System.Collections.Generic;
using System.Linq;

namespace gcc.layer
{

	using Object = System.Object;
	using TagTarget = System.Object;

	/**
	 * 对象tag属性记录
	 */
	public class TagRecord
	{
		public TagTarget Target;
		public HashSet<string> TagsSet = new HashSet<string>();
	}

	/**
	 * tag管理类
	 */
	public class TagFilter
	{

		/**
		 * 记录项列表
		 */
		protected List<TagRecord> Records = new List<TagRecord>();

		protected TagRecord GetRecord(TagTarget target)
		{
			var record = this.Records.Find(r => r.Target == target);
			return record;
		}

		/**
		 * 更新对象的tag设置
		 * @param target 
		 * @param tags 
		 */
		public void UpdateTargetTags(TagTarget target, IEnumerable<string> tags)
		{
			var record = this.GetRecord(target);
			if (record != null)
			{
				record.TagsSet.Clear();
				foreach (var tag in tags)
				{
					record.TagsSet.Add(tag);
				}
			}
		}

		/**
		 * 移除记录的对象
		 * @param target 
		 */
		public void RemoveTarget(TagTarget target)
		{
			var record = this.GetRecord(target);
			this.Records.Remove(record);
		}

		/**
		 * 通过tag筛选所有对象
		 * @param tags 
		 */
		public TagTarget[] FilterTargetsByTags<T>(string[] tags)
		{
			var result = this.Records.FindAll(r =>
			{
				var matched = true;
				foreach (var tag in tags)
				{
					if (!r.TagsSet.Contains(tag))
					{
						matched = false;
					}
				}
				return matched;
			}).Select(r => r.Target).ToArray();
			return result;
		}
	}

}
