using System.Threading.Tasks;

namespace UnityEngine.AddressableAssets.MyExt
{
	public static class AddressablesHelper
	{
		public static async Task<Object> LoadAssetFast(this AssetReference uiActionSelf)
		{
			return uiActionSelf.Asset != null && uiActionSelf.IsValid()
				? uiActionSelf.Asset
				: await uiActionSelf.LoadAssetAsync<Object>().Task;
		}

		public static async Task<T> LoadAssetFast<T>(this AssetReference uiActionSelf) where T : Object
		{
			return uiActionSelf.Asset != null && uiActionSelf.IsValid()
				? (T)(uiActionSelf.Asset)
				: await uiActionSelf.LoadAssetAsync<T>().Task;
		}

		public static async Task<T> LoadAssetFast<T>(this AssetReferenceT<T> uiActionSelf) where T : Object
		{
			return uiActionSelf.Asset != null && uiActionSelf.IsValid()
				? (T)(uiActionSelf.Asset)
				: await uiActionSelf.LoadAssetAsync<T>().Task;
		}

		/// <summary>
		/// delay release to improve performance
		/// </summary>
		/// <param name="uiActionSelf"></param>
		public static async ValueTask DelayRelease(this AssetReference uiActionSelf)
		{
			if (uiActionSelf != null && uiActionSelf.IsValid())
			{
				await AsyncUtils.WaitForEndOfFrame();
				uiActionSelf.ReleaseAsset();
			}
		}
	}
}