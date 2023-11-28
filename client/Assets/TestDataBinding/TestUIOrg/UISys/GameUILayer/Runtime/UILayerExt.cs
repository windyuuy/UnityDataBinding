using gcc.layer;

namespace UI.UISys.GameUILayer.Runtime
{
	public static class UILayerExt
	{
		/// <summary>
		/// 获取在顶部的全屏界面
		/// </summary>
		/// <param name="layerManager"></param>
		/// <returns></returns>
		public static string GetTopUICover(this LayerMG layerManager)
		{
			LayerModel coverModel = null;
			layerManager.ForEachLayers(layerModel =>
			{
				if (layerModel.IsOpen && !layerModel.BeCovered && layerModel.Tags.Contains("UICover"))
				{
					coverModel = layerModel;
					return true;
				}

				return false;
			});

			if (coverModel != null)
			{
				return coverModel.Uri;
			}

			return null;
		}

		/// <summary>
		/// 当前是否处于首页
		/// </summary>
		/// <param name="layerManager"></param>
		/// <returns></returns>
		public static bool IsInHomePage(this LayerMG layerManager)
		{
			var uri = GetTopUICover(layerManager);
			return uri == "Chapter/ChapterView";
		}
	}
}