﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.GameLib.MonoUtils;
using UISys.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;
using Slider = UnityEngine.UI.Slider;

namespace UISys.Editor
{
	[CustomPropertyDrawer(typeof(UIAction))]
	public class UIActionEditor : UnityEditor.PropertyDrawer
	{
		public struct CompMethodInfo
		{
			public string ActionDisplayName;
			public string CompFullName;
			public MethodInfo Method;
			public Component Comp;
		}

		public class AdditionComp
		{
			public string CompFullName => CompType.FullName;
			public Type CompType;
			public string CompTypeName;

			public string[] MethodNames;

			protected static AdditionComp[] AdditionComps;

			public static AdditionComp[] GetAdditionComps()
			{
				if (AdditionComps == null)
				{
					AdditionComps = new[]
					{
						new AdditionComp()
						{
							CompType = null,
							MethodNames = new[]
							{
								"SetActive",
							}
						},
						new AdditionComp()
						{
							CompType = typeof(Transform),
							MethodNames = new[]
							{
								"SetPositionAndRotation",
							}
						},
						new AdditionComp
						{
							CompType = typeof(Slider),
							MethodNames = new string[]
							{
								"set_value",
							}
						},
						new AdditionComp
						{
							CompTypeName = "DG.Tweening.DOTweenAnimation",
							CompType = null,
							MethodNames = null,
						}
					};
				}

				if (AdditionComps.Any(additionComp =>
					    !string.IsNullOrEmpty(additionComp.CompTypeName) && additionComp.CompType == null))
				{
					var ass = AppDomain.CurrentDomain.GetAssemblies();
					foreach (var additionComp in AdditionComps)
					{
						if (!string.IsNullOrEmpty(additionComp.CompTypeName) && additionComp.CompType == null)
						{
							var typeName = additionComp.CompTypeName;
							Type type = null;
							foreach (var assembly in ass)
							{
								type = assembly.GetType(typeName);
								if (type != null)
								{
									break;
								}
							}

							// Debug.Assert(type != null);

							additionComp.CompType = type;
						}
					}
				}

				return AdditionComps;
			}
		}

		private int _propCount = 1;
		private int _lineCount = 1;

		protected Rect CalculcateRect(Rect pre)
		{
			var x = pre.xMax;
			var y = pre.y;
			var height = pre.height;

			if (x >= PropWidth && x + PropWidth > WindowWidth)
			{
				x = StartX;
				y += PropHeight + DivHeight;
				// _lineCount++;
			}
			else
			{
				x += DivWidth;
			}

			// _propCount++;

			var rect = new Rect(x, y, PropWidth, height);
			return rect;
		}

		protected float PropWidth = 120;
		protected float DivWidth = 10;
		protected float WindowWidth = 120;
		protected float PropHeight = 18;
		protected float StartX = 0;
		const float MinWidth = 80;
		const float WrapWidth = 200;
		const float DivHeight = 2;

		private int _lastEleCount = 1;

		private void AdaptLayout(Rect position, SerializedProperty property, GUIContent label)
		{
			UpdateEleCount(property);

			var lastPropCount = _propCount;
			// _propCount = 1;
			// _lineCount = 1;

			StartX = position.x;

			WindowWidth = EditorGUIUtility.currentViewWidth;
			var viewWindowWidth = WindowWidth - 40;
			var currentWidth = viewWindowWidth / _lastEleCount - DivWidth;
			if (MinWidth > viewWindowWidth)
			{
				PropWidth = viewWindowWidth;
				_lineCount = _propCount;
			}
			else if (MinWidth <= currentWidth && currentWidth <= WrapWidth)
			{
				var eleCount = _lastEleCount;
				_lineCount = (_propCount + eleCount - 1) / eleCount;
				PropWidth = currentWidth;
			}
			else if (MinWidth * lastPropCount + DivWidth * (lastPropCount - 1) > viewWindowWidth)
			{
				var wrapWidth = currentWidth > WrapWidth ? WrapWidth : MinWidth;
				var eleCount = Mathf.FloorToInt((viewWindowWidth + DivWidth) / (wrapWidth + DivWidth));
				PropWidth = viewWindowWidth / eleCount - DivWidth;
				_lineCount = (_propCount + eleCount - 1) / eleCount;
				_lastEleCount = eleCount;
			}
			else
			{
				PropWidth = viewWindowWidth / lastPropCount - DivWidth;
				_lineCount = 1;
			}

			PropHeight = base.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			try
			{
				DrawProps(position, property, label);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public bool DrawParaTitle(ref Rect rect, string title)
		{
			var labelWidth = Mathf.Min(rect.width - MinWidth, title.Length * 6f + (title.Length - 1) * 0.15f + 1.5f);
			var offset = DivWidth - 4;
			if (rect.x == StartX)
			{
				offset = 0;
			}

			if (rect.width > MinWidth + labelWidth)
			{
				var titleRect = new Rect(rect.x - offset, rect.y, labelWidth, rect.height);
				EditorGUI.SelectableLabel(titleRect, title, new GUIStyle()
				{
					stretchWidth = true,
				});
				rect.xMin += (labelWidth - offset);
				return true;
			}

			return false;
		}

		protected void UpdateEleCount(SerializedProperty property)
		{
			_propCount = 0;

			_propCount++;

			var selfProp = property.FindPropertyRelative("self");
			var selfObjProp = property.FindPropertyRelative("selfObj");
			Object selfObj0 = selfObjProp.objectReferenceValue;
			AssetReference objRef = null;

			try
			{
				if (selfObj0 == null)
				{
					objRef = selfProp.boxedValue as AssetReference;
					if (objRef != null && objRef.RuntimeKeyIsValid())
					{
						selfObj0 = WaitForCompletion(objRef);
					}
				}

				if (selfObj0 != null)
				{
					_propCount++;
					var compProp = property.FindPropertyRelative("comp");
					if (selfObj0 is GameObject gameObject && !string.IsNullOrEmpty(compProp.stringValue))
					{
						var actionProp = property.FindPropertyRelative("action");
						if (!string.IsNullOrEmpty(actionProp.stringValue))
						{
							var comp = gameObject.GetComponent(compProp.stringValue);
							if (comp == null)
							{
								var type = Assembly.GetAssembly(typeof(UnityEngine.Transform))
									.GetType(compProp.stringValue);
								if (type != null)
								{
									comp = gameObject.GetComponent(type);
								}
							}

							if (comp != null)
							{
								var methodField = comp.GetType()
									.GetMethod(actionProp.stringValue, BindingFlags.Instance | BindingFlags.Public);

								if (methodField != null)
								{
									var paraInfos = methodField.GetParameters();

									_propCount += paraInfos.Length;
								}

								Debug.Assert(methodField != null);
							}
						}
					}
					else
					{
						var actionProp = property.FindPropertyRelative("action");
						if (!string.IsNullOrEmpty(actionProp.stringValue))
						{
							var methodField = selfObj0.GetType()
								.GetMethod(actionProp.stringValue, BindingFlags.Instance | BindingFlags.Public);
							// Debug.Assert(methodField != null);
							if (methodField != null)
							{
								var paraInfos = methodField.GetParameters();

								_propCount += paraInfos.Length;
							}
						}
					}
				}
			}
			finally
			{
				if (objRef != null && objRef.IsValid())
				{
					objRef.ReleaseAsset();
				}
			}
		}

		private static Object WaitForCompletion(AssetReference objRef)
		{
			Object selfObj0;
			var op = objRef.LoadAssetAsync<Object>();
			var ret = op.GetType().GetMethod("WaitForCompletion", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(op, new object[0]);
			selfObj0 = (Object)ret;
			return selfObj0;
		}

		public void DrawProps(Rect position, SerializedProperty property, GUIContent label)
		{
			AdaptLayout(position, property, label);

			var selfProp = property.FindPropertyRelative("self");
			var selfObjProp = property.FindPropertyRelative("selfObj");
			Object selfObj0 = selfObjProp.objectReferenceValue;
			AssetReference objRef = null;
			try
			{
				if (selfObj0 == null)
				{
					objRef = selfProp.boxedValue as AssetReference;
					if (objRef != null && objRef.RuntimeKeyIsValid())
					{
						// selfObj0 = objRef.LoadAssetAsync<Object>().WaitForCompletion();
						selfObj0 = WaitForCompletion(objRef);
					}
				}

				var selfPropRect = new Rect(StartX, position.y, PropWidth, PropHeight);
				// DrawParaTitle(ref selfPropRect, "self");
				EditorGUI.BeginChangeCheck();
				// EditorGUI.PropertyField(selfPropRect, selfProp, GUIContent.none);
				var ret = EditorGUI.ObjectField(selfPropRect, GUIContent.none, selfObj0, typeof(Object), true);
				if (EditorGUI.EndChangeCheck())
				{
					var targetObj = (Component)property.serializedObject.targetObject;
					if (AssetDatabase.IsMainAsset(ret) && ret != targetObj.gameObject)
					{
						var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(ret));
						objRef = new AssetReference(guid);
						selfProp.boxedValue = objRef;
						selfObjProp.objectReferenceValue = null;
					}
					else
					{
						selfObjProp.objectReferenceValue = ret;

						var nullRef = (objRef != null && objRef.IsValid() == false) ? objRef : new AssetReference();
						selfProp.boxedValue = nullRef;
					}
				}

				if (selfObj0 != null)
				{
					DrawObjectAndActions(property, selfObj0, selfPropRect);
				}
			}
			finally
			{
				if (objRef != null && objRef.IsValid())
				{
					objRef.ReleaseAsset();
					objRef = null;
				}
			}
		}

		private void DrawObjectAndActions(SerializedProperty property, Object asset, Rect selfPropRect)
		{
			var assetType = asset.GetType();
			if (asset is GameObject gameObject)
			{
				// draw gameobject methods

				var compMethodInfosE = Enumerable.Empty<CompMethodInfo>();
				var comps = gameObject.GetComponents<Component>();
				var excludeComps = Array.Empty<Component>();
				DrawGameObjectAndComps(property, selfPropRect, comps, excludeComps, gameObject, assetType,
					compMethodInfosE);
			}
			else if (asset is Component comp0)
			{
				// draw comp and gameobject methods

				var compMethodInfosE = Enumerable.Empty<CompMethodInfo>();

				gameObject = comp0.gameObject;
				var comps = gameObject.GetComponents<Component>();

				var execludeIndex = Array.IndexOf(comps, comp0);
				compMethodInfosE = CollectCompMethods(compMethodInfosE, comp0, null, execludeIndex);

				var excludeComps = new Component[] { comp0 };
				var mainType = comp0.GetType();
				DrawGameObjectAndComps(property, selfPropRect, comps, excludeComps, gameObject, mainType,
					compMethodInfosE);
			}
			else
			{
				// draw object methods
				var compMethodInfosE = Enumerable.Empty<CompMethodInfo>();

				var methodInfos = assetType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
				var assetFullName = assetType.FullName;
				var compMethodInfos1 = WrapMarkedMethods(methodInfos, null, assetFullName);
				compMethodInfosE = compMethodInfosE.Concat(compMethodInfos1);

				var compMethodInfos = compMethodInfosE.ToArray();

				var actionProp = property.FindPropertyRelative("action");
				var actionIndex = -1;
				for (var index = 0; index < compMethodInfos.Length; index++)
				{
					var compMethodInfo = compMethodInfos[index];
					if (compMethodInfo.Method.Name == actionProp.stringValue)
					{
						actionIndex = index;
						break;
					}
				}

				var actionPropRect = CalculcateRect(selfPropRect);
				var methodNames = compMethodInfos.Select(info => info.ActionDisplayName).ToArray();
				DrawParaTitle(ref actionPropRect, "action");
				EditorGUI.BeginChangeCheck();
				var selectIndex = EditorGUI.Popup(actionPropRect, actionIndex, methodNames);
				if (EditorGUI.EndChangeCheck())
				{
					var compMethodInfo = compMethodInfos[selectIndex];
					actionProp.stringValue = compMethodInfo.Method.Name;

					var compProp = property.FindPropertyRelative("comp");
					compProp.stringValue = "";

					ClearParas(property);
				}

				if (0 <= selectIndex && selectIndex < compMethodInfos.Length)
				{
					var selectCompMethodInfo = compMethodInfos[selectIndex];

					DrawActionParas(property, selectCompMethodInfo, actionPropRect, actionProp);
				}
			}
		}

		private void DrawGameObjectAndComps(SerializedProperty property, Rect selfPropRect, Component[] comps,
			Component[] excludeComps,
			GameObject gameObject, Type assetType, IEnumerable<CompMethodInfo> compMethodInfosE)
		{
			// foreach (var comp in comps)
			for (var i = 0; i < comps.Length; i++)
			{
				var comp = comps[i];
				if (comp == null)
				{
					continue;
				}

				if (!excludeComps.Contains(comp))
				{
					var compFullName = comp.GetType().FullName;
					compMethodInfosE = CollectCompMethods(compMethodInfosE, comp, compFullName, i);
				}
			}

			// add additional actions for native objs
			{
				compMethodInfosE = AddAdditionComps(gameObject, assetType, compMethodInfosE);
			}

			var compMethodInfos = compMethodInfosE.ToArray();

			var compProp = property.FindPropertyRelative("comp");
			var actionProp = property.FindPropertyRelative("action");
			var actionIndex = -1;
			for (var index = 0; index < compMethodInfos.Length; index++)
			{
				var compMethodInfo = compMethodInfos[index];
				if (compMethodInfo.Method.Name == actionProp.stringValue &&
				    (
					    assetType.IsSubclassOf(typeof(Component))
					    || compMethodInfo.CompFullName == compProp.stringValue
				    ))
				{
					actionIndex = index;
					break;
				}
			}

			var actionPropRect = CalculcateRect(selfPropRect);
			var methodNames = compMethodInfos.Select(info => info.ActionDisplayName).ToArray();
			DrawParaTitle(ref actionPropRect, "action");
			EditorGUI.BeginChangeCheck();
			var selectIndex = EditorGUI.Popup(actionPropRect, actionIndex, methodNames);
			if (EditorGUI.EndChangeCheck())
			{
				var compMethodInfo = compMethodInfos[selectIndex];
				actionProp.stringValue = compMethodInfo.Method.Name;

				var selfObjProp = property.FindPropertyRelative("selfObj");
				if (selfObjProp.objectReferenceValue != null)
				{
					compProp.stringValue = "";
					if (compMethodInfo.Comp != null)
					{
						// if is selfObj
						selfObjProp.objectReferenceValue = compMethodInfo.Comp;
					}
					else
					{
						selfObjProp.objectReferenceValue = gameObject;
					}
				}
				else
				{
					compProp.stringValue = compMethodInfo.CompFullName;
				}

				ClearParas(property);
			}

			if (0 <= selectIndex && selectIndex < compMethodInfos.Length)
			{
				var selectCompMethodInfo = compMethodInfos[selectIndex];

				DrawActionParas(property, selectCompMethodInfo, actionPropRect, actionProp);
			}
		}

		private static IEnumerable<CompMethodInfo> CollectCompMethods(IEnumerable<CompMethodInfo> compMethodInfosE,
			Component comp, string compFullName, int index)
		{
			var methodInfos = comp.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
			var compMethodInfos1 = WrapMarkedMethods(methodInfos, comp, compFullName, index);
			compMethodInfosE = compMethodInfosE.Concat(compMethodInfos1);
			return compMethodInfosE;
		}

		private static IEnumerable<CompMethodInfo> AddAdditionComps(GameObject gameObject, Type assetType,
			IEnumerable<CompMethodInfo> compMethodInfosE)
		{
			var additionComps = AdditionComp.GetAdditionComps();
			foreach (var additionComp in additionComps)
			{
				var compBaseType = additionComp.CompType;
				if (compBaseType != null && !assetType.IsSubclassOf(compBaseType))
				{
					var comp = gameObject.GetComponent(compBaseType);
					if (comp != null)
					{
						var directType = comp != null ? comp.GetType() : additionComp.CompType;
						var methodNames2 = additionComp.MethodNames;
						MethodInfo[] methodInfos2;
						if (additionComp.MethodNames == null)
						{
							methodInfos2 = directType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
								.Where(m =>
									m.DeclaringType == directType ||
									m.DeclaringType.IsSubclassOf(directType))
								.ToArray();
						}
						else
						{
							methodInfos2 = methodNames2.Select(name =>
									directType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public))
								.ToArray();
						}

						var compFullName = directType.FullName;
						var compMethodInfos1 = WrapNativeMethods(methodInfos2, compFullName, comp);
						compMethodInfosE = compMethodInfosE.Concat(compMethodInfos1);
					}
				}
				else if (string.IsNullOrEmpty(additionComp.CompTypeName))
				{
					// CompTypeName == null && CompType == null, 那么即为普通object
					var isGameObject = assetType == typeof(GameObject);
					assetType = typeof(GameObject);
					var methodNames2 = additionComp.MethodNames;
					MethodInfo[] methodInfos2;
					if (additionComp.MethodNames == null)
					{
						methodInfos2 = assetType.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(
								m =>
									m.DeclaringType == assetType ||
									m.DeclaringType.IsSubclassOf(assetType))
							.ToArray();
					}
					else
					{
						methodInfos2 = methodNames2.Select(name =>
								assetType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public))
							.Where(m => m != null)
							.ToArray();
					}

					var compMethodInfos1 = WrapNativeMethods(methodInfos2, isGameObject ? "" : nameof(GameObject), null);
					compMethodInfosE = compMethodInfosE.Concat(compMethodInfos1);
				}
			}

			return compMethodInfosE;
		}

		public class MethodInfoCompare : IEqualityComparer<MethodInfo>
		{
			public bool Equals(MethodInfo x, MethodInfo y)
			{
				return x.Name == y.Name;
			}

			public int GetHashCode(MethodInfo obj)
			{
				return obj.GetHashCode();
			}
		}

		private static IEnumerable<CompMethodInfo> WrapMarkedMethods(MethodInfo[] methodInfos, Component comp,
			string assetFullName,
			int index = -1)
		{
			var compMethodInfos1 = methodInfos
				.Where(m => m.GetCustomAttribute<UIActionAttribute>(true) != null)
				.Distinct(new MethodInfoCompare())
				.Select(m => new CompMethodInfo
				{
					ActionDisplayName =
						ToActionDisplayName(assetFullName, m, index),
					// ActionDisplayName = $"{m.Name}",
					CompFullName = assetFullName,
					Method = m,
					Comp = comp,
				});
			return compMethodInfos1;
		}

		private static string ToActionDisplayName(string selfName, MethodInfo m, int index)
		{
			var head = string.IsNullOrEmpty(selfName)
				? (index >= 0 ? $"{index}. {m.Name}" : m.Name)
				: (index >= 0 ? $"{index}. {selfName}/{m.Name}" : $"{selfName}/{m.Name}");
			var divCount = 4 - (head.Length % 4);
			var end = "";
			if (m.GetParameters().Length > 0)
			{
				end = $"({string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})";
			}

			return $"{head} {new string(' ', divCount)}{end}";
		}

		private static IEnumerable<CompMethodInfo> WrapNativeMethods(MethodInfo[] methodInfos, string compFullName,
			Component comp)
		{
			var compMethodInfos1 = methodInfos
				// .Where(m => m.GetCustomAttribute<UIActionAttribute>(true) != null)
				.Select(m => new CompMethodInfo
				{
					ActionDisplayName = ToActionDisplayName(compFullName, m, -1),
					// ActionDisplayName = $"{m.Name}",
					CompFullName = compFullName,
					Method = m,
					Comp = comp,
				});
			return compMethodInfos1;
		}

		private static void ClearParas(SerializedProperty property)
		{
			// var paraArrayProp = property.FindPropertyRelative("paras");
			// paraArrayProp.ClearArray();
			// var uparaArrayProp = property.FindPropertyRelative("uparas");
			// uparaArrayProp.ClearArray();
		}

		private void DrawActionParas(SerializedProperty property, CompMethodInfo selectCompMethodInfo,
			Rect actionPropRect, SerializedProperty actionProp)
		{
			Rect paraPropRect = actionPropRect;
			if (!string.IsNullOrEmpty(actionProp.stringValue))
			{
				var methodField = selectCompMethodInfo.Method;
				var paraInfos = methodField.GetParameters();
				var parasCount0 = paraInfos.Count(info => !info.ParameterType.IsSubclassOf(typeof(UnityEngine.Object)));
				var uparasCount0 = paraInfos.Count(info => info.ParameterType.IsSubclassOf(typeof(UnityEngine.Object)));
				var paraArrayProp = property.FindPropertyRelative("paras");
				var paraCount = 0;
				var uparaArrayProp = property.FindPropertyRelative("uparas");
				var uParaCount = 0;
				paraArrayProp.arraySize = parasCount0;
				uparaArrayProp.arraySize = uparasCount0;
				for (var i = 0; i < paraInfos.Length; i++)
				{
					var paraInfo = paraInfos[i];
					// paraPropRect = new Rect(paraPropRect.xMax + 20, position.y, 150, position.height);
					paraPropRect = CalculcateRect(paraPropRect);
					var isTitleDraw = DrawParaTitle(ref paraPropRect, paraInfo.Name);

					var paraType = paraInfo.ParameterType;
					if (paraType.IsSubclassOf(typeof(UnityEngine.Object)))
					{
						Debug.Assert(uParaCount <= uparaArrayProp.arraySize);
						// if (uparaArrayProp.arraySize <= uParaCount)
						// {
						// 	uparaArrayProp.InsertArrayElementAtIndex(uParaCount);
						// }

						var paraProp = uparaArrayProp.GetArrayElementAtIndex(uParaCount++);
						EditorGUI.ObjectField(paraPropRect, paraProp, paraType, GUIContent.none);
					}
					else
					{
						Debug.Assert(paraCount <= paraArrayProp.arraySize);
						// if (paraArrayProp.arraySize <= paraCount)
						// {
						// 	paraArrayProp.InsertArrayElementAtIndex(paraCount);
						// }

						if (paraType == typeof(string))
						{
							var paraProp = paraArrayProp.GetArrayElementAtIndex(paraCount++);
							EditorGUI.PropertyField(paraPropRect, paraProp, GUIContent.none);
						}
						else if (
							paraType == typeof(bool)
						)
						{
							var paraProp = paraArrayProp.GetArrayElementAtIndex(paraCount++);

							var boolOptions = new string[] { false.ToString(), true.ToString() };
							var index = Array.IndexOf(boolOptions, paraProp.stringValue);
							var toggleRect = WrapToggleRect(paraPropRect, isTitleDraw);
							EditorGUI.BeginChangeCheck();
							var check = EditorGUI.Toggle(toggleRect, GUIContent.none, index == 1);
							if (EditorGUI.EndChangeCheck())
							{
								paraProp.stringValue = boolOptions[check ? 1 : 0];
							}
						}
						else if (
							paraType == typeof(int)
							|| paraType == typeof(float)
							|| paraType == typeof(long)
							|| paraType == typeof(double)
						)
						{
							var paraProp = paraArrayProp.GetArrayElementAtIndex(paraCount++);

							EditorGUI.BeginChangeCheck();
							object inputValue;
							var isParseFailed = false;
							var text = paraProp.stringValue;
							if (paraType == typeof(int))
							{
								inputValue = EditorGUI.IntField(paraPropRect, GUIContent.none,
									BaseTypeParseHelper.ParseInt(text, ref isParseFailed));
							}
							else if (paraType == typeof(float))
							{
								inputValue = EditorGUI.FloatField(paraPropRect, GUIContent.none,
									BaseTypeParseHelper.ParseFloat(text, ref isParseFailed));
							}
							else if (paraType == typeof(double))
							{
								inputValue = EditorGUI.DoubleField(paraPropRect, GUIContent.none,
									BaseTypeParseHelper.ParseDouble(text, ref isParseFailed));
							}
							else if (paraType == typeof(long))
							{
								inputValue = EditorGUI.LongField(paraPropRect, GUIContent.none,
									BaseTypeParseHelper.ParseLong(text, ref isParseFailed));
							}
							else
							{
								throw new NotImplementedException();
							}

							if (EditorGUI.EndChangeCheck())
							{
								paraProp.stringValue = inputValue.ToString();
							}
						}
						else if (paraType == typeof(Vector2)
						         || paraType == typeof(Vector3)
						         || paraType == typeof(Vector4)
						         || paraType == typeof(Quaternion)
						        )
						{
							var paraProp = paraArrayProp.GetArrayElementAtIndex(paraCount++);

							EditorGUI.BeginChangeCheck();
							object inputValue;
							var isParseFailed = false;
							var text = paraProp.stringValue;
							if (paraType == typeof(Vector2))
							{
								inputValue = EditorGUI.Vector2Field(paraPropRect, GUIContent.none,
									ParseJson<Vector2>(text, ref isParseFailed));
							}
							else if (paraType == typeof(Vector3))
							{
								inputValue = EditorGUI.Vector3Field(paraPropRect, GUIContent.none,
									ParseJson<Vector3>(text, ref isParseFailed));
							}
							else if (paraType == typeof(Vector4))
							{
								inputValue = EditorGUI.Vector4Field(paraPropRect, GUIContent.none,
									ParseJson<Vector4>(text, ref isParseFailed));
							}
							else if (paraType == typeof(Quaternion))
							{
								var qt0 = ParseJson<Quaternion>(text, ref isParseFailed);
								var rt0 = qt0.eulerAngles;
								var rt1 = EditorGUI.Vector3Field(paraPropRect, GUIContent.none,
									rt0);
								var qt = Quaternion.Euler(rt1);
								inputValue = qt;
							}
							else
							{
								throw new NotImplementedException();
							}

							if (EditorGUI.EndChangeCheck() || isParseFailed)
							{
								paraProp.stringValue = JsonUtility.ToJson(inputValue);
							}
						}
						else if (
							paraType == typeof(AssetReference) ||
							paraType.IsSubclassOf(typeof(AssetReference))
						)
						{
							var baseType = paraType;
							Type gType = null;
							if (baseType != typeof(AssetReference))
							{
								while (baseType.BaseType != typeof(AssetReference))
								{
									baseType = baseType.BaseType;
								}

								if (baseType.IsGenericType)
								{
									gType = baseType.GenericTypeArguments[0];
								}
							}

							if (gType == null)
							{
								gType = typeof(Object);
							}

							var paraProp = paraArrayProp.GetArrayElementAtIndex(paraCount++);
							AssetReference value1;
							var isParseFailed = false;
							if (string.IsNullOrEmpty(paraProp.stringValue))
							{
								value1 = null;
								paraProp.stringValue = "";
							}
							else
							{
								try
								{
									value1 = (AssetReference)JsonUtility.FromJson(paraProp.stringValue,
										paraType);
								}
								catch
								{
									isParseFailed = true;
									value1 = null;
								}
							}

							var value2 = value1 != null && value1.RuntimeKeyIsValid()
								? AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(value1.AssetGUID),
									gType)
								: null;
							var filterType = gType;
							if (gType.IsSubclassOf(typeof(Component)))
							{
								filterType = typeof(GameObject);
							}

							EditorGUI.BeginChangeCheck();
							var value3 = EditorGUI.ObjectField(paraPropRect, GUIContent.none, value2,
								filterType, true);
							if (EditorGUI.EndChangeCheck() || isParseFailed)
							{
								if (gType.IsSubclassOf(typeof(Component)) && value3 is GameObject gameObject)
								{
									value3 = gameObject.GetComponent(gType);
								}

								var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value3));
								paraProp.stringValue = JsonUtility.ToJson(new AssetReference(guid));
							}
						}
						else
						{
							throw new NotImplementedException();
						}
					}
				}
			}
		}

		private Rect WrapToggleRect(Rect paraPropRect, bool isTitleDrawed)
		{
			if (isTitleDrawed)
			{
				float offset = 10;
				return new Rect(paraPropRect.x + offset, paraPropRect.y, paraPropRect.width - offset,
					paraPropRect.height);
			}

			return paraPropRect;
		}

		private static T ParseJson<T>(string text, ref bool isParseFailed)
		{
			if (string.IsNullOrEmpty(text))
			{
				return default;
			}
			else
			{
				try
				{
					return JsonUtility.FromJson<T>(text);
				}
				catch
				{
					isParseFailed = true;
					return default;
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Rect scale = GUILayoutUtility.GetLastRect();
			AdaptLayout(scale, property, label);

			var height = base.GetPropertyHeight(property, label);
			return height * _lineCount + DivHeight * (_lineCount - 1);
		}
	}
}