using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityVisualize.Runtime;

namespace Framework.UISys.GameUILayer.Runtime
{
	[ExecuteInEditMode]
	public class UIPages : ToggleGroup
	{
		public IEnumerable<Toggle> GetToggles()
		{
			return m_Toggles;
		}

#if UNITY_EDITOR
		[NonSerialized] public bool EnableMirrorEditor = true;

		[NonSerialized] public bool IsPreviewedOnce = false;

		private void Update()
		{
			var toggles = this.GetToggles().ToArray();
			if (toggles.Length > 0)
			{
				foreach (var activeToggle in toggles)
				{
					var actionsComps = activeToggle.GetComponents<EaseActionsComp>();
					var ac1 = actionsComps[0];
					var ac2 = actionsComps[1];
					var j = 0;
					for (var i = 0; i < ac1.Actions.Length; i++)
					{
						var uiAction = ac1.Actions[i];
						if (uiAction.action == "OpenLayer" && uiAction.paras.Length >= 1)
						{
							for (; j < ac2.Actions.Length; j++)
							{
								var uiAction2 = ac2.Actions[j];
								if (uiAction.paras.Length >= 1 && uiAction2.paras.Length >= 1
								                               && ((uiAction.self != null &&
								                                    uiAction.self == uiAction2.self) ||
								                                   (uiAction.selfObj != null &&
								                                    uiAction.selfObj == uiAction2.selfObj))
								                               && uiAction2.action == "CloseLayer")
								{
									uiAction2.paras[0] = uiAction.paras[0];
								}
								else if (uiAction.paras.Length >= 1 && uiAction2.paras.Length >= 1
								                                    && (uiAction2.paras[0] == uiAction.paras[0])
								                                    && uiAction2.action == "CloseLayer")
								{
									uiAction2.self = uiAction.self;
									uiAction2.selfObj = uiAction.selfObj;
								}
							}
						}
					}
				}
			}
		}
#endif
	}
}