﻿using UnityEditor;
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

			if (pName.EndsWith(".unpack.prefab"))
			{
				var parent = menuCommandContext == null ? null : menuCommandContext.transform;
				var childCount = goPrefab.transform.childCount;
				for (var i = 0; i < childCount; i++)
				{
					var prefabChild = goPrefab.transform.GetChild(i);
					var childGo = Object.Instantiate(prefabChild.gameObject, parent);
					childGo.name = prefabChild.name;

					GameObjectUtility.SetParentAndAlign(childGo, menuCommandContext);
					Undo.RegisterCreatedObjectUndo(childGo, "Create" + goPrefab.name);

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
				Undo.RegisterCreatedObjectUndo(go, "Create" + goPrefab.name);
				Selection.activeObject = go;
			}
		}

		[MenuItem("GameObject/UIEase/GridScrollView", false, 7, secondaryPriority = 20)]
		public static void CreateUIEase_GridScrollView(MenuCommand menuCommand){
			var pName = "UIEase/GridScrollView.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/HorizontalListView", false, 7, secondaryPriority = 20)]
		public static void CreateUIEase_HorizontalListView(MenuCommand menuCommand){
			var pName = "UIEase/HorizontalListView.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/PagesToggle", false, 7, secondaryPriority = 7)]
		public static void CreateUIEase_PagesToggle(MenuCommand menuCommand){
			var pName = "UIEase/PagesToggle.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/PagesToggle.unpack", false, 7, secondaryPriority = 7)]
		public static void CreateUIEase_PagesToggle_unpack(MenuCommand menuCommand){
			var pName = "UIEase/PagesToggle.unpack.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/ProgressBar", false, 7, secondaryPriority = 7)]
		public static void CreateUIEase_ProgressBar(MenuCommand menuCommand){
			var pName = "UIEase/ProgressBar.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/VerticleListView", false, 7, secondaryPriority = 20)]
		public static void CreateUIEase_VerticleListView(MenuCommand menuCommand){
			var pName = "UIEase/VerticleListView.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
	}
}