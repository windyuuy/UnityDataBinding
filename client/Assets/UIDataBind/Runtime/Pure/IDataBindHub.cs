using System;
using System.Collections.Generic;

namespace DataBind.UIBind
{
	using TExprCall = EventHandlerMV2<object, object>;
	using TWatchExprCall = EventHandlerMV2<string, EventHandlerMV2<object, object>>;

	public interface IRawObjObservable
	{
		public object RawObj { get;}
		// public string DebugName { get; }
	}
	public interface IDataSourcePump: IRawObjObservable
	{
		public DataBindHub BindHub { get;}
		public IEnumerable<T> PeekSourceBindEvents<T>();
	}
	public interface IDataBindPump: IRawObjObservable
	{
		public DataBindHub BindHub { get;}
	}

	public interface IDataSourceDispatcher: IDataBindPump
	{
		public IEnumerable<DataSourceHub> GetDataSourceHubs();
	}
	public interface IDataBindRepeater: IRawObjObservable, IDataBindHubSubTree
	{
		public List<DataBindHub> BindHubs { get; }
		public IEnumerable<(string key, IRawObjObservable target)> PeekBindEvents();
		public IEnumerable<IDataBindPump> GetBindPumps();
	}
	
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

	public interface IDataBindHubSubTree
	{
		public List<IDataBindHubTree> Parents { get; }
	}

	public interface IDataBindHub : IDataBindHubTree, IDataBindHubSubTree, IDataBindRepeater
	{
		// List<IDataBindHubTree> Parents { get;}
		// IDataBindHubTree Parent { get; set; }

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
