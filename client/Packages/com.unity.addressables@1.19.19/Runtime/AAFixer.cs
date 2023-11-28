using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.AddressableAssets;
using System.IO;
using System.Linq;

public class AAFixer
{

	public static void DeleteBackups()
	{
		DeleteBackupPath("hash");
		DeleteBackupPath("json");
	}
	protected static void DeleteBackupPath(string key0)
	{
		try
		{
			var saveDir = Application.persistentDataPath;
			PlayerPrefs.DeleteKey("AddressablesCatalog.LoadPath." + key0);
			var backupFilePathSaver = Path.Combine(saveDir, "AddressablesCatalog.LoadPath." + key0);
			if (File.Exists(backupFilePathSaver))
			{
				File.Delete(backupFilePathSaver);
			}
			var backupFile = Path.Combine(saveDir, "AddressablesCatalog.Save." + key0);
			if (File.Exists(backupFile))
			{
				File.Delete(backupFile);
			}
			var emptyMark = Path.Combine(saveDir, "AddressablesCatalog.EmptyMark." + key0);
			if (File.Exists(emptyMark))
			{
				File.Delete(emptyMark);
			}
		}
		catch (Exception e)
		{
			Debug.LogError("DeleteBackupPath failed:");
			Debug.LogException(e);
		}
	}
	protected static string GetSaveTargetPath(string key0)
	{
		var saveDir = Application.persistentDataPath;
		string saveTargetPath = null;
		//saveTargetPath=PlayerPrefs.GetString("AddressablesCatalog.LoadPath." + key0);
		if (string.IsNullOrEmpty(saveTargetPath))
		{
			var backupFilePathSaver = Path.Combine(saveDir, "AddressablesCatalog.LoadPath." + key0);
			try
			{
				if (File.Exists(backupFilePathSaver))
				{
					saveTargetPath = File.ReadAllText(backupFilePathSaver);
					return saveTargetPath;
				}
			}
			catch (Exception e)
			{
				Debug.LogError("touch backupFilePathSaver failed:");
				Debug.LogException(e);
			}
		}
		return null;
	}
	protected static void WriteRecoverAction(string key0, string action)
	{
		try
		{
			var saveDir = Application.persistentDataPath;
			var actionMark = Path.Combine(saveDir, "AddressablesCatalog.ActionMark." + key0);
			File.WriteAllText(actionMark, action);
		}
		catch (Exception e)
		{
			Debug.LogError("write action mark failed");
			Debug.LogException(e);
		}
	}
	protected static void RecoverFile(string key0)
	{
		try
		{
			//PlayerPrefs.DeleteAll();
			var saveDir = Application.persistentDataPath;
			var emptyMark = Path.Combine(saveDir, "AddressablesCatalog.EmptyMark." + key0);
			if (File.Exists(emptyMark))
			{
				var emptyTargetPath = File.ReadAllText(emptyMark);
				if (File.Exists(emptyTargetPath))
				{
					File.Delete(emptyTargetPath);
					WriteRecoverAction(key0, "DELETE");
				}
				else
				{
					WriteRecoverAction(key0, "NOTHING_DELETE");
				}
			}
			else
			{
				var saveTargetPath = GetSaveTargetPath(key0);
				if (string.IsNullOrEmpty(saveTargetPath))
				{
					// no backup target
					WriteRecoverAction(key0, "NO_BACKUP");
				}
				else
				{
					var backupFile = Path.Combine(saveDir, "AddressablesCatalog.Save." + key0);
					if (File.Exists(backupFile))
					{
						File.Copy(backupFile, saveTargetPath, true);
						WriteRecoverAction(key0, "DELETE");
					}
					else
					{
						Debug.LogError($"no backup file: {backupFile}");
						WriteRecoverAction(key0, "MISSING_BACKUP");
					}
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogError("RecoverFile failed:");
			Debug.LogException(e);
		}
	}
	protected static bool fixedOnce = false;
	/// <summary>
	/// 针对热更失败, 恢复更新索引
	/// </summary>
	/// <param name="force"></param>
	public static void FixCatalog(bool force = false)
	{
		if (fixedOnce && (!force))
		{
			return;
		}
		fixedOnce = true;

		RecoverFile("hash");
		RecoverFile("json");
	}
	public static void SaveCatalog()
	{
		{
			var ResourceManager = Addressables.ResourceManager;
			var locators=Addressables.m_AddressablesInstance.m_ResourceLocators;
			var locations = locators.Select(l =>
			{
				return l.CatalogLocation;
			});
			foreach (var loc in locations)
			{
				if (loc == null)
				{
					continue;
				}
				if (loc.Dependencies.Count > (int)ContentCatalogProvider.DependencyHashIndex.Cache)
				{
					var hashFilePath = ResourceManager.TransformInternalId(loc.Dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Cache]);
					var cacheFilePath = hashFilePath.Replace(".hash", ".json");
					BackUpFile(hashFilePath, "hash");
					BackUpFile(cacheFilePath, "json");
				}
				else
				{
					Debug.LogWarning($"No ContentCatalogProvider[Cache] found");
				}
			}
		}
	}
	protected static void BackUpFile(string path0, string key0)
	{
		if (GetSaveTargetPath(key0) != null)
		{
			return;
		}

		try
		{
			var saveDir = Application.persistentDataPath;
			if (File.Exists(path0))
			{
				var backupFilePath = Path.Combine(saveDir, "AddressablesCatalog.Save." + key0);
				PlayerPrefs.SetString("AddressablesCatalog.LoadPath." + key0, path0);
				File.Copy(path0, backupFilePath, true);
				File.WriteAllText(Path.Combine(saveDir, "AddressablesCatalog.LoadPath." + key0), path0);
			}
			else
			{
				var emptyMark = Path.Combine(saveDir, "AddressablesCatalog.EmptyMark." + key0);
				File.WriteAllText(emptyMark, path0);
			}
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}
	}

}
