using System.Collections.Generic;
using System.Linq;
using System.Linq.MyExt;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace TrackableResourceManager.Runtime
{
	[CreateAssetMenu(fileName = "资源分组.asset", menuName = "UISys/资源分组")]
	public class GroupConfig : ScriptableObject
	{
		[Header("分组名")] public string groupName;

		// split with ;
		[Header("附加标签")] [Tooltip("split with ;")] [SerializeField]
		protected string tags;

		public string[] Tags => tags.Split(";");

		/// <summary>
		/// 是否打进包内
		/// </summary>
		[Header("是否也打进包内")] public bool isOffline;

		/// <summary>
		/// 帮助缩减搜索范围，加快搜索速度
		/// </summary>
		[Header("资源清单搜索路径")] [Tooltip("帮助缩减搜索范围，加快搜索速度")]
		public string scanPath = "Assets";

		public bool CheckValid()
		{
			if (string.IsNullOrEmpty(this.groupName))
			{
				Debug.LogError("请为分组填写名称");
				return false;
			}

			return true;
		}

		public readonly struct UKey
		{
			public readonly string Guid;
			public readonly int Order;

			public UKey(string guid, int order)
			{
				Guid = guid;
				Order = order;
			}

			public override int GetHashCode()
			{
				return Guid.GetHashCode() ^ Order;
			}

			public override bool Equals(object obj)
			{
				if (obj is UKey uKey)
				{
					return this.Guid == uKey.Guid && this.Order == uKey.Order;
				}

				return false;
			}
		}

		/// <summary>
		/// key-manifest asset guid
		/// </summary>
		protected Dictionary<UKey, string> KeyNamespace = new();

		protected bool IsLoaded = false;

		public IEnumerable<ResourceManifestConfig.ResourceItem> LoadAllManifestItems()
		{
			var assets = FindResourceManifestConfigs();
			var items = assets.MergeGroup(resourceManifestConfig => resourceManifestConfig.CollectResourceItems());
			return items;
		}

		public bool LoadAllManifestNamespace()
		{
			if (IsLoaded)
			{
				return false;
			}

			IsLoaded = true;

			var assets = FindResourceManifestConfigs();
			foreach (var asset in assets)
			{
				foreach (var resourceItemSet in asset.itemSets)
				{
					foreach (var resourceItem in resourceItemSet.items)
					{
						if (!KeyNamespace.TryAdd(new UKey(resourceItem.ResUri, asset.SortOrder), resourceItem.key))
						{
							Debug.LogError($"conflict key: {resourceItem}");
						}
					}
				}
			}

			return true;
		}

		private IEnumerable<ResourceManifestConfig> FindResourceManifestConfigs()
		{
			var assets = AssetDatabase
				.FindAssets("t:ResourceManifestConfig", new[] { scanPath })
				.Select(AssetDatabase.GUIDToAssetPath)
				.Select(AssetDatabase.LoadAssetAtPath<ResourceManifestConfig>)
				.Where(asset => asset.group == this);
			return assets;
		}

		public bool ResolveNamespace(string guid, string manifestGuid, int order, out string conflictKey)
		{
			var uKey = new UKey(guid, order);
			if (KeyNamespace.TryGetValue(uKey, out var assetGuid0))
			{
				if (manifestGuid != assetGuid0)
				{
					// namespace conflict
					var asset = AssetDatabase.LoadAssetAtPath<ResourceManifestConfig>(
						AssetDatabase.GUIDToAssetPath(assetGuid0));
					var item = asset.GetItemByGuid(guid);
					Debug.Assert(item != null, "item!=null");
					conflictKey = item.key;
					return false;
				}

				conflictKey = null;
				return true;
			}

			conflictKey = null;
			return true;
		}

		public bool AddKey(string key, string guid, int order, string manifestGuid)
		{
			var uKey = new UKey(guid, order);
			if (KeyNamespace.TryGetValue(uKey, out var assetGuid0))
			{
				if (manifestGuid != assetGuid0)
				{
					// namespace conflict
					return false;
				}

				KeyNamespace[uKey] = manifestGuid;
			}
			else
			{
				KeyNamespace.Add(uKey, manifestGuid);
			}

			return true;
		}

		public bool RemoveKey(string guid, int order, string manifestGuid)
		{
			var uKey = new UKey(guid, order);
			if (KeyNamespace.TryGetValue(uKey, out var assetGuid0))
			{
				if (manifestGuid != assetGuid0)
				{
					// namespace conflict
					return false;
				}

				KeyNamespace.Remove(uKey);
			}

			return true;
		}

		public bool FindBelong(
			ResourceManifestConfig.ResourceItem item,
			out ResourceManifestConfig belongManifestConfig)
		{
			var configs = FindResourceManifestConfigs();
			foreach (var resourceManifest in configs)
			{
				if (resourceManifest.GetBelongedSet(item, out var itemSet, out var itemOut))
				{
					belongManifestConfig = resourceManifest;
					return true;
				}
			}

			belongManifestConfig = null;
			return false;
		}

		public static bool GetEntryUKey(ResourceManifestConfig.ResourceItem item, out string uKey)
		{
			var configs = FindGroupConfigs();

			foreach (var groupConfig in configs)
			{
				if (groupConfig.FindBelong(item, out var belongManifest))
				{
					return belongManifest.GetEntryUKey(item, out uKey);
				}
			}

			uKey = "";
			return false;
		}

		private static IEnumerable<GroupConfig> FindGroupConfigs()
		{
			var configs = AssetDatabase.FindAssets("t:GroupConfig")
				.Select(AssetDatabase.GUIDToAssetPath)
				.Select(AssetDatabase.LoadAssetAtPath<GroupConfig>);
			return configs;
		}

		public void InjectToAAGroups(AddressableAssetSettings aaSettings)
		{
			var asset = this;
			var items = asset.LoadAllManifestItems();
			if (string.IsNullOrWhiteSpace(asset.groupName) == false && items.Any())
			{
				var group = aaSettings.groups.FirstOrDefault(group => group.Name == asset.groupName);
				if (group == null)
				{
					group = aaSettings.CreateGroup(asset.groupName, false, false, true,
						new List<AddressableAssetGroupSchema>());
				}

				foreach (var resourceItem in items)
				{
					var entry = aaSettings.CreateOrMoveEntry(resourceItem.ResUri, group, true);
					entry.SetAddress(resourceItem.key);

					entry.labels.Clear();
					foreach (var assetTag in asset.Tags)
					{
						entry.SetLabel(assetTag, true, true);
					}
				}
			}
		}

		public static void InjectAllToAAGroups(AddressableAssetSettings aaSettings)
		{
			var assets = FindGroupConfigs();
			foreach (var resourceManifestConfig in assets)
			{
				resourceManifestConfig.InjectToAAGroups(aaSettings);
			}
		}

		[MenuItem("Tools/Resources/InjectAllToAAGroups")]
		public static void InjectAllToAAGroups()
		{
			InjectAllToAAGroups(AddressableAssetSettingsDefaultObject.Settings);
		}
	}
}