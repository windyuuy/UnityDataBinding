using UISys.Runtime;
using UnityEditor;
using UnityEngine;

namespace UISys.Editor
{
	[CanEditMultipleObjects, CustomEditor(typeof(TagOrdersConfig), true)]
	public class TagOrdersConfigEditor : UnityEditor.Editor
	{
		protected SerializedObject SerializedObjectCopy;

		private static readonly GUIContent MainOrdersLabel = new GUIContent("MainOrdersView");
		private static readonly GUIContent DependOrdersLabel = new GUIContent("DependOrdersView");
		public override void OnInspectorGUI()
		{
			base.DrawDefaultInspector();

			var asset = (TagOrdersConfig)target;
			var (mainOrders0, dependOrders0) = asset.LoadAll();

			SerializedObjectCopy ??= new SerializedObject(serializedObject.targetObject);

			GUI.enabled = false;
			{
				var mainOrders = SerializedObjectCopy.FindProperty("mainOrders");
				mainOrders.arraySize = mainOrders0.Length;
				for (var i = 0; i < mainOrders0.Length; i++)
				{
					var s = mainOrders0[i];
					mainOrders.GetArrayElementAtIndex(i).stringValue = s;
				}

				EditorGUILayout.PropertyField(mainOrders, MainOrdersLabel);
			}
			{
				var dependOrders = SerializedObjectCopy.FindProperty("dependOrders");
				dependOrders.arraySize = dependOrders0.Length;
				for (var i = 0; i < dependOrders0.Length; i++)
				{
					var s = dependOrders0[i];
					dependOrders.GetArrayElementAtIndex(i).stringValue = s;
				}

				EditorGUILayout.PropertyField(dependOrders, DependOrdersLabel);
			}
			GUI.enabled = true;
		}
	}
}