using System;
using System.Diagnostics;

namespace System.Runtime.CompilerServices
{
	public static class IsExternalInit
	{
	}
}

#if !(UNITY_EDITOR || UNITY_2017_1_OR_NEWER)
namespace UnityEngine
{
	public class JsonUtility
	{
		public static T FromJson<T>(string text)
		{
			return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(text);
		}

		public static string ToJson(object json)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(json);
		}
	}

	public static class Application
	{
		public static string persistentDataPath
		{
			get
			{
#if UNITY_EDITOR
				return UnityEngine.Application.persistentDataPath;
#else
				return "./";
#endif
			}
		}

		public static event Action quitting
		{
#if UNITY_EDITOR
			add => UnityEngine.Application.quitting += value;
			remove => UnityEngine.Application.quitting -= value;
#else
			add
			{
			}
			remove { }
#endif
		}
	}

	public static class Debug
	{
		public static void LogError(string log)
		{
			Console.Error.WriteLine(log);
		}
	}
}

#endif