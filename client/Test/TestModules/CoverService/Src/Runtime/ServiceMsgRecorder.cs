using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CoverService.Recoverable
{
	[Serializable]
	public class ServiceMsg
	{
		public ServiceMsg(long t, string d, string c, string s)
		{
			this.t = t;
			this.d = d;
			this.c = c;
			this.s = s;
		}

		/// <summary>
		/// emit time
		/// </summary>
		public long t;

		/// <summary>
		/// req data string
		/// </summary>
		public string d;

		/// <summary>
		/// service name
		/// </summary>
		public string s;

		/// <summary>
		/// caller name
		/// </summary>
		public string c;

		/// <summary>
		/// full type name
		/// </summary>
		public string f;
	}

	[Serializable]
	public class ServerMsgBatch
	{
		public List<ServiceMsg> m = new();

		/// <summary>
		/// 获取系统时间(ms)
		/// </summary>
		/// <returns></returns>
		public static long Now()
		{
			var toNow = DateTime.UtcNow.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return Convert.ToInt64(toNow.TotalMilliseconds);
		}

		public void Record(string name, object data)
		{
			string callerName;
			if (data is IWithCallerNameGetter caller)
			{
				callerName = caller.GetCallerName();
			}
			else
			{
				callerName = data.GetType().FullName;
			}

			var msg = new ServiceMsg(Now(), JsonUtility.ToJson(data), name, callerName);
			m.Add(msg);
		}

		public void Write(StreamWriter fs)
		{
			if (m.Count > 0)
			{
				var text = JsonUtility.ToJson(this);
				//{"m":[{}]}
				//fs.WriteLine(text[6..^2]);
				fs.WriteLine(text.Substring(6, text.Length - 2 - 6));
				this.m.Clear();
			}
		}
	}

	[Serializable]
	public class ServerMsgReadBatch
	{
		public static string WriteDir = $"{Application.persistentDataPath}/CoverRecords/";
		public ServiceMsg[] m;
	}

	public class ServiceMsgRecorder : IDisposable
	{
		protected StreamWriter SW;
		protected ServerMsgBatch MsgBatch = new();

		public ServiceMsgRecorder()
		{
			var writeDir = ServerMsgReadBatch.WriteDir;
			var writePath = $"{writeDir}{ServerMsgBatch.Now()}.records";

			if (!Directory.Exists(writeDir))
			{
				Directory.CreateDirectory(writeDir);
			}

			SW = new StreamWriter(writePath);

			Action quiting = null;
			quiting = () =>
			{
				Application.quitting -= quiting;
				Dispose();
			};
			Application.quitting += quiting;
		}

		protected int Count = 0;
		protected const int CountLimit = 200;

		public void Record(string name, object data)
		{
			MsgBatch.Record(name, data);

			if (++Count >= CountLimit)
			{
				this.SaveRecords();
			}
		}

		public void SaveRecords()
		{
			Count = 0;
			MsgBatch.Write(SW);
		}

		public void Dispose()
		{
			SaveRecords();

			try
			{
				if (SW != null)
				{
					SW.Close();
					SW.Dispose();
					SW = null;
				}
			}
			catch { }
		}
	}

	public class ServiceMsgRecover : IDisposable
	{
		protected StreamReader FileReader;
		protected int Index = 0;
		protected ServerMsgReadBatch Batch;

		public ServiceMsgRecover(string name)
		{
			var readDir = ServerMsgReadBatch.WriteDir;
			var readPath = $"{readDir}{name}.records";

			if (!Directory.Exists(readDir))
			{
				return;
			}

			FileReader = new(readPath);

			Action quiting = null;
			quiting = () =>
			{
				Application.quitting -= quiting;
				Dispose();
			};
			Application.quitting += quiting;
		}

		public bool IsEof()
		{
			return FileReader.EndOfStream && (Batch == null || Index >= Batch.m.Length);
		}

		public TR ReadOne<TR>()
		{
			if (Batch == null || Index >= Batch.m.Length)
			{
				var line = FileReader.ReadLine();
				Batch = JsonUtility.FromJson<ServerMsgReadBatch>($"{{\"m\":[{{{line}}}]}}");
				Index = 0;
			}

			var msg = Batch.m[Index++];
			var msgT = JsonUtility.FromJson<TR>(JsonUtility.ToJson(msg));
			return msgT;
		}

		public void Dispose()
		{
			if (FileReader != null)
			{
				FileReader.Close();
				FileReader.Dispose();
				FileReader = null;
			}
		}
	}

	public class ServiceMsgRecordService : IServiceTemplate
	{
		internal static readonly ServiceMsgRecordService Inst = new();
		protected ServiceAccessor Accessor { get; }

		public ServiceMsgRecordService()
		{
			Accessor = new(this);
		}

		public ServiceAccessor GetAccessor(object caller)
		{
			return Accessor;
		}

		protected readonly ServiceMsgRecorder Recorder = new();

		public class ServiceAccessor : IServiceAccessor
		{
			protected ServiceMsgRecordService Service;

			internal ServiceAccessor(ServiceMsgRecordService service)
			{
				Service = service;
			}

			public bool Record(string name, object obj)
			{
				Service.Recorder.Record(name, obj);
				return true;
			}

			public object Caller { get; set; }
		}

		public string Name { get; } = "ServiceMsgRecordService";
	}
}