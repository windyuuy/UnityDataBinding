using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace CoverService.Editor
{
	public static class ProjectsSyncTool
	{
		[MenuItem("Tools/CoverService/SyncProjects")]
		public static void SyncProjects()
		{
			var testProjectDir = @".\Test\TestModules";
			var dirs = Directory.GetDirectories(testProjectDir);
			var projectDirs = dirs.Where(dir =>
				Directory.Exists($"{dir}/Src")
				&& Directory.Exists($"{dir}.Test")
			).ToArray();
			if (projectDirs.Length > 0)
			{
				var addDict = new Dictionary<string, string>();
				foreach (var projectDir in projectDirs)
				{
					var projectName = Path.GetFileName(projectDir);
					var packageName = projectName.ToLower();
					var srcDir = $"{projectDir}/Src";
					var relativeSrcDir = Path.GetRelativePath(Application.dataPath, srcDir);
					var localVersion = $"file:{relativeSrcDir}".Replace("\\", "/");
					var packagePath = $"{srcDir}/package.json";
					var projectFile = $"{projectDir}/{projectName}.csproj";
					CreatePackage(packagePath, projectName, packageName, projectFile);
					addDict.Add(packageName, localVersion);
				}

				addDict.Add("com.unity.nuget.newtonsoft-json", "3.2.1");

				AddToManifest(addDict);
			}
		}

		private static Dictionary<string, string> ReadDeps(string projectFile)
		{
			var ret = new Dictionary<string, string>();
			XmlDocument xml = new XmlDocument();
			xml.Load(projectFile);
			var nsmgr = new XmlNamespaceManager(xml.NameTable);
			nsmgr.AddNamespace("proj", "http://schemas.microsoft.com/developer/msbuild/2003");
			var refers = xml.SelectNodes("//proj:ProjectReference", nsmgr);
			for (var i = 0; i < refers.Count; i++)
			{
				var refer = refers[i];
				string referName = null;
				for (var i1 = 0; i1 < refer.ChildNodes.Count; i1++)
				{
					var child = refer.ChildNodes[i1];
					if (child.Name == "Name")
					{
						referName = child.InnerText;
						break;
					}
				}

				if (referName != null)
				{
					ret.Add(referName.ToLower(), "1.0.0");
				}
			}

			return ret;
		}

		[Serializable]
		public class ManifestLite
		{
			public Dictionary<string, string> dependencies;
		}

		private static void AddToManifest(Dictionary<string, string> addDict)
		{
			var manifestPath = $"Packages/manifest.json";
			var json = Newtonsoft.Json.JsonConvert.DeserializeObject<ManifestLite>(File.ReadAllText(manifestPath,
				Encoding.UTF8));
			var needItems = addDict.Where(pair => !json.dependencies.ContainsKey(pair.Key)).ToArray();
			if (needItems.Length > 0)
			{
				var lines = File.ReadLines(manifestPath).ToList();
				for (var i = 0; i < lines.Count; i++)
				{
					var line = lines[i];
					if (line.TrimStart().StartsWith("\"com.unity.ugui\":"))
					{
						foreach (var item in needItems)
						{
							lines.Insert(i, $"    \"{item.Key}\": \"{item.Value}\",");
						}

						break;
					}
				}

				var content = string.Join("\n", lines);
				File.WriteAllText(manifestPath, content, Encoding.UTF8);
			}
		}

		private static void CreatePackage(string packagePath, string projectName, string packageName,
			string projectFile)
		{
			if (!File.Exists(packagePath))
			{
				var deps = ReadDeps(projectFile);

				var depLines = string.Join(",\n", deps.Select(dep =>
				{
					var line = $"    \"{dep.Key}\": \"{dep.Value}\"";
					return line;
				}));
				var content = @"{
  ""name"": """ + packageName + @""",
  ""displayName"": """ + projectName + @""",
  ""version"": ""1.0.0"",
  ""unity"": ""2019.4"",
  ""description"": """",
  ""dependencies"": {
" + depLines + @"
  }
}
";
				File.WriteAllText(packagePath, content, Encoding.UTF8);
			}
		}
	}
}
