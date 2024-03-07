﻿using System;
using System.Linq;
using System.Reflection;
using UISys.Runtime;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UISys.Editor
{
	[CustomEditor(typeof(UIActionsComp))]
	public class UIActionCompRemovalDetector : UnityEditor.Editor
	{
		private GameObject _gameObject;
		private int buttonIndex = -1;
		private int eventTriggerIndex = -1;

		private void Awake()
		{
			var comp = (UIActionsComp)target;
			if (comp != null)
			{
				if (comp.IsReseted)
				{
					comp.IsReseted = false;
					this.HandleReset();
				}
			}
		}

		internal void HandleReset()
		{
			InitEditor((Component)this.target);
		}

		private void OnEnable()
		{
			_gameObject = ((Component)this.target).gameObject;
			UpdateIndex();
		}

		public void UpdateIndex()
		{
			var button = _gameObject.GetComponent<Button>();
			if (button != null)
			{
				buttonIndex = FindEventIndex(button.onClick, (Component)target);
			}

			var eventTrigger = _gameObject.GetComponent<EventTrigger>();
			if (eventTrigger != null)
			{
				var onClickTrigger =
					eventTrigger.triggers.FirstOrDefault(trigger => trigger.eventID == EventTriggerType.PointerClick);
				if (onClickTrigger != null)
				{
					eventTriggerIndex = FindEventIndex(onClickTrigger.callback, (Component)target);
				}
			}
		}

		public override void OnInspectorGUI()
		{
			try
			{
				base.OnInspectorGUI();
			}
			catch (NullReferenceException ex)
			{
				Debug.LogException(ex);

				// var actionArrayProp = serializedObject.FindProperty("actions");
				// for (var i = 0; i < actionArrayProp.arraySize; i++)
				// {
				// 	var actionProp=actionArrayProp.GetArrayElementAtIndex(i);
				// 	var selfProp = actionProp.FindPropertyRelative("self");
				// 	var selfObjProp = actionProp.FindPropertyRelative("selfObj");
				// 	if (
				// 		(selfProp.boxedValue == null || ((AssetReference)selfProp.boxedValue).IsValid() == false)
				// 		&& selfObjProp.objectReferenceValue == null
				// 	)
				// 	{
				// 		actionProp.boxedValue = new UIAction();
				// 	}
				// }
				//
				// serializedObject.ApplyModifiedProperties();
			}
			
			UpdateIndex();
		}

		void OnDisable()
		{
			try
			{
				if (target == null && _gameObject != null)
				{
					if (buttonIndex >= 0)
					{
						var button = _gameObject.GetComponent<Button>();
						if (button != null)
						{
							UnityEventTools.RemovePersistentListener(button.onClick, buttonIndex);
							buttonIndex = -1;
						}
					}

					if (eventTriggerIndex >= 0)
					{
						var eventTrigger = _gameObject.GetComponent<EventTrigger>();
						if (eventTrigger != null)
						{
							var onClickTrigger =
								eventTrigger.triggers.FirstOrDefault(
									trigger => trigger.eventID == EventTriggerType.PointerClick);
							if (onClickTrigger != null)
							{
								UnityEventTools.RemovePersistentListener(onClickTrigger.callback, eventTriggerIndex);
							}

							eventTriggerIndex = -1;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static int FindEventIndex(UnityEventBase evt, Component self)
		{
			var persistentEventCount = evt.GetPersistentEventCount();
			for (var i = 0; i < persistentEventCount; i++)
			{
				if (evt.GetPersistentTarget(i) == self)
				{
					return i;
				}
			}

			return -1;
		}

		public static void InitEditor(Component target)
		{
			var button = target.GetComponent<Button>();
			if (button != null)
			{
				var exist = FindEventIndex(button.onClick, (Component)target) >= 0;

				if (!exist)
				{
					var methodInfo = target.GetType().GetMethod("Run", Type.EmptyTypes)!;
					var methodDelegate =
						System.Delegate.CreateDelegate(typeof(UnityAction), target,
							methodInfo) as UnityAction;
					UnityEventTools.AddPersistentListener(button.onClick, methodDelegate);
				}
			}

			var eventTrigger = target.GetComponent<EventTrigger>();
			if (eventTrigger != null)
			{
				var onClickTrigger =
					eventTrigger.triggers.FirstOrDefault(trigger => trigger.eventID == EventTriggerType.PointerClick);
				if (onClickTrigger != null)
				{
					var exist = FindEventIndex(onClickTrigger.callback, (Component)target) >= 0;

					if (!exist)
					{
						var methodInfo = target.GetType()
							.GetMethod("Run", BindingFlags.Instance | BindingFlags.Public)!;
						var methodDelegate =
							System.Delegate.CreateDelegate(typeof(UnityAction<BaseEventData>), target, methodInfo) as
								UnityAction<BaseEventData>;
						UnityEventTools.AddPersistentListener(onClickTrigger.callback, methodDelegate);
					}
				}
			}

			var toggle = target.GetComponent<Toggle>();
			if (toggle != null)
			{
				var exist = FindEventIndex(toggle.onValueChanged, (Component)target) >= 0;

				if (!exist)
				{
					var methodInfo = target.GetType().GetMethod("RunWithToggle", new Type[]{toggle.GetType()})!;
					var methodDelegate =
						System.Delegate.CreateDelegate(typeof(UnityAction<Toggle>), target,
							methodInfo) as UnityAction<Toggle>;
					UnityEventTools.AddObjectPersistentListener(toggle.onValueChanged, methodDelegate, toggle);
				}
			}
		}
	}
}