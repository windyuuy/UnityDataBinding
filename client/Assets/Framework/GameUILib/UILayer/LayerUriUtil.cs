using System.Text.RegularExpressions;

namespace gcc.layer
{
	public class LayerUriUtil
	{
		public static string WrapUri(string uri)
		{
			return $"Assets/Bundles/UI/Dialogs/{uri}.prefab";
		}

		private static readonly Regex UriRegex = new Regex(@"^Assets/Bundles/UI/Dialogs/{[a-zA-Z0-9_/]+}.prefab$");

		public static string ExtractUri(string resUri)
		{
			if (!string.IsNullOrEmpty(resUri))
			{
				var m = UriRegex.Match(resUri);
				if (m.Success)
				{
					return m.Groups[1].Value;
				}

				return resUri;
			}

			return null;
		}
	}
}