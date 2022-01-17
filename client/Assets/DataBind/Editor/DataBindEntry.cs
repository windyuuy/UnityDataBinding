using System.Collections;
using System.Collections.Generic;
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

		SupportU3DDataBind();
	}
	[InitializeOnLoadMethod]
	public static void SupportU3DDataBind()
	{

		BindEntry.SupportU3DDataBind();

	}

}
