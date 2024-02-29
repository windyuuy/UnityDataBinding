using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace UISys.Runtime
{
	[AttributeUsage(AttributeTargets.Method)]
	public class UIActionAttribute : Attribute
	{
	}

	[Serializable]
	public class UIAction
	{
		public AssetReference self;
		public Object selfObj;
		public string comp;
		public string action;
		public string[] paras;
		public Object[] uparas;
	}

	[ExecuteInEditMode]
	[AddComponentMenu("UISys/UIActions")]
	public class UIActionsComp : MonoBehaviour
	{
		public UIAction[] actions;

#if UNITY_EDITOR
		[NonSerialized] public Action OnReset;
		private void Reset()
		{
			OnReset?.Invoke();
		}
#endif

		public void Run()
		{
			_ = RunActions();
		}

		public async Task RunActions()
		{
			foreach (var uiAction in actions)
			{
				Object asset = uiAction.selfObj;
				AssetReference uiActionSelf = null;
				if (asset == null)
				{
					uiActionSelf = uiAction.self;
					if (uiActionSelf == null || !uiActionSelf.RuntimeKeyIsValid())
					{
						continue;
					}

					// var asset = await LoadActionAsset(uiActionSelf);
					asset = await UILayerUtils.LoadAsset(uiActionSelf);
				}

				try
				{
					if (asset == null)
					{
						continue;
					}

					object ret;
					if (asset is GameObject go && !string.IsNullOrEmpty(uiAction.comp))
					{
						var comp = go.GetComponent(uiAction.comp);
						var methodInfo = comp.GetType()
							.GetMethod(uiAction.action, BindingFlags.Instance | BindingFlags.Public);
						var callParas = GetCallParas(methodInfo, uiAction);
						ret = methodInfo!.Invoke(comp, callParas);

						if (ret is Task task)
						{
							await task;
						}
					}
					else
					{
						var methodInfo = asset.GetType()
							.GetMethod(uiAction.action, BindingFlags.Instance | BindingFlags.Public);
						var callParas = GetCallParas(methodInfo, uiAction);
						ret = methodInfo!.Invoke(asset, callParas);

						if (ret is Task task)
						{
							await task;
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
				finally
				{
					// uiActionSelf.ReleaseAsset();
					_ = UILayerUtils.DelayRelease(uiActionSelf);
				}
			}
		}

		private static async Task<Object> LoadActionAsset(AssetReference self)
		{
			var asset = await self.LoadAssetAsync<UnityEngine.Object>().Task;
			return asset;
		}

		private static object[] GetCallParas(MethodInfo methodInfo, UIAction uiAction)
		{
			var paraIndex = -1;
			var uParaIndex = -1;
			var callParas = methodInfo.GetParameters().Select(paraInfo =>
			{
				if (paraInfo.ParameterType.IsSubclassOf(typeof(UnityEngine.Object)))
				{
					return uiAction.uparas[++uParaIndex];
				}
				else
				{
					var json = uiAction.paras[++paraIndex];
					var value = JsonUtility.FromJson(json, paraInfo.ParameterType);
					return value;
				}
			}).ToArray();
			return callParas;
		}

		protected void Run(BaseEventData eventData)
		{
			Run();
		}
	}
}