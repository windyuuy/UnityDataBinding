
using DataBindService;
using UnityEditor.Callbacks;
using UnityEditor;

using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Diagnostics;

namespace DataBinding.Editor.DataBindEntry
{

	public class DataBindEntry
	{
		public static bool HasSupport = false;
		[PostProcessBuild(1000)]
		private static void OnPostprocessBuildPlayer(BuildTarget buildTarget, string buildPath)
		{
			HasSupport = false;
		}

		[PostProcessScene]
		public static void SupportU3DDataBindPost()
		{
			if (HasSupport == true)
			{
				return;
			}
			HasSupport = true;

			SupportU3DDataBind();
		}
		[InitializeOnLoadMethod]
		public static void SupportU3DDataBind()
		{

			BindEntry.SupportU3DDataBind();
		}

	}

	public class MyCustomBuildProcessor0
	{
		public virtual void HandleDLLs(BuildReport report)
		{
		}

	}

	public class MyCustomBuildProcessor : MyCustomBuildProcessor0, IPostBuildPlayerScriptDLLs
	{
		public int callbackOrder { get { return 0; } }

		[Conditional("UNITY_2019_1_OR_NEWER")]
		public new void HandleDLLs(BuildReport report)
		{
			var filePath = report.files.Single(file => file.path.EndsWith("Assembly-CSharp.dll")).path;
			var binaryPath = Path.GetDirectoryName(filePath);
			BindEntry.SupportDataBind(filePath, new BindOptions());
		}

		public void OnPostBuildPlayerScriptDLLs(BuildReport report)
		{
			HandleDLLs(report);
		}

	}

}
