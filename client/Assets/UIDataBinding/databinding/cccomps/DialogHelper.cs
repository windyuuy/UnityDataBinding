using System;
using UnityEngine;

namespace UI.DataBinding
{
	public class DialogHelper
	{

		/**
		 * 获取当前组件所在节点的host
		 */
		public static CCDialogComp findDialogComp(Transform curNode)
		{
			var node = curNode;

			try
			{
				while (!node.GetComponent<CCDialogComp>())
				{
					node = node.parent;
				}
			}
			catch
			{

			}
			if (node == null)
			{
				return null;
			}

			var dialogComponent = node.GetComponent<CCDialogComp>();
			return dialogComponent;
		}

	}
}
