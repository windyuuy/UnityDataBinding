
using System;
using Framework.GameLib.MonoUtils;

public class Date
{
	protected int hours;
	public Date()
	{
		var now = System.DateTime.Now;
		this.hours = now.Hour;
	}

	/// <summary>
	/// 获取小时单位时段(0~24)
	/// </summary>
	/// <returns></returns>
	public int getHours()
	{
		return this.hours;
	}

	/// <summary>
	/// 获取系统时间(小时)
	/// </summary>
	/// <returns></returns>
	public double GetTotalHours()
	{
		return new TimeSpan(System.DateTime.Now.Ticks).TotalHours;
	}

	/// <summary>
	/// 获取系统时间(ms)
	/// </summary>
	/// <returns></returns>
	public static long Now()
	{
		var toNow = DateTime.UtcNow.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
		return Convert.ToInt64(toNow.TotalMilliseconds);
	}

	public static long Now(DateTime nowDateTime)
	{
		var toNow = nowDateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
		return Convert.ToInt64(toNow.TotalMilliseconds);
	}

	public static string GetLocalShortTimestamp()
	{
		var date = DateTime.Now;
		return $"{date.Hour}:{date.Minute}:{date.Second}.{date.Millisecond.ToString("d3")}";
	}

	public static string GetLocalTimestamp()
	{
		var date = DateTime.Now;
		return $"{date.Month}.{date.Day}-{date.Hour}:{date.Minute}:{date.Second}.{date.Millisecond.ToString("d3")}";
	}

	/// <summary>
	/// 获取系统时间(ms)
	/// </summary>
	/// <returns></returns>
	public static double GetTotalMilliseconds()
	{
		return new TimeSpan(System.DateTime.Now.Ticks).TotalMilliseconds;
	}

	/// <summary>
	/// 获取系统时间(分钟)
	/// </summary>
	/// <returns></returns>
	public static double GetTotalMinutes()
	{
		return new TimeSpan(System.DateTime.Now.Ticks).TotalMinutes;
	}

	/// <summary>
	/// 获取系统时间(秒)
	/// </summary>
	/// <returns></returns>
	public static double GetTotalSeconds()
	{
		return new TimeSpan(System.DateTime.Now.Ticks).TotalSeconds;
	}

	public static string NowTimestampUIso8601()
	{
		return DateTime.UtcNow.ToIso8601();
	}
}
