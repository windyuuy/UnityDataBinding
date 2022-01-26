using System;
using System.Collections.Generic;

namespace UI.DataBinding
{
	using TExprCall = EventHandlerMV2<object, object>;
	using TWatchExprCall = EventHandlerMV2<string, EventHandlerMV2<object, object>>;

	public interface IDataBindHubTree
	{
		void addBindHub(IDataBindHub bindHub);
		void removeBindHub(IDataBindHub bindHub);
		/**
		 * 立即同步表达式的值
		 * @param expr
		 * @param call
		 * @returns
		 */
		void syncExprValue(string expr, TExprCall call);
	}

	public interface IDataBindHub : IDataBindHubTree
	{
		List<IDataBindHubTree> parents { get; set; }
		IDataBindHubTree parent { get; set; }

		/**
		 * 监听未监听过的接口
		 * @param expr 
		 */
		TWatchExprCall onWatchNewExpr(TWatchExprCall call);
		void offWatchNewExpr(TWatchExprCall call);
		/**
		 * 不再监听某个接口
		 * @param expr 
		 */
		TWatchExprCall onUnWatchExpr(TWatchExprCall call);
		void offUnWatchExpr(TWatchExprCall call);

		EventHandlerMV3<string, object, object> onAnyValueChanged(EventHandlerMV3<string, object, object> call);
		void offAnyValueChanged(EventHandlerMV3<string, object, object> call);

		ISEventCleanInfo2<object, object> watchExprValue(string expr, EventHandlerMV2<object, object> call);
		void unWatchExprValue(string expr, EventHandlerMV2<object, object> call);
	}
}
