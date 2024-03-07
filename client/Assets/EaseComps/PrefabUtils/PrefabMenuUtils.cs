using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EaseComps.PrefabUtils
{
	public static class PrefabMenuUtils
	{
		[Serializable]
		public class MenuItemSettings
		{
			public MenuItemSettings()
			{
			}

			public MenuItemSettings(MenuFolderSettings settings)
			{
				order = settings.order;
			}

			public string name;
			public string guid;

			/// <summary>
			/// 菜单顺序
			/// </summary>
			public int order = 10;
		}

		[Serializable]
		public class MenuFolderSettings
		{
			public MenuFolderSettings()
			{
			}

			/// <summary>
			/// 菜单顺序
			/// </summary>
			public int order = 10;

			public MenuItemSettings[] items = Array.Empty<MenuItemSettings>();
		}

		[MenuItem("Tools/EaseComps/更新资源菜单")]
		public static void UpdateMenus()
		{
			var path = "Assets/Template/Prefabs";
			var codePath = $"{path}/CreateMenus.cs";
			if (Directory.Exists(path))
			{
				var files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);

				var inlineCode2 = @"			CreatePrefabInstanceAtMenu(menuCommand, pName);
";
				var menusCode = string.Join('\n', files.Select(file =>
				{
					file = file.Replace('\\', '/');
					var dir = Path.GetDirectoryName(file)!.Replace('\\', '/');
					var settingsPath = dir + "/MenuSettings.json";
					MenuFolderSettings settings;
					if (File.Exists(settingsPath))
					{
						settings = JsonUtility.FromJson<MenuFolderSettings>(File.ReadAllText(settingsPath,
							Encoding.UTF8));
					}
					else
					{
						settings = new();
					}

					var fileRelativePath = Path.GetRelativePath(path, file);
					var assetPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
					// var fileName = Path.GetFileNameWithoutExtension(fileRelativePath);
					var guid = AssetDatabase.AssetPathToGUID(assetPath);
					var itemSettings = settings.items.FirstOrDefault(item => item.guid == guid) ??
					                   new MenuItemSettings(settings);

					var menuOrder = settings.order;
					var subMenuOrder = itemSettings.order;
					fileRelativePath = fileRelativePath.Replace('\\', '/');
					fileRelativePath = Path.ChangeExtension(fileRelativePath, "");
					fileRelativePath = fileRelativePath.Substring(0, fileRelativePath.Length - 1);
					var inlineCode1 = $"\t\t\tvar pName = \"{fileRelativePath}.prefab\";\n";
					return
						$"		[MenuItem(\"GameObject/{fileRelativePath}\", false, {menuOrder}, secondaryPriority = {subMenuOrder})]\n\t\tpublic static void Create{fileRelativePath.Replace('\\', '/').Replace('/', '_').Replace(".", "_")}(MenuCommand menuCommand){{\n{inlineCode1}{inlineCode2}\t\t}}";
				}));
				var code =
					@"using UnityEditor;
using UnityEngine;

namespace EaseComps.PrefabUtils
{
	public static class PrefabCreateMenus
	{
		public static void CreatePrefabInstanceAtMenu(MenuCommand menuCommand, string pName)
		{
			var goPath = ""Assets/Template/Prefabs/"" + pName;
			var goPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(goPath);
			var menuCommandContext = menuCommand.context as GameObject;

			if (pName.EndsWith("".unpack.prefab""))
			{
				var parent = menuCommandContext == null ? null : menuCommandContext.transform;
				var childCount = goPrefab.transform.childCount;
				for (var i = 0; i < childCount; i++)
				{
					var prefabChild = goPrefab.transform.GetChild(i);
					var childGo = Object.Instantiate(prefabChild.gameObject, parent);
					childGo.name = prefabChild.name;

					GameObjectUtility.SetParentAndAlign(childGo, menuCommandContext);
					Undo.RegisterCreatedObjectUndo(childGo, ""Create"" + goPrefab.name);

					if (i == 0)
					{
						Selection.activeObject = childGo.gameObject;
					}
				}
			}
			else
			{
				var go = Object.Instantiate(goPrefab, menuCommandContext == null ? null : menuCommandContext.transform);
				go.name = goPrefab.name;
				GameObjectUtility.SetParentAndAlign(go, menuCommandContext);
				Undo.RegisterCreatedObjectUndo(go, ""Create"" + goPrefab.name);
				Selection.activeObject = go;
			}
		}

" + menusCode + @"
	}
}";
				File.WriteAllText(codePath, code, Encoding.UTF8);
				AssetDatabase.Refresh();
			}
		}
	}
}