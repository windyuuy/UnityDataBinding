using System;
using System.Collections;
using TrackableResourceManager.Runtime;
using UISys.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TrackableResourceManager.Test
{
	public class TestResourceManager : MonoBehaviour
	{
		[SerializeField] protected AssetReferenceT<UILayerRootRefer> layerRootRefer1;
		[SerializeField] protected AssetReferenceT<UILayerRootRefer> layerRootRefer2;
		private async void Start()
		{
			// var wfe=await layerRootRefer1.LoadAssetAsync().Task;
			// var wfe2=await layerRootRefer2.LoadAssetAsync().Task;
			Debug.Log("TestResourceManager");
			{
				using var rr = ResourceScope.New;
			}

			StartCoroutine(DelayTest());
		}

		IEnumerator DelayTest()
		{
			yield return new WaitForEndOfFrame();
			{
				using var rr = ResourceScope.New;
			}
		}
	}
}