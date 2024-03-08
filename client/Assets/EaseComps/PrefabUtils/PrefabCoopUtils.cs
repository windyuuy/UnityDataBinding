using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EaseComps.PrefabUtils
{
	public static class PrefabCoopUtils
	{
		[MenuItem("Tools/EaseComps/PrefabVariants")]
		public static void CreatePrefabVariants()
		{
			_ = Selection.assetGUIDs.Select(guid => { return CreatePrefabVariant(guid); }).ToArray();
		}

		private static GameObject CreatePrefabVariant(string guid)
		{
			var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
			var savePath =
				Path.ChangeExtension(
					$"{Path.GetDirectoryName(prefabPath)}/{Path.GetFileNameWithoutExtension(prefabPath)}Variant",
					".prefab");
			var i = 0;
			while (File.Exists(savePath))
			{
				savePath =
					Path.ChangeExtension(
						$"{Path.GetDirectoryName(prefabPath)}/{Path.GetFileNameWithoutExtension(prefabPath)}Variant{++i}",
						".prefab");
			}

			Object originalPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
			GameObject objSource = PrefabUtility.InstantiatePrefab(originalPrefab) as GameObject;
			GameObject prefabVariant = PrefabUtility.SaveAsPrefabAsset(objSource, savePath);
			var stringBuilder = Enumerable.Empty<string>();
			var isSkiping = false;
			foreach (var readLine in File.ReadLines(savePath, Encoding.UTF8))
			{
				if (readLine.Contains("m_Modifications:"))
				{
					stringBuilder =
						stringBuilder.Append(readLine.Replace("m_Modifications:", "m_Modifications: []"));
					isSkiping = true;
				}
				else if (isSkiping)
				{
					if (readLine.Contains("m_RemovedComponents:"))
					{
						isSkiping = false;
					}
				}
				else
				{
					stringBuilder = stringBuilder.Append(readLine);
				}
			}

			File.WriteAllLines(savePath, stringBuilder, Encoding.UTF8);

			return prefabVariant;
		}
	}
}
