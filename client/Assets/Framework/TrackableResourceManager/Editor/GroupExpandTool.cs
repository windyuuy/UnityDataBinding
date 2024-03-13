using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Content;
using UnityEditor.Build.Player;
using UnityEngine;

namespace Framework.TrackableResourceManager.Editor
{
	public static class GroupExpandTool
	{
		public class GroupDepInfo
		{
			public static bool EnableTest = false;
			public static string TagPrefix = "#";
			public static string IWUTag => $"{TagPrefix}iwu";
			public AddressableAssetGroup Group;
			public AddressableAssetGroup InternalGroup;
			public string Tag => Group.Default ? $"{TagPrefix}default" : $"{TagPrefix}{Group.Name}";
			public string GroupName => Group.Name;
			public string GroupNameIWU => $"{GroupName}IWU";
			public HashSet<ObjectIdentifier> SourceObjs = new();
			public HashSet<ObjectIdentifier> WeakRefObjs = new();
			public HashSet<ObjectIdentifier> IndirectDeps = new();
			public HashSet<ObjectIdentifier> IndexableDeps = new();

			public GroupDepInfo(AddressableAssetGroup group)
			{
				Group = group;
			}
		}

		[MenuItem("Tools/Resources/ExpandGroups")]
		public static void TestExpandGroups()
		{
			var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
			try
			{
				ExpandGroups(aaSettings, true);
				EditorUtility.ClearProgressBar();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void ExpandGroups(AddressableAssetSettings aaSettings, bool enableTest)
		{
			GroupDepInfo.EnableTest = enableTest;
			try
			{
				ExpandGroups(aaSettings);
			}
			finally
			{
				GroupDepInfo.EnableTest = false;
			}
		}

		public static void ExpandGroups(AddressableAssetSettings aaSettings)
		{
			ClearGroupDeps(aaSettings);

			var groups = aaSettings.groups.Where(group => group != null).ToArray();

			var context = new AddressablesDataBuilderInput(aaSettings);
			ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
			scriptCompilationSettings.group = context.TargetGroup;
			scriptCompilationSettings.target = context.Target;
			var tempDir = "./Library/com.unity.addressables/AnalyzeData/CompiledPlayerScriptsData/";
			Directory.CreateDirectory(tempDir);
			ScriptCompilationResult scriptCompilationResult =
				PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, tempDir);

			Dictionary<ObjectIdentifier, int> depsMap = new();
			Dictionary<ObjectIdentifier, List<ObjectIdentifier>> referMap = new();

			List<GroupDepInfo> groupDepInfos = new();
			foreach (var group in groups)
			{
				var groupInfo = new GroupDepInfo(group);
				groupDepInfos.Add(groupInfo);

				foreach (var entry in group.entries)
				{
					var vGuid = new GUID(entry.guid);
					var includedObjects =
						ContentBuildInterface.GetPlayerObjectIdentifiersInAsset(vGuid, context.Target);
					var mainObjs = includedObjects.Select(obj =>
					{
						var representations =
							ContentBuildInterface.GetPlayerAssetRepresentations(obj.guid, context.Target);
						return representations.First();
					});
					groupInfo.SourceObjs.UnionWith(mainObjs);
				}

				{
					var excludeObjs = new HashSet<ObjectIdentifier>(groupInfo.SourceObjs);
					// excludeObjs.UnionWith(groupInfo.DirectDeps);

					ObjectIdentifier[] deps = groupInfo.SourceObjs.ToArray();
					var diffDeps = new HashSet<ObjectIdentifier>(deps);

					while (deps.Length > 0)
					{
						var subDeps = ContentBuildInterface.GetPlayerDependenciesForObjects(
								deps,
								context.Target,
								scriptCompilationResult.typeDB)
							.Select(obj =>
								ContentBuildInterface.GetPlayerAssetRepresentations(obj.guid, context.Target).First())
							.Where(obj => !excludeObjs.Contains(obj));

						diffDeps.Clear();
						diffDeps.UnionWith(subDeps);
						deps = diffDeps.ToArray();
						
						// 通过AssetReference方式引用的依赖此处不考虑做额外处理

						excludeObjs.UnionWith(deps);
						groupInfo.IndirectDeps.UnionWith(deps);
					}
				}
			}

			foreach (var groupDepInfo in groupDepInfos)
			{
				foreach (var objectIdentifier in groupDepInfo.IndirectDeps)
				{
					if (depsMap.TryGetValue(objectIdentifier, out var count))
					{
						depsMap[objectIdentifier] = count + 1;
					}
					else
					{
						depsMap[objectIdentifier] = 1;
					}
				}
			}

			Dictionary<ObjectIdentifier, int> objMap;
			{
				var depsMapCopy = depsMap;
				depsMap = new Dictionary<ObjectIdentifier, int>();
				foreach (var keyValuePair in depsMapCopy)
				{
					if (keyValuePair.Value > 1)
					{
						depsMap.Add(keyValuePair.Key, keyValuePair.Value);
					}
				}

				objMap = depsMapCopy;
			}

			foreach (var groupDepInfo in groupDepInfos)
			{
				foreach (var objectIdentifier in groupDepInfo.SourceObjs)
				{
					depsMap.Remove(objectIdentifier);
				}
			}

			if(false)
			{
				foreach (var groupDepInfo in groupDepInfos)
				{
					foreach (var objectIdentifier in groupDepInfo.SourceObjs)
					{
						objMap.Add(objectIdentifier, 1);
					}
				}

				foreach (var pair in objMap)
				{
					var subDeps = ContentBuildInterface.GetPlayerDependenciesForObject(
							pair.Key,
							context.Target,
							scriptCompilationResult.typeDB)
						.Select(obj =>
							ContentBuildInterface.GetPlayerAssetRepresentations(obj.guid, context.Target).First())
						.Where(obj => objMap.ContainsKey(obj));

					foreach (var objectIdentifier in subDeps)
					{
						if (!referMap.TryGetValue(objectIdentifier, out var refer))
						{
							refer = new();
							referMap.Add(objectIdentifier, refer);
						}

						refer.Add(objectIdentifier);
					}
				}
				

				var referGroupMap = new Dictionary<ObjectIdentifier, GroupDepInfo>();
				// TODO: 完善分组依赖分析

				// 资源无重规则：递归向上处理引用路径，只有最近端点都在同一个分组的可以忽略加组，不在同一个组则加组
				// 去重方法：递归向上处理引用路径，当所有父节点标签全部相同，则排除该节点
				// 暂时不考虑优化引用深度的问题
				var handledMap = new Dictionary<ObjectIdentifier, HashSet<int>>();

				// HashSet<int> HandleRefer(ObjectIdentifier obj)
				// {
				// }
			}

			foreach (var groupDepInfo in groupDepInfos)
			{
				if (groupDepInfo.IndirectDeps.Count > 0)
				{
					groupDepInfo.IndexableDeps.UnionWith(groupDepInfo.IndirectDeps
						.Where(dep => depsMap.ContainsKey(dep))
						.Where(dep =>
							AddressableAssetUtility.IsPathValidForEntry(AssetDatabase.GUIDToAssetPath(dep.guid)))
					);
				}
			}

			var updatedMap = new Dictionary<ObjectIdentifier, AddressableAssetEntry>();
			foreach (var groupDepInfo in groupDepInfos)
			{
				foreach (var objectIdentifier in groupDepInfo.SourceObjs)
				{
					var entry = aaSettings.CreateOrMoveEntry(objectIdentifier.guid.ToString(), groupDepInfo.Group,
						false);
					entry.SetLabel(groupDepInfo.Tag, true, true);
				}

				foreach (var objectIdentifier in groupDepInfo.IndexableDeps)
				{
					if (!updatedMap.TryGetValue(objectIdentifier, out var entry))
					{
						entry = aaSettings.CreateOrMoveEntry(objectIdentifier.guid.ToString(), groupDepInfo.Group,
							true);
						updatedMap.Add(objectIdentifier, entry);
					}
					else if (!entry.ReadOnly)
					{
						entry.ReadOnly = true;
					}

					entry.SetLabel(GroupDepInfo.IWUTag, true, true);
					entry.SetLabel(groupDepInfo.Tag, true, true);
				}
			}
		}

		[MenuItem("Tools/Resources/ClearGroupDeps")]
		public static void ClearGroupDeps()
		{
			var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
			ClearGroupDeps(aaSettings);
		}

		private static void ClearGroupDeps(AddressableAssetSettings aaSettings)
		{
			var groups = aaSettings.groups.Where(group => group != null).ToArray();

			// clear tag iwu from all entries
			foreach (var group in groups)
			{
				foreach (var entry in group.entries.ToArray())
				{
					if (entry.labels.Count > 0)
					{
						if (entry.labels.Contains(GroupDepInfo.IWUTag))
						{
							aaSettings.RemoveAssetEntry(entry.guid);
						}
						else
						{
							var removes = entry.labels.Where(label => label.StartsWith(GroupDepInfo.TagPrefix))
								.ToArray();
							foreach (var tag in removes)
							{
								entry.SetLabel(tag, false, false);
							}
						}
					}
				}
			}
		}
	}
}