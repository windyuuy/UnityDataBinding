public static class RandomExt
{
	public static double Range(this System.Random random, double minValue, double maxValue)
	{
		var d = random.NextDouble();
		var n = (maxValue - minValue) * d + minValue;
		return n;
	}
	public static int Range(this System.Random random, int minValue, int maxValue)
	{
		var n = random.Next(minValue, maxValue);
		return n;
	}

	static System.Random sharedRandom = new System.Random();
	public static double Range(double minValue, double maxValue)
	{
		return Range(sharedRandom, minValue, maxValue);
	}
	public static double Range()
	{
		return sharedRandom.NextDouble();
	}
}