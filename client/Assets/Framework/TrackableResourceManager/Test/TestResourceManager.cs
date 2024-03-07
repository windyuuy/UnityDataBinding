using System;
using System.Collections;
using ResourceManager.Trackable.Runtime;
using UISys.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ResourceManager.Trackable.Test
{
	public class TestResourceManager : MonoBehaviour
	{
		[SerializeField] protected AssetReferenceT<UILayerRootRefer> layerRootRefer1;
		[SerializeField] protected AssetReferenceT<UILayerRootRefer> layerRootRefer2;
		private async void Start()
		{
			// var wfe=await layerRootRefer1.LoadAssetAsync().Task;
			// var wfe2=await layerRootRefer2.LoadAssetAsync().Task;
			Debug.Log("lkwjef");
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