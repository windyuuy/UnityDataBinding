﻿using System;
using System.Collections;
using DataBind.UIBind;
using DataBind.UIBind.RecycleContainer;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EaseScrollView.Test
{
	public class RecycleContainerAsyncCtrlTest1:RecycleContainerCtrl
	{
		public GameObject templateNode;

		protected Transform Root;
		protected override void InitContainer(ContainerBindComp containerBindComp)
		{
			Root = new GameObject("RecycleRoot").transform;
			Root.gameObject.SetActive(false);
			
			base.InitContainer(containerBindComp);
		}

		protected override Transform GetTemplateNode(int index)
		{
			return null;
		}

		private int ixz = 0;
		private int e2 = 0;
		public override Transform CreateNewNodeAsync(int index, Action<Transform, Transform, Action<Transform>> onCreatedAsync, Func<int, string, Transform> createStandSync, Action<Transform> recycleStandAsync)
		{
			var stand = createStandSync(index,$"stand{index}");
			IEnumerator DelayCreate()
			{
				// Debug.Log($"createasync: {ixz}");
				// yield return new WaitForSeconds(0.5f+0.01f*ixz++);
				// yield return null;
				yield return new WaitForSeconds(3f);
				var child = GameObject.Instantiate(templateNode, Root).transform;
				child.gameObject.name = templateNode.gameObject.name+$"_{e2++}";
				onCreatedAsync(child, stand, recycleStandAsync);
			}

			StartCoroutine(DelayCreate());
			return stand;
		}
	}
}