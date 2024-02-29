using UnityEditor;
using UnityEngine;

namespace EaseComps.PrefabUtils
{
	public static class PrefabCreateMenus
	{
		public static void CreatePrefabInstanceAtMenu(MenuCommand menuCommand, string pName)
		{
			var goPath = "Assets/Template/Prefabs/" + pName;
			var goPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(goPath);
			var menuCommandContext = menuCommand.context as GameObject;
			var go = Object.Instantiate(goPrefab, menuCommandContext == null ? null : menuCommandContext.transform);
			go.name = goPrefab.name;
			GameObjectUtility.SetParentAndAlign(go, menuCommandContext);
			Undo.RegisterCreatedObjectUndo(go, "Create" + goPrefab.name);
			Selection.activeObject = go;
		}

		[MenuItem("GameObject/UIEase/PagesToggle", false, 7)]
		public static void CreateUIEase_PagesToggle(MenuCommand menuCommand){
			var pName = "UIEase/PagesToggle.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
	}
}