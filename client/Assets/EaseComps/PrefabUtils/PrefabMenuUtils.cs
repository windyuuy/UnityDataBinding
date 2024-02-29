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
		[MenuItem("EaseComps/UpdateMenus")]
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
					var fileName = Path.GetRelativePath(path,file);
					fileName = fileName.Replace('\\', '/');
					fileName = Path.ChangeExtension(fileName, "");
					fileName = fileName.Substring(0, fileName.Length - 1);
					var inlineCode1 = $"\t\t\tvar pName = \"{fileName}.prefab\";\n";
					return
						$"		[MenuItem(\"GameObject/{fileName}\", false, 7)]\n\t\tpublic static void Create{fileName.Replace('\\', '/').Replace('/', '_')}(MenuCommand menuCommand){{\n{inlineCode1}{inlineCode2}\t\t}}";
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
			var go = Object.Instantiate(goPrefab, menuCommandContext == null ? null : menuCommandContext.transform);
			go.name = goPrefab.name;
			GameObjectUtility.SetParentAndAlign(go, menuCommandContext);
			Undo.RegisterCreatedObjectUndo(go, ""Create"" + goPrefab.name);
			Selection.activeObject = go;
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