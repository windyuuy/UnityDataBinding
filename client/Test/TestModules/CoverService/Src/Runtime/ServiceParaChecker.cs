using CoverService.Recoverable;
using static UnityEngine.JsonUtility;

namespace CoverService.Utils
{
	public delegate void e();

	public static class ServiceParaChecker
	{
		private static T CheckPara<T>(T para) where T : struct
		{
			return FromJson<T>(ToJson(para));
		}

		public static T HandleReqPara<T>(string serviceName, T para) where T : struct
		{
			ServiceMsgRecordService.Inst.GetAccessor(null).Record(serviceName, para);
			return CheckPara(para);
		}

		public static T HandleRespPara<T>(string serviceName, T para) where T : struct
		{
			// TODO: support check resp result
			// ServiceMsgRecordService.Inst.GetAccessor(null).Record(serviceName, para);
			return CheckPara(para);
		}

	}
}