using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Framework.GameLib.MonoUtils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UISys.Runtime
{
	[AttributeUsage(AttributeTargets.Method)]
	public class UIActionAttribute : Attribute
	{
	}

	// TODO: 支持异步设置
	[Serializable]
	public class UIAction
	{
		// public string run;
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
		[SerializeField] protected UIAction[] actions;

		public UIAction[] Actions => actions;

#if UNITY_EDITOR
		[NonSerialized] public bool IsReseted = false;
		private void Reset()
		{
			IsReseted = true;
		}
#endif

		protected void OnEnable()
		{
			
		}

		public void Run()
		{
			_ = RunActions();
		}

		public void RunWithToggle(Toggle change)
		{
			var isOn = change.isOn;

			_ = RunActionsWithFirstPara(isOn);
		}

		public void RunWithToggle(MyToggle change)
		{
			var isOn = change.isOn;

			_ = RunActionsWithFirstPara(isOn);
		}

		public async Task RunActionsWithFirstPara(bool isOn)
		{
			await RunActionsWithFirstPara(true, isOn);
		}

		public async Task RunActions()
		{
			await RunActionsWithFirstPara(false, null);
		}

		public class RunContext
		{
			public bool Exit = false;
		}

		protected RunContext CurContext;
		protected async Task RunActionsWithFirstPara(bool usePara, object para)
		{
			if (!enabled)
			{
				return;
			}
			
			var runContext = new RunContext();
			
			bool isFirst = true;
			foreach (var uiAction in actions)
			{
				if (runContext.Exit)
				{
					break;
				}
				
				var useFirstPara = isFirst && usePara;
				isFirst = false;

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
					asset = await uiActionSelf.LoadAssetFast();
				}

				try
				{
					if (asset == null)
					{
						continue;
					}

					object caller;
					object[] callParas;
					MethodInfo methodInfo;
					if (asset is GameObject go && !string.IsNullOrEmpty(uiAction.comp))
					{
						var comp = go.GetComponent(uiAction.comp);
						methodInfo = comp.GetType()
							.GetMethod(uiAction.action, BindingFlags.Instance | BindingFlags.Public);
						callParas = GetCallParas(methodInfo, uiAction);
						caller = comp;
					}
					else
					{
						methodInfo = asset.GetType()
							.GetMethod(uiAction.action, BindingFlags.Instance | BindingFlags.Public);
						callParas = GetCallParas(methodInfo, uiAction);
						caller = asset;
					}

					if (useFirstPara && para != null && callParas.Length > 0)
					{
						var callPara0 = callParas[0];
						var callPara0Type = callPara0.GetType();
						var paraType = para.GetType();
						if (paraType == callPara0Type ||
						    paraType.IsSubclassOf(callPara0Type))
						{
							if (para is bool bPara)
							{
								// 特殊规则：当读档参数为 true，则传入参数取反
								callParas[0] = ((bool)callPara0) ? !bPara : bPara;
							}
							else
							{
								callParas[0] = para;
							}
						}
					}

					CurContext = runContext;
					var ret = methodInfo!.Invoke(caller, callParas);

					if (ret is Task task)
					{
						await task;
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
					var text = uiAction.paras[++paraIndex];
					var paraType = paraInfo.ParameterType;
					object value;
					if (paraType == typeof(string))
					{
						value = text;
					}
					else if (paraType == typeof(bool))
					{
						value = BaseTypeParseHelper.ParseBool(text);
					}
					else if (paraType == typeof(int))
					{
						value = BaseTypeParseHelper.ParseInt(text);
					}
					else if (paraType == typeof(float))
					{
						value = BaseTypeParseHelper.ParseFloat(text);
					}
					else if (paraType == typeof(double))
					{
						value = BaseTypeParseHelper.ParseDouble(text);
					}
					else if (paraType == typeof(long))
					{
						value = BaseTypeParseHelper.ParseLong(text);
					}
					else
					{
						value = JsonUtility.FromJson(text, paraInfo.ParameterType);
					}

					return value;
				}
			}).ToArray();
			return callParas;
		}

		protected void Run(BaseEventData eventData)
		{
			Run();
		}

		/// <summary>
		/// 是否终止执行
		/// </summary>
		/// <param name="exit"></param>
		[UIAction]
		public void ExitIfBool(bool exit)
		{
			var runContext = this.CurContext;
			runContext.Exit = exit;
		}
	}
}