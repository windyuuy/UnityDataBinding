using System;
using System.Collections.Generic;

namespace DataBinding.UIBind
{
	using TExprCall = EventHandlerMV2<object, object>;
	using TWatchExprCall = EventHandlerMV2<string, EventHandlerMV2<object, object>>;

	public interface IDataBindHubTree
	{
		void AddBindHub(IDataBindHub bindHub);
		void RemoveBindHub(IDataBindHub bindHub);
		/**
		 * 立即同步表达式的值
		 * @param expr
		 * @param call
		 * @returns
		 */
		void SyncExprValue(string expr, TExprCall call);
	}

	public interface IDataBindHub : IDataBindHubTree
	{
		List<IDataBindHubTree> Parents { get; set; }
		IDataBindHubTree Parent { get; set; }

		/**
		 * 监听未监听过的接口
		 * @param expr 
		 */
		TWatchExprCall OnWatchNewExpr(TWatchExprCall call);
		void OffWatchNewExpr(TWatchExprCall call);
		/**
		 * 不再监听某个接口
		 * @param expr 
		 */
		TWatchExprCall OnUnWatchExpr(TWatchExprCall call);
		void OffUnWatchExpr(TWatchExprCall call);

		EventHandlerMV3<string, object, object> OnAnyValueChanged(EventHandlerMV3<string, object, object> call);
		void OffAnyValueChanged(EventHandlerMV3<string, object, object> call);

		ISEventCleanInfo2<object, object> WatchExprValue(string expr, EventHandlerMV2<object, object> call);
		void UnWatchExprValue(string expr, EventHandlerMV2<object, object> call);
	}
}
