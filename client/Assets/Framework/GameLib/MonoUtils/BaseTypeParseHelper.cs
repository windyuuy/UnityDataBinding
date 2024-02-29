namespace Framework.GameLib.MonoUtils
{
	public static class BaseTypeParseHelper
	{
		public static bool ParseBool(string text)
		{
			if (!bool.TryParse(text, out var value))
			{
				value = false;
			}

			return value;
		}

		public static long ParseLong(string text)
		{
			if (!long.TryParse(text, out var value))
			{
				value = 0;
			}

			return value;
		}

		public static double ParseDouble(string text)
		{
			if (!double.TryParse(text, out var value))
			{
				value = 0;
			}

			return value;
		}

		public static int ParseInt(string text)
		{
			if (!int.TryParse(text, out var value))
			{
				value = 0;
			}

			return value;
		}

		public static float ParseFloat(string text)
		{
			if (!float.TryParse(text, out var value))
			{
				value = 0;
			}

			return value;
		}

	}
}