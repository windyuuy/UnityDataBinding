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

		[MenuItem("GameObject/UIEase/Button", false, 6, secondaryPriority = 22)]
		public static void CreateUIEase_Button(MenuCommand menuCommand){
			var pName = "UIEase/Button.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/GridScrollView", false, 6, secondaryPriority = 30)]
		public static void CreateUIEase_GridScrollView(MenuCommand menuCommand){
			var pName = "UIEase/GridScrollView.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/HorizontalListView", false, 6, secondaryPriority = 30)]
		public static void CreateUIEase_HorizontalListView(MenuCommand menuCommand){
			var pName = "UIEase/HorizontalListView.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/Image", false, 6, secondaryPriority = 19)]
		public static void CreateUIEase_Image(MenuCommand menuCommand){
			var pName = "UIEase/Image.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/InputField", false, 6, secondaryPriority = 24)]
		public static void CreateUIEase_InputField(MenuCommand menuCommand){
			var pName = "UIEase/InputField.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/Panel", false, 6, secondaryPriority = 23)]
		public static void CreateUIEase_Panel(MenuCommand menuCommand){
			var pName = "UIEase/Panel.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/ProgressBar", false, 6, secondaryPriority = 100)]
		public static void CreateUIEase_ProgressBar(MenuCommand menuCommand){
			var pName = "UIEase/ProgressBar.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/SuperImage", false, 6, secondaryPriority = 100)]
		public static void CreateUIEase_SuperImage(MenuCommand menuCommand){
			var pName = "UIEase/SuperImage.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/Text", false, 6, secondaryPriority = 20)]
		public static void CreateUIEase_Text(MenuCommand menuCommand){
			var pName = "UIEase/Text.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/TMPInputField", false, 6, secondaryPriority = 25)]
		public static void CreateUIEase_TMPInputField(MenuCommand menuCommand){
			var pName = "UIEase/TMPInputField.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/TMPText", false, 6, secondaryPriority = 21)]
		public static void CreateUIEase_TMPText(MenuCommand menuCommand){
			var pName = "UIEase/TMPText.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/VerticleListView", false, 6, secondaryPriority = 30)]
		public static void CreateUIEase_VerticleListView(MenuCommand menuCommand){
			var pName = "UIEase/VerticleListView.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/内嵌标签页", false, 6, secondaryPriority = 32)]
		public static void CreateUIEase_内嵌标签页(MenuCommand menuCommand){
			var pName = "UIEase/内嵌标签页.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/内嵌标签页.unpack", false, 6, secondaryPriority = 33)]
		public static void CreateUIEase_内嵌标签页_unpack(MenuCommand menuCommand){
			var pName = "UIEase/内嵌标签页.unpack.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
		[MenuItem("GameObject/UIEase/标签页", false, 6, secondaryPriority = 31)]
		public static void CreateUIEase_标签页(MenuCommand menuCommand){
			var pName = "UIEase/标签页.prefab";
			CreatePrefabInstanceAtMenu(menuCommand, pName);
		}
	}
}