using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets.MyExt;
using Framework.GameLib.MonoUtils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UnityVisualize.Runtime
{
	[AttributeUsage(AttributeTargets.Method)]
	public class UIActionAttribute : Attribute
	{
	}

	// TODO: 支持异步设置
	[Serializable]
	public class UIAction
	{
		// continue next without wait this async func
		public bool wait;
		public AssetReference self;
		public Object selfObj;
		public string comp;
		public string action;
		public string[] paras;
		public Object[] uparas;
	}

	[ExecuteInEditMode]
	[AddComponentMenu("UISys/EaseActions")]
	public class EaseActionsComp : MonoBehaviour
	{
		/// <summary>
		/// 参数传递方式
		/// </summary>
		public enum ParaPassType
		{
			None,
			PassFirstOnly,
			PassOneByOne,
			PassEvery,
		}

		[SerializeField] protected ParaPassType paraPassType;
		[SerializeField] protected UIAction[] actions;

		public UIAction[] Actions => actions;

#if UNITY_EDITOR
		[NonSerialized] public bool IsReseted = false;

		private void Reset()
		{
			IsReseted = true;
		}

		protected void OnEnable()
		{
		}
#endif

		public void Run()
		{
			_ = RunActions();
		}

		public void RunWithToggle(Toggle change)
		{
			var isOn = change.isOn;

			_ = RunActionsWithOnePara(isOn);
		}

		public void RunWithToggle(MyToggle change)
		{
			var isOn = change.isOn;

			_ = RunActionsWithOnePara(isOn);
		}

		public async Task RunActionsWithOnePara(bool isOn)
		{
			await RunActionsWithOnePara(this.paraPassType, isOn);
		}

		public async Task RunActions()
		{
			await RunActionsWithOnePara(this.paraPassType, null);
		}

		public class RunContext
		{
			public bool Exit = false;
			public ParaPassType ParaPassType;
			public List<Task> PendingTasks;

			internal void AddPendingTask(Task pendingTask)
			{
				if (PendingTasks == null)
				{
					PendingTasks = new();
				}

				PendingTasks.Add(pendingTask);
			}
		}

		protected RunContext CurContext;

		public Task RunActionsWithOnePara(object para)
		{
			return RunActionsWithOnePara(this.paraPassType, para);
		}

		protected async Task RunActionsWithOnePara(ParaPassType paraPassType0, object para)
		{
			if (!enabled)
			{
				return;
			}

			var runContext = new RunContext()
			{
				ParaPassType = paraPassType0,
			};

			bool isFirst = true;
			Task pendingTask = null;
			object nextPara = para;
			foreach (var uiAction in actions)
			{
				if (runContext.Exit)
				{
					break;
				}

				paraPassType0 = runContext.ParaPassType;
				var usePara = paraPassType0 switch
				{
					ParaPassType.PassFirstOnly => isFirst,
					ParaPassType.None => false,
					ParaPassType.PassOneByOne => true,
					ParaPassType.PassEvery => true,
					_ => throw new ArgumentOutOfRangeException(nameof(paraPassType0), paraPassType0, null)
				};
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

					if (usePara && nextPara != null && callParas.Length > 0)
					{
						var callPara0 = callParas[0];
						var callPara0Type = callPara0.GetType();
						var paraType = nextPara.GetType();
						if (paraType == callPara0Type ||
						    paraType.IsSubclassOf(callPara0Type))
						{
							if (nextPara is bool bPara)
							{
								// 特殊规则：当读档参数为 true，则传入参数取反
								callParas[0] = ((bool)callPara0) ? !bPara : bPara;
							}
							else
							{
								callParas[0] = nextPara;
							}
						}
					}

					CurContext = runContext;
					var ret = methodInfo!.Invoke(caller, callParas);

					if (paraPassType0 == ParaPassType.PassOneByOne)
					{
						object result;
						if (ret is Task task)
						{
							if (uiAction.wait)
							{
								pendingTask = task;

								await task;
								var resultProp = task.GetType().GetProperty("Result");
								result = resultProp?.GetValue(task);
							}
							else
							{
								runContext.AddPendingTask(task);
								pendingTask = null;

								result = ret;
							}
						}
						else
						{
							result = ret;
						}

						nextPara = result;
					}
					else
					{
						if (ret is Task task)
						{
							if (uiAction.wait)
							{
								pendingTask = task;

								await task;
							}
							else
							{
								runContext.AddPendingTask(task);
								pendingTask = null;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					if (paraPassType0 == ParaPassType.PassOneByOne)
					{
						runContext.Exit = true;
					}
				}
				finally
				{
					// uiActionSelf.ReleaseAsset();
					_ = uiActionSelf.DelayRelease();
				}
			}

			if (pendingTask != null)
			{
				runContext.AddPendingTask(pendingTask);
				pendingTask = null;
			}

			if (runContext.PendingTasks != null)
			{
				await Task.WhenAll(runContext.PendingTasks);
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