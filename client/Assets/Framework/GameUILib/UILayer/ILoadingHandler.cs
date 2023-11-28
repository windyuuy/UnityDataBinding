
namespace gcc.layer
{

	public interface IUILoading
	{

	}

	/**
	 * 触发loading事件处理器
	 */
	public interface ILoadingHandler
	{
		void onShowLoading(string key);
		void onHideLoading(string key);
		void onCloseLoading(string key);
	}

}
