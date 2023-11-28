
using System.Collections.Generic;
using System.Linq;

namespace lang.libs
{
	using boolean = System.Boolean;
	using number = System.Double;
	using Error = System.Exception;
	using JSON = lang.json.JSON;
	using Object = System.Object;
	using console = Game.Diagnostics.Debug;
	using StringBuilder = System.Text.StringBuilder;

	/**
     * 日志参数
     */
	public interface ILogParam
	{
		boolean time { get; set; }
		string[] tags { get; set; }
	}

	public class LogParam : ILogParam
	{
		protected bool _time;
		public bool time
		{
			get => _time;
			set => _time = value;
		}
		public string[] _tags;
		public string[] tags
		{
			get => _tags;
			set => _tags = value;
		}
	}

	public class Log
	{

		private static boolean _enablePlainLog = false;
		/**
         * 是否启用平铺日志
         * - 如果启用平铺日志, 将会直接序列化日志对象, 转换为字符串打印出来
         */
		public static boolean enablePlainLog
		{
			get
			{
				return Log._enablePlainLog;
			}
			set
			{
				Log._enablePlainLog = value;
			}
		}

		/**
		 * 将对象转换为平铺日志
		 * - 如果启用平铺日志, 将会直接序列化日志对象, 转换为字符串打印出来
		 */
		public static List<string> toPlainLog(object[] args)
		{
			var plainTexts = new List<string>();

			foreach (var info in args)
			{
				var ret = "";

				if (info is Error)
				{
					var err = info as Error;
					ret = $"Error content: { JSON.stringify(err)}\n{ err.StackTrace}";

				}
				else if (info is Object)
				{
					ret = JSON.stringify(info);

				}
				else
				{
					if (info is string)
					{
						ret = info as string;
					}
					else
					{
						ret = info.ToString();
					}

				}
				plainTexts.Add(ret);


			}

			return plainTexts;
		}

		protected static Log _instance;
		/**
         * 可选使用的单例
         */
		public static Log Inst
		{
			get
			{
				if (Log._instance == null)
				{
					Log._instance = new Log();
				}
				return Log._instance;
			}
		}

		/**
		 * 是否打印时间戳
		 */
		protected boolean time;
		/**
         * 日志标签
         */
		protected List<string> tags;
		/**
         * 日志选项内容是否需要更新
         */
		protected boolean dirty = true;


		public Log(ILogParam x = null)
		{
			if (x == null)
			{
				x = new LogParam();
			}
			this.setLogOptions(x);
		}

		/**
		 * 尾部追加标签
		 * @param tag 
		 * @returns 
		 */
		public Log appendTag(string tag)
		{
			if (this.tags != null)
			{
				this.tags.Add(tag);
			}
			else
			{
				// this.tags = new string[] { tag };
				this.tags = new List<string>(1);
				this.tags.Add(tag);
			}
			this.dirty = this.dirty || tag != null;

			return this;
		}

		/**
		 * 尾部追加标签列表
		 * @param tags 
		 * @returns 
		 */
		public Log appendTags(string[] tags)
		{
			foreach (var tag in tags)
			{
				this.appendTag(tag);
			}
			this.dirty = this.dirty || tags.Length > 0;
			return this;
		}

		/**
		 * 设置日志选项
		 * @param param0 
		 * @returns 
		 */
		public Log setLogOptions(ILogParam p = null)
		{
			var time = p.time;
			var tags = p.tags;

			this.time = time;

			if (tags != null)
			{
				this.tags = tags.Clone() as List<string>;

			}
			this.dirty = true;

			return this;

		}

		/**
		 * 缓存的日志标签戳
		 */
		protected string _cachedTagsStamp;
		/**
         * 获取日志标签戳
         * @returns 
         */
		protected string getTagsStamp()
		{
			if (!this.dirty)
			{
				return this._cachedTagsStamp;
			}

			string tag;
			if (this.tags != null)
			{
				tag = $"[{ string.Concat(this.tags, "][")}]";
			}
			else
			{
				tag = "";



			}

			if (this.time)
			{
				tag = tag + $"[t/{ System.DateTime.Now}]";
			}

			this._cachedTagsStamp = tag;

			this.dirty = false;


			return tag;

		}

		/**
		 * log通道打印日志，并储至日志文件
		 * @param args 
		 */
		public void log(params object[] args)
		{
			// if (this.tags) {
			//     args = this.tags.concat(args)
			// }
			// if (this.time) {
			//     args.push(new Date().getTime())
			// }

			console.Log(
				new StringBuilder(" -", 2 + args.Length).Append(this.getTagsStamp()).Append(args)
			);
		}

		/**
		 * 将消息打印到控制台，不存储至日志文件
		 */
		public void debug(params object[] args)
		{
			// if (this.tags) {
			//     args = this.tags.concat(args)
			// }
			// if (this.time) {
			//     args.push(new Date().getTime())
			// }
			console.Log(
				new StringBuilder(" -", 2 + args.Length).Append(this.getTagsStamp()).Append(args)
			);
		}

		/**
		 * 将消息打印到控制台，不存储至日志文件
		 */
		public void info(params object[] args)
		{
			// if (this.tags) {
			//     args = this.tags.concat(args)
			// }
			// if (this.time) {
			//     args.push(new Date().getTime())
			// }
			console.Log(
				new StringBuilder(" -", 2 + args.Length).Append(this.getTagsStamp()).Append(args)
			);
		}

		/**
		 * 将消息打印到控制台，并储至日志文件
		 */
		public void warn(params object[] args)
		{
			// if (this.tags) {
			//     args = this.tags.concat(args)
			// }
			// if (this.time) {
			//     args.push(new Date().getTime())
			// }
			console.LogWarning(
				new StringBuilder(" -", 2 + args.Length).Append(this.getTagsStamp()).Append(args)
			);
		}

		/**
		 * 将消息打印到控制台，并储至日志文件
		 */
		public void error(params object[] args)
		{
			// if (this.tags) {
			//     args = this.tags.concat(args)
			// }
			// if (this.time) {
			//     args.push(new Date().getTime())
			// }
			console.LogError(
				new StringBuilder(" -", 2 + args.Length).Append(this.getTagsStamp()).Append(args)
			);
			foreach (var p in args)
			{
				if (p is Error)
				{
					var e = p as Error;
					console.Log(e.StackTrace);
				}
			}
			console.Log(">>>error");

			console.Log(new Error().StackTrace);

		}

		/**
		 * 从目标覆盖日志选项到自身
		 * @param source 
		 */
		public Log mergeFrom(Log source)
		{
			this.time = source.time;



			if (source.tags != null)
			{
				if (this.tags != null)
				{
					for (var i = 0; i < source.tags.Count; i++)
					{
						this.tags[i] = source.tags[i];
					}
				}
				else
				{
					this.tags = source.tags.Clone();
				}
			}
			else
			{
				if (this.tags != null)
				{
					this.tags.Clear();
				}
			}
			this.dirty = source.dirty;
			this._cachedTagsStamp = source._cachedTagsStamp;
			return this;
		}

		/**
		 * 克隆自己
		 * @returns 
		 */
		public Log clone()
		{
			var log = new Log();
			log.mergeFrom(this);
			return log;
		}

	}

}