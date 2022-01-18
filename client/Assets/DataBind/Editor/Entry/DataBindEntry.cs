
using UnityEngine;
using DataBindService;
using UnityEditor.Callbacks;
using UnityEditor;

public class DataBindEntry
{
	private static bool hasSupport = false;
	[PostProcessBuild(1000)]
	private static void OnPostprocessBuildPlayer(BuildTarget buildTarget, string buildPath)
	{
		Debug.LogWarning("OnPostprocessBuildPlayer");
		hasSupport = false;
	}

	[PostProcessScene]
	public static void SupportU3DDataBindPost()
	{
		if (hasSupport == true)
		{
			return;
		}
		hasSupport = true;

		Debug.LogWarning("SupportU3DDataBindPost");
		SupportU3DDataBind();
	}
	[InitializeOnLoadMethod]
	public static void SupportU3DDataBind()
	{

		Debug.LogWarning("SupportU3DDataBind");
		BindEntry.SupportU3DDataBind();

	}

}
