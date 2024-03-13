using System;
using System.Text.RegularExpressions;
using Game.Diagnostics;

namespace TrackableResourceManager.Runtime
{
	public interface IResourceKey
	{
	}

	[Serializable]
	public readonly struct ResourceKey : IResourceKey
	{
		/// <summary>
		/// key or subUri
		/// </summary>
		public readonly string Key;

		internal ResourceKey(string key0)
		{
			this.Key = key0;
		}

		private static readonly Regex ParseExcelRegex = new Regex(@"@[a-zA-Z0-9_]+:[a-zA-Z0-9_]+:[a-zA-Z0-9_]+$");

		public static ResourceKey ParseFromLiteral(string key)
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			var m = ParseExcelRegex.Match(key);
			if (!m.Success)
			{
				string msg = $"invalid resource key format: {key}";
#if UNITY_EDITOR
				throw new ArgumentException(msg);
#else
				Debug.LogError(msg);
#endif
			}

#endif
			return new ResourceKey(key);
		}
	}
}