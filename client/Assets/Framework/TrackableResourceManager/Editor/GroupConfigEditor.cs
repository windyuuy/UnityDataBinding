using System.Collections.Generic;
using System.Linq;
using ResourceManager.Trackable.Runtime;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace ResourceManager.Trackable.Editor
{
	[CustomEditor(typeof(GroupConfig))]
	public class GroupConfigEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var needBuild = GUILayout.Button("预览分组");
			if (needBuild)
			{
				var asset = (GroupConfig)this.target;
				asset.InjectToAAGroups(AddressableAssetSettingsDefaultObject.Settings);

				AddressableAssetsWindow.Init();
			}
		}

		[MenuItem("Assets/CopyUKey", priority = 5)]
		public static void CopyResourceItemUKey()
		{
			var guid = Selection.assetGUIDs[0];
			CopyResourceItemUKey(guid);
		}

		public static void CopyResourceItemUKey(string guid)
		{
			if (GroupConfig.GetEntryUKey(new(guid), out var uKey))
			{
				GUIUtility.systemCopyBuffer = uKey;
				Debug.Log($"复制uKey for {guid}: {uKey}");
			}
			else
			{
				Debug.LogError($"复制uKey失败 for {guid}");
			}
		}
	}
}