
// using System;
// using System.Threading.Tasks;
// using System.Collections.Generic;
// using System.Linq;

// namespace fsync.async
// {
// 	using Task = System.Threading.Tasks.Task;
// 	using console = UnityEngine.Debug;
// 	using Interlocked = System.Threading.Interlocked;

// 	public class PromiseBridge<T, T2>
// 	{
// 		public Action<T> onfulfilled;
// 		public Action<Exception> onrejected;

// 		public PromiseBridge(Action<T> onfulfilled, Action<Exception> onrejected)
// 		{
// 			this.onfulfilled = onfulfilled;
// 			this.onrejected = onrejected;
// 		}
// 	}

// 	public class Promise<T>
// 	{
// 		public Task<T> Task
// 		{
// 			get
// 			{
// 				return this.taskSource.Task;
// 			}
// 		}

// 		public bool isPending
// 		{
// 			get
// 			{
// 				return !this.taskSource.Task.IsCompleted;
// 			}
// 		}

// 		public T Result
// 		{
// 			get
// 			{
// 				return this.Task.Result;
// 			}
// 		}

// 		public static Promise<T2[]> All<T2>(Promise<T2>[] promises)
// 		{
// 			var task = new Promise<T2[]>((resolve, reject) =>
// 			{
// 				var isComplete = false;
// 				var pendingCount = promises.Length;
// 				var results = new T2[promises.Length];
// 				for (int i = 0; i < promises.Length; i++)
// 				{
// 					var promise = promises[i];
// 					promise.Then<T2>((data) =>
// 					{
// 						if (isComplete)
// 						{
// 							return data;
// 						}

// 						results[i] = data;
// 						var count = Interlocked.Decrement(ref pendingCount);
// 						if (count == 0)
// 						{
// 							isComplete = true;
// 							resolve(results);
// 						}
// 						return data;
// 					}, (err) =>
// 					{
// 						isComplete = true;
// 						reject(err);
// 						return (T2)(object)null;
// 					});
// 				}
// 				resolve(results);
// 			});
// 			return task;
// 		}

// 		public static Promise<T2> Resolve<T2>(T2 data)
// 		{

// 		}

// 		public static Promise<Nullable<T2>> Race<T2>(Promise<T2>[] promises)
// 		{
// 			if (promises.Length == 0)
// 			{
// 				return Resolve<Nullable<T2>>(null);
// 			}

// 			var task = new Promise<Nullable<T2>>((resolve, reject) =>
// 			{

// 				foreach (var item in promises)
// 				{
// 					item.Then((data) =>
// 					{
// 						resolve(data);
// 					}, (err) =>
// 					{

// 					});
// 				}
// 			});

// 			return task;
// 		}

// 		protected TaskCompletionSource<T> taskSource;
// 		public Promise(Action<Action<T>, Action<Exception>> handler)
// 		{
// 			this.taskSource = new TaskCompletionSource<T>();
// 			if (handler != null)
// 			{
// 				handler((T result) =>
// 				{
// 					this.handleResult(result);
// 				}, (Exception err) =>
// 				{
// 					this.handleException(err);
// 				});
// 			}
// 		}

// 		protected void handleResult(T result)
// 		{
// 			try
// 			{
// 				taskSource.SetResult(result);
// 			}
// 			catch (Exception e)
// 			{
// 				console.LogError("uncaught exception:");
// 				console.LogError(e);
// 				throw e;
// 			}
// 			finally
// 			{
// 				taskSource.TrySetCanceled();
// 			}

// 			try
// 			{
// 				while (this.pendingCalls.Count > 0)
// 				{
// 					var copy = this.pendingCalls.ToArray();
// 					this.pendingCalls.Clear();

// 					foreach (var item in copy)
// 					{
// 						item.onfulfilled(result);
// 					}
// 				}
// 			}
// 			catch (Exception e)
// 			{
// 				console.LogError(e);
// 			}
// 		}

// 		protected void handleException(Exception err)
// 		{
// 			try
// 			{
// 				taskSource.SetException(err);
// 			}
// 			catch (Exception e)
// 			{
// 				console.LogError("uncaught exception:");
// 				console.LogError(e);
// 				throw e;
// 			}
// 			finally
// 			{
// 				taskSource.TrySetCanceled();
// 			}

// 			try
// 			{
// 				while (this.pendingCalls.Count > 0)
// 				{
// 					var copy = this.pendingCalls.ToArray();
// 					this.pendingCalls.Clear();

// 					foreach (var item in copy)
// 					{
// 						item.onrejected(err);
// 					}
// 				}
// 			}
// 			catch (Exception e)
// 			{
// 				console.LogError(e);
// 			}
// 		}

// 		protected List<PromiseBridge<T, object>> pendingCalls = new List<PromiseBridge<T, object>>();

// 		public Promise<T2> Then<T2>(Func<T, T2> onfulfilled = null, Func<Exception, T2> onrejected = null)
// 		{
// 			var promise = new Promise<T2>(null);
// 			var bridge = new PromiseBridge<T, T2>((T input) =>
// 			{
// 				if (onrejected != null)
// 				{
// 					var isHandled = false;
// 					try
// 					{
// 						var output = onfulfilled(input);
// 						isHandled = true;
// 						promise.handleResult(output);
// 					}
// 					catch (Exception e)
// 					{
// 						if (!isHandled)
// 						{
// 							promise.handleException(e);
// 						}
// 					}
// 				}
// 				else
// 				{
// 					var output = (T2)Convert.ChangeType(input, typeof(T2));
// 					promise.handleResult(output);
// 				}
// 			}, (err) =>
// 			{
// 				if (onrejected != null)
// 				{
// 					var isHandled = false;
// 					try
// 					{
// 						var output = onrejected(err);
// 						isHandled = true;
// 						promise.handleResult(output);
// 					}
// 					catch (Exception e)
// 					{
// 						if (!isHandled)
// 						{
// 							promise.handleException(e);
// 						}
// 					}
// 				}
// 				else
// 				{
// 					promise.handleResult((T2)(object)null);
// 				}
// 			});
// 			this.pendingCalls.Add(bridge as PromiseBridge<T, object>);
// 			return promise;
// 		}

// 		public Promise<T2> Catch<T2>(Func<Exception, T2> onrejected = null)
// 		{
// 			var promise = new Promise<T2>(null);
// 			var bridge = new PromiseBridge<T, T2>((T input) =>
// 			{
// 				var output = (T2)Convert.ChangeType(input, typeof(T2));
// 				promise.handleResult(output);
// 			}, (err) =>
// 			{
// 				if (onrejected != null)
// 				{
// 					var isHandled = false;
// 					try
// 					{
// 						var output = onrejected(err);
// 						isHandled = true;
// 						promise.handleResult(output);
// 					}
// 					catch (Exception e)
// 					{
// 						if (!isHandled)
// 						{
// 							promise.handleException(e);
// 						}
// 					}
// 				}
// 				else
// 				{
// 					promise.handleResult((T2)(object)null);
// 				}
// 			});
// 			this.pendingCalls.Add(bridge as PromiseBridge<T, object>);
// 			return promise;
// 		}
// 	}

// }
