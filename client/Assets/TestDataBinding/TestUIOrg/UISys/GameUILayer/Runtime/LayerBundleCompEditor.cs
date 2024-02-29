using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UISys.Runtime
{
	[CustomEditor(typeof(LayerBundleComp), false)]
	public class LayerBundleCompEditor : Editor
	{
		private class NodeInfo
		{
			public string Guid;
			public Transform Obj;

			public NodeInfo(string guid, Transform obj)
			{
				Guid = guid;
				Obj = obj;
			}
		}

		protected bool NeedDrawPreview = false;

		public override void OnInspectorGUI()
		{
			var isDirty = false;
			var old = GUI.enabled;
			GUI.enabled = true;
			EditorGUI.BeginChangeCheck();
			var needDraw = EditorGUILayout.Toggle("启用预览", NeedDrawPreview);
			if (EditorGUI.EndChangeCheck())
			{
				NeedDrawPreview = needDraw;
				isDirty = true;
			}

			GUI.enabled = !NeedDrawPreview;
			base.OnInspectorGUI();
			GUI.enabled = old;

			if (isDirty)
			{
				OnPreSceneGUI();
			}
		}

		private void OnPreSceneGUI()
		{
			if (NeedDrawPreview)
			{
				var comp = (LayerBundleComp)this.target;
				var transform = comp.transform;

				transform.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;

				var nodes = new List<NodeInfo>();
				for (var i = 0; i < transform.childCount; i++)
				{
					var child = transform.GetChild(i);
					var guid = AssetDatabase.AssetPathToGUID(
						PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child.gameObject));
					if (!string.IsNullOrEmpty(guid))
					{
						nodes.Add(new(guid, child));
					}
				}

				var initLayerProp = serializedObject.FindProperty("initLayers");
				var initLayersCount = initLayerProp.arraySize;
				// foreach (var openLayerConfig in initLayers)
				for (var i = 0; i < initLayersCount; i++)
				{
					var openLayerConfigProp = initLayerProp.GetArrayElementAtIndex(i);
					var openLayerConfig = (OpenLayerConfig)openLayerConfigProp.boxedValue;
					var layerAssetGuid = openLayerConfig.layer.AssetGUID;
					var cur = nodes.FirstOrDefault(node => node.Guid == layerAssetGuid);
					if (cur == null)
					{
						if (!string.IsNullOrEmpty(layerAssetGuid))
						{
							var layerUrl = AssetDatabase.GUIDToAssetPath(layerAssetGuid);
							var layer = AssetDatabase.LoadAssetAtPath<GameObject>(layerUrl);
							if (layer != null)
							{
								var copyLayer = PrefabUtility.InstantiatePrefab(layer, comp.transform);
								copyLayer.hideFlags |= HideFlags.DontSave;
							}
						}
					}
					else
					{
						nodes.Remove(cur);
					}
				}

				foreach (var nodeInfo in nodes)
				{
					// PrefabUtility.RevertAddedGameObject(nodeInfo.Obj.gameObject, InteractionMode.AutomatedAction	);
					GameObject.DestroyImmediate(nodeInfo.Obj.gameObject);
				}
			}
			else
			{
				var comp = (LayerBundleComp)this.target;
				var transform = comp.transform;
				transform.gameObject.hideFlags = HideFlags.None;

				for (var i = transform.childCount - 1; i >= 0; i--)
				{
					var child = transform.GetChild(i);
					GameObject.DestroyImmediate(child.gameObject);
				}
			}
		}
	}
}