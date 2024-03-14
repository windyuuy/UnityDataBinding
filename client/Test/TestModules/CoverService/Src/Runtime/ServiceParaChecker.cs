using System;
using System.Linq;
using CoverService.Runtime.Recoverable;
using static UnityEngine.JsonUtility;

namespace CoverService.Runtime.Utils
{
	public static class ServiceParaChecker
	{
		private static T CheckPara<T>(T para)
		{
			var type = para.GetType();
			if (!type.GetCustomAttributes(typeof(SerializableAttribute), false).Any())
			{
				throw new ArgumentException("Req and Resp Structure should add SerializableAttribute");
			}
			return (T)FromJson(ToJson(para), type);
		}

		public static T HandleReqPara<T>(string serviceName, T para)
		{
			ServiceMsgRecordService.Inst.GetAccessor(null).Record(serviceName, para);
			return CheckPara(para);
		}

		public static T HandleRespPara<T>(string serviceName, T para)
		{
			// TODO: support check resp result
			// ServiceMsgRecordService.Inst.GetAccessor(null).Record(serviceName, para);
			return CheckPara(para);
		}

	}
}