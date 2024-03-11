using System;
using System.Collections.Generic;
using System.Linq;
using Framework.UISys.GameUILayer.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityVisualize.Runtime;

namespace Framework.UISys.Editor
{
	[CustomEditor(typeof(UIPages))]
	public class UIPagesEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var comp = (UIPages)this.serializedObject.targetObject;

			var enableOld = GUI.enabled;
			GUI.enabled = true;

			EditorGUI.BeginChangeCheck();
			comp.EnableMirrorEditor = EditorGUILayout.Toggle(new GUIContent("开启镜像编辑"), comp.EnableMirrorEditor);
			if (EditorGUI.EndChangeCheck())
			{
			}

			EditorGUI.BeginChangeCheck();
			var enablePreview = false;
			for (var i = comp.transform.childCount - 1; i >= 0; i--)
			{
				var child = comp.transform.GetChild(i);
				if (child.name.StartsWith("~~") && child.name.EndsWith("~"))
				{
					enablePreview = true;
					break;
				}
			}

			enablePreview =
				EditorGUILayout.Toggle(new GUIContent("开启预览 (必须关掉预览才能保存预制体)", "必须关掉预览才能保存预制体"), enablePreview);

			GUI.enabled = enableOld;

			ShowPreview(comp, enablePreview);
		}

		private void ShowPreview(UIPages comp, bool enablePreview)
		{
			var mainCanvas = comp.GetComponentInParent<Canvas>();
			if (mainCanvas == null)
			{
				return;
			}

			var size = mainCanvas.renderingDisplaySize;
			if (!enablePreview)
			{
				comp.gameObject.hideFlags &= ~(HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.NotEditable);
			}

			if (EditorGUI.EndChangeCheck() || !comp.IsPreviewedOnce)
			{
				var isInPrefab = mainCanvas.transform.parent == null &&
				                 mainCanvas.name == "Canvas (Environment)"
				                 && !SceneManager.GetActiveScene().GetRootGameObjects()
					                 .Contains(mainCanvas.gameObject);
				var enablePreviewFirstTime = !isInPrefab && !comp.IsPreviewedOnce && !Application.isPlaying;
				if (enablePreviewFirstTime)
				{
					comp.IsPreviewedOnce = true;
					enablePreview = true;
				}

				if (enablePreview)
				{
					ClearAllNodes(comp);

					if (isInPrefab)
					{
						comp.gameObject.hideFlags |= HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
					}
					else
					{
						comp.gameObject.hideFlags |= HideFlags.NotEditable;
					}

					var toggles = comp.GetToggles().ToArray();
					if (toggles.Length > 0)
					{
						var wrapCount = Mathf.Ceil(Mathf.Sqrt(toggles.Length));

						var showCount = 0;
						foreach (var activeToggle in toggles)
						{
							var actionsComps = activeToggle.GetComponents<EaseActionsComp>();
							foreach (var uiActionsComp in actionsComps)
							{
								foreach (var uiAction in uiActionsComp.Actions)
								{
									if (uiAction.action == "OpenLayer" && uiAction.paras.Length >= 1)
									{
										var ss = JsonUtility.FromJson<AssetReferenceGameObject>(uiAction.paras[0]);
										var asset = AssetDatabase.LoadAssetAtPath<GameObject>(
											AssetDatabase.GUIDToAssetPath(ss.AssetGUID));
										var obj = PrefabUtility.InstantiatePrefab(asset, comp.transform) as GameObject;
										if (obj != null)
										{
											obj.name = "~~" + obj.name + "~";
											obj.transform.localPosition += new Vector3(
												showCount % wrapCount * size.x,
												(int)(showCount / wrapCount) * size.y,
												0);
											showCount++;

											obj.hideFlags |= HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
											// comp.PreviewNodes.Add(obj);
										}
									}
								}
							}
						}
					}
				}
				else
				{
					ClearAllNodes(comp);
					// PrefabUtility.RevertPrefabInstance(comp.gameObject, InteractionMode.AutomatedAction);
					comp.gameObject.hideFlags &= ~(HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.NotEditable);
				}
			}
		}

		private void ClearAllNodes(UIPages comp)
		{
			// var nodes = comp.PreviewNodes.ToArray();
			// comp.PreviewNodes.Clear();
			if (comp != null)
			{
				for (var i = comp.transform.childCount - 1; i >= 0; i--)
				{
					var child = comp.transform.GetChild(i);
					if (child.name.StartsWith("~~") && child.name.EndsWith("~"))
					{
						GameObject.DestroyImmediate(child.gameObject);
					}
				}
			}
		}
	}
}