using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EaseRecycle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestRecycle
{
	public string assetmanifestkey = "Assets/EaseRecycle/Test/TestRecycle.prefab";
	[UnityTest]
	public IEnumerator TestRecycle1()
	{
		// Use the Assert class to test conditions.
		// Use yield to skip a frame.
		var op = Addressables.LoadAssetAsync<GameObject>(assetmanifestkey);
		yield return op;
		var testAssets = op.Result.GetComponent<TestAssets>();

		var op2 = testAssets.RecycleRecursive.LoadAssetAsync<GameObject>();
		yield return op2;
		var asset = op2.Result;
		var recyclePool = new RecyclePool();

		recyclePool.LoadPrefab(asset);

		var image = recyclePool.Get("Image-bbeb2f9a174ba2e4dae9d493cc9e236e");
		Debug.Assert(image != null);
		var image2 = recyclePool.Get("Image-bbeb2f9a174ba2e4dae9d493cc9e236e");
		Debug.Assert(image.GetComponent<RecycleMark>().Uid == image2.GetComponent<RecycleMark>().Uid);
		var image3 = recyclePool.Get("Image-bbeb2f9a174ba2e4dae9d493cc9e236e");
		Debug.Assert(image.GetComponent<RecycleMark>().Uid == image3.GetComponent<RecycleMark>().Uid);

		Addressables.Release(op2);
		Addressables.Release(op);
	}
	[UnityTest]
	public IEnumerator TestRecycle1_2()
	{
		var op = Addressables.LoadAssetAsync<GameObject>(assetmanifestkey);
		yield return op;
		var testAssets = op.Result.GetComponent<TestAssets>();

		var op2 = testAssets.RecycleRecursive2.LoadAssetAsync<GameObject>();
		yield return op2;
		var asset = op2.Result;
		var recyclePool = new RecyclePool();

		recyclePool.LoadPrefab(asset);

		var image = recyclePool.Get("Image-bbeb2f9a174ba2e4dae9d493cc9e236e");
		Debug.Assert(image != null);
		var image2 = recyclePool.Get("Image-bbeb2f9a174ba2e4dae9d493cc9e236e");
		Debug.Assert(image.GetComponent<RecycleMark>().Uid == image2.GetComponent<RecycleMark>().Uid);
		var image3 = recyclePool.Get("Image-bbeb2f9a174ba2e4dae9d493cc9e236e");
		Debug.Assert(image.GetComponent<RecycleMark>().Uid == image3.GetComponent<RecycleMark>().Uid);

		Addressables.Release(op2);
		Addressables.Release(op);
	}
	[UnityTest]
	public IEnumerator TestRecycle1_3()
	{
		var op = Addressables.LoadAssetAsync<GameObject>(assetmanifestkey);
		yield return op;
		var testAssets = op.Result.GetComponent<TestAssets>();

		var op2 = testAssets.Single1.LoadAssetAsync<GameObject>();
		yield return op2;
		var asset = op2.Result;
		var recyclePool = new RecyclePool();

		recyclePool.LoadPrefab(asset);

		var image = recyclePool.Get("Image-bbeb2f9a174ba2e4dae9d493cc9e236e");
		Debug.Assert(image != null);
		var image2 = recyclePool.Get("Image-bbeb2f9a174ba2e4dae9d493cc9e236e");
		Debug.Assert(image.GetComponent<RecycleMark>().Uid == image2.GetComponent<RecycleMark>().Uid);
		var image3 = recyclePool.Get("Image-bbeb2f9a174ba2e4dae9d493cc9e236e");
		Debug.Assert(image.GetComponent<RecycleMark>().Uid == image3.GetComponent<RecycleMark>().Uid);

		Addressables.Release(op2);
		Addressables.Release(op);
	}
	[UnityTest]
	public IEnumerator TestRecycle1_4()
	{
		var op = Addressables.LoadAssetAsync<GameObject>(assetmanifestkey);
		yield return op;
		var testAssets = op.Result.GetComponent<TestAssets>();

		var op2 = testAssets.Single1.LoadAssetAsync<GameObject>();
		yield return op2;
		var asset = op2.Result;
		var recyclePool = new RecyclePool();

		recyclePool.LoadPrefab(asset,"uid1_4");

		var image = recyclePool.Get("uid1_4");
		Debug.Assert(image != null);
		var image2 = recyclePool.Get("uid1_4");
		Debug.Assert(image.GetComponent<RecycleMark>().Uid == image2.GetComponent<RecycleMark>().Uid);
		var image3 = recyclePool.Get("uid1_4");
		Debug.Assert(image.GetComponent<RecycleMark>().Uid == image3.GetComponent<RecycleMark>().Uid);

		Addressables.Release(op2);
		Addressables.Release(op);
	}
	
	[UnityTest]
	public IEnumerator TestRecycle2()
	{
		var op = Addressables.LoadAssetAsync<GameObject>(assetmanifestkey);
		yield return op;
		var testAssets = op.Result.GetComponent<TestAssets>();

		var op2 = testAssets.RecycleSingle.LoadAssetAsync<GameObject>();
		yield return op2;
		var asset = op2.Result;
		var recyclePool = new RecyclePool();

		recyclePool.LoadPrefab(asset);

		var image = recyclePool.Get("RecycleSingle-1a1365e071cc9e3468bfeae4849f9ed7");
		Debug.Assert(image != null);
		var image2 = recyclePool.Get("RecycleSingle-1a1365e071cc9e3468bfeae4849f9ed7");
		Debug.Assert(image.GetComponent<RecycleMark>().Uid == image2.GetComponent<RecycleMark>().Uid);
		var image3 = recyclePool.Get("RecycleSingle-1a1365e071cc9e3468bfeae4849f9ed7");
		Debug.Assert(image.GetComponent<RecycleMark>().Uid == image3.GetComponent<RecycleMark>().Uid);

		Addressables.Release(op2);
		Addressables.Release(op);
	}
	
	[UnityTest]
	public IEnumerator TestRecycle3()
	{
		var op = Addressables.LoadAssetAsync<GameObject>(assetmanifestkey);
		yield return op;
		var testAssets = op.Result.GetComponent<TestAssets>();

		var op2 = testAssets.UnrecycleSingle.LoadAssetAsync<GameObject>();
		yield return op2;
		var asset = op2.Result;
		var recyclePool = new RecyclePool();

		recyclePool.LoadPrefab(asset);
		Debug.Assert(recyclePool.NodePools.Count == 0);

		Addressables.Release(op2);
		Addressables.Release(op);
	}
	
	[UnityTest]
	public IEnumerator TestRecycle3_2()
	{
		var op = Addressables.LoadAssetAsync<GameObject>(assetmanifestkey);
		yield return op;
		var testAssets = op.Result.GetComponent<TestAssets>();

		var op2 = testAssets.UnrecycleSingle.LoadAssetAsync<GameObject>();
		yield return op2;
		var asset = op2.Result;
		var recyclePool = new RecyclePool();

		recyclePool.LoadPrefab(asset,"uid3_2");

		var image = recyclePool.Get("uid3_2");
		Debug.Assert(image != null);
		var image2 = recyclePool.Get("uid3_2");
		Debug.Assert(image2 != null);
		var image3 = recyclePool.Get("uid3_2");
		Debug.Assert(image3 != null);

		Addressables.Release(op2);
		Addressables.Release(op);
	}

	[UnityTest]
	public IEnumerator TestRecycle4()
	{
		var op = Addressables.LoadAssetAsync<GameObject>(assetmanifestkey);
		yield return op;
		var testAssets = op.Result.GetComponent<TestAssets>();

		var op2 = testAssets.UnusedSingle.LoadAssetAsync<GameObject>();
		yield return op2;
		var asset = op2.Result;
		var recyclePool = new RecyclePool();

		recyclePool.LoadPrefab(asset);
		Debug.Assert(recyclePool.NodePools.Count == 0);

		Addressables.Release(op2);
		Addressables.Release(op);
	}
}
