using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoverService.Runtime.Utils;
using DataBinding;
using UnityEngine;

namespace CoverService.Runtime
{
	public interface IWithCallerNameGetter
	{
		public string GetCallerName();
	}

	public interface IServiceAccessor
	{
		public object Caller { get; }
	}

	public interface IServiceAccessorSet
	{
	}

	public record ServiceAccessorSet : IServiceAccessorSet
	{
	}

	public interface IRequestHandler
	{
	}

	public interface IRequestHandler<Ss, R> : IRequestHandler
		where Ss : IServiceAccessorSet
	{
		public R Handle(Ss accessors, object req);
	}

	/// <summary>
	/// RequestHandler
	/// </summary>
	/// <typeparam name="Ss"></typeparam>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="R">返回值，如果是void则改用bool</typeparam>
	public abstract class InternalRequestHandler<Ss, T, R> : IRequestHandler<Ss, R>
		where T : struct where R : struct where Ss : IServiceAccessorSet
	{
		public R Handle(Ss accessors, object req)
		{
			return Handle(accessors, (T)req);
		}

		internal R Handle(Ss accessors, T req)
		{
			return OnHandle(accessors, req);
		}

		protected abstract R OnHandle(Ss accessor, T req);
	}

	public interface IAsyncRequestHandler<Ss, R> : IRequestHandler<Ss, Task<R>>
		where R : struct where Ss : IServiceAccessorSet
	{
		public Task<R> Handle(Ss accessors, object req);
	}

	/// <summary>
	/// RequestHandler
	/// </summary>
	/// <typeparam name="Ss"></typeparam>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="R">返回值，如果是void则改用bool</typeparam>
	public abstract class InternalAsyncRequestHandler<Ss, T, R> : IAsyncRequestHandler<Ss, R>
		where T : struct where R : struct where Ss : IServiceAccessorSet
	{
		public Task<R> Handle(Ss accessors, object req)
		{
			return Handle(accessors, (T)req);
		}

		internal Task<R> Handle(Ss accessors, T req)
		{
			return OnHandle(accessors, req);
		}

		protected abstract Task<R> OnHandle(Ss accessors, T req);
	}

	public interface IServiceTemplate
	{
		public string Name { get; }
	}

	public interface IServiceTemplate<Ss> : IServiceTemplate where Ss : IServiceAccessorSet
	{
		/// <summary>
		/// default sender
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="req"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <returns></returns>
		Task<R> SendRequest<R>(IRequest req, IAsyncRequestHandler<Ss, R> handler)
			where R : struct;

		/// <summary>
		/// default sender
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="req"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <returns></returns>
		R SendRequest<R>(IRequest req, IRequestHandler<Ss, R> handler)
			where R : struct;

		/// <summary>
		/// default sender
		/// </summary>
		/// <param name="req"></param>
		/// <param name="call"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <returns></returns>
		Task<R> SendRequest<T, R>(T req, Func<T, Task<R>> call)
			where T : struct where R : struct;

		/// <summary>
		/// default sender
		/// </summary>
		/// <param name="req"></param>
		/// <param name="call"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <returns></returns>
		R SendRequest<T, R>(T req, Func<T, R> call)
			where T : struct where R : struct;
	}

	public interface IServiceTemplateWithDefaultAccessor<Sa> : IServiceTemplate where Sa : IServiceAccessor
	{
		public Sa CreateAccessor(object caller);
		public Sa GetAccessor(object caller);
	}

	public abstract class AIocContainer
	{
		private Dictionary<Type, object> _mapper = new();

		internal IEnumerable<IAsyncRequestHandler<Ss, R>> GetAsync<Ss, R>(IRequest req)
			where R : struct where Ss : IServiceAccessorSet
		{
			foreach (var @interface in req.GetType().GetInterfaces())
			{
				if (_mapper.TryGetValue(@interface, out var value))
				{
					if (value is IAsyncRequestHandler<Ss, R> handler)
					{
						yield return handler;
					}
				}
			}
		}

		internal IEnumerable<IRequestHandler<Ss, R>> Get<Ss, R>(IRequest req)
			where R : struct where Ss : IServiceAccessorSet
		{
			foreach (var @interface in req.GetType().GetInterfaces())
			{
				if (_mapper.TryGetValue(@interface, out var value))
				{
					if (value is IRequestHandler<Ss, R> handler)
					{
						yield return handler;
					}
				}
			}
		}

		protected void Register<T>(Func<IRequestHandler> handlerGetter)
		{
			var handler = handlerGetter();
			_mapper.Add(typeof(T), handler);
		}

		public abstract void Init();
	}

	public interface IRequest
	{
	}

	public abstract class ServiceAccessor<St, Ss> : IServiceAccessor
		where St : IServiceTemplate<Ss> where Ss : ServiceAccessorSet
	{
		protected St Service;
		protected virtual AIocContainer IocContainer { get; set; }

		internal void Init(St service, object caller)
		{
			Service = service;
			Caller = caller;

			if (IocContainer != null)
			{
				IocContainer.Init();
			}
		}

		public object Caller { get; protected set; }

		/// <summary>
		/// default sender with ioc
		/// </summary>
		/// <param name="req"></param>
		/// <param name="handler"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <returns></returns>
		public virtual IEnumerable<Task<R>> BroadRequestAsync<R>(IRequest req)
			where R : struct
		{
			if (IocContainer == null)
			{
				yield break;
			}

			foreach (var asyncRequestHandler in IocContainer.GetAsync<Ss, R>(req))
			{
				yield return Service.SendRequest(req, asyncRequestHandler);
			}
		}

		/// <summary>
		/// default sender with ioc
		/// </summary>
		/// <param name="req"></param>
		/// <param name="handler"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <returns></returns>
		public virtual IEnumerable<R> BroadRequest<R>(IRequest req)
			where R : struct
		{
			if (IocContainer == null)
			{
				yield break;
			}

			foreach (var requestHandler in IocContainer.Get<Ss, R>(req))
			{
				yield return Service.SendRequest(req, requestHandler);
			}
		}
	}

	public interface IViewModel
	{
		public Task Load();
	}

	public abstract class ViewModelBase<T> : IViewModel, IStdHost
	{
		protected readonly T Accessor;

		protected ViewModelBase(T accessor)
		{
			Accessor = accessor;
		}

		public abstract Task Load();
	}

	public abstract class ServiceTemplate<St, Ss, Sa> : IServiceTemplate<Ss>, IServiceTemplateWithDefaultAccessor<Sa>
		where Ss : ServiceAccessorSet where Sa : ServiceAccessor<St, Ss>, new() where St : ServiceTemplate<St, Ss, Sa>
	{
		public string Name { get; } = typeof(St).FullName;

		public virtual void Init(Ss accessorSet)
		{
			Accessors = accessorSet;
		}

		/// <summary>
		/// default accessor set
		/// </summary>
		protected virtual Ss Accessors { get; set; }

		/// <summary>
		/// default accessor
		/// </summary>
		/// <returns></returns>
		public virtual Sa CreateAccessor(object caller)
		{
			var accessor = new Sa();
			accessor.Init((St)this, caller);
			return accessor;
		}

		public abstract class InternalServiceAccessor : ServiceAccessor<St, Ss>
		{
		}

		protected Dictionary<object, Sa> AccessorMap;

		public virtual Sa GetAccessor(object caller)
		{
			if (AccessorMap == null)
			{
				AccessorMap = new();
			}

			if (!AccessorMap.TryGetValue(caller, out var accessor))
			{
				accessor = CreateAccessor(caller);
				AccessorMap.Add(caller, accessor);
			}

			return accessor;
		}

		public abstract class ViewModelBase : ViewModelBase<Sa>
		{
			protected ViewModelBase(Sa accessor) : base(accessor)
			{
			}
		}

		public abstract class AsyncRequestHandler<T, R> : InternalAsyncRequestHandler<Ss, T, R>
			where T : struct where R : struct
		{
		}

		public abstract class RequestHandler<T, R> : InternalRequestHandler<Ss, T, R>
			where T : struct where R : struct
		{
		}

		/// <summary>
		/// default sender
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="req"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <returns></returns>
		public virtual async Task<R> SendRequest<R>(IRequest req, IAsyncRequestHandler<Ss, R> handler)
			where R : struct
		{
			return ServiceParaChecker.HandleRespPara(this.Name,
				await handler.Handle(Accessors, ServiceParaChecker.HandleReqPara(this.Name, req)));
		}

		/// <summary>
		/// default sender
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="req"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <returns></returns>
		public virtual R SendRequest<R>(IRequest req, IRequestHandler<Ss, R> handler)
			where R : struct
		{
			return ServiceParaChecker.HandleRespPara(this.Name,
				handler.Handle(Accessors, ServiceParaChecker.HandleReqPara(this.Name, req)));
		}

		/// <summary>
		/// default sender
		/// </summary>
		/// <param name="req"></param>
		/// <param name="call"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <returns></returns>
		public virtual async Task<R> SendRequest<T, R>(T req, Func<T, Task<R>> call)
			where T : struct where R : struct
		{
			return ServiceParaChecker.HandleRespPara(this.Name,
				await call(ServiceParaChecker.HandleReqPara(this.Name, req)));
		}

		/// <summary>
		/// default sender
		/// </summary>
		/// <param name="req"></param>
		/// <param name="call"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <returns></returns>
		public virtual R SendRequest<T, R>(T req, Func<T, R> call)
			where T : struct where R : struct
		{
			return ServiceParaChecker.HandleRespPara(this.Name, call(ServiceParaChecker.HandleReqPara(this.Name, req)));
		}
	}

	public interface IWithAccessorGetter
	{
		public Sa GetAccessor<Sa>(object callerServices) where Sa : class, IServiceAccessor, new();
	}

	public class ServiceSet : IWithAccessorGetter
	{
		private readonly Dictionary<Type, IServiceTemplate> _mapper = new();

		public void RegisterInstance<T>() where T : IServiceTemplate, new()
		{
			if (!_mapper.TryGetValue(typeof(T), out _))
			{
				_mapper.Add(typeof(T), new T());
			}
			else
			{
				Debug.LogError($"duplicate services: {typeof(T).FullName}");
			}
		}

		public void RegisterInstance<T>(T obj) where T : IServiceTemplate, new()
		{
			if (!_mapper.TryGetValue(typeof(T), out _))
			{
				_mapper.Add(typeof(T), obj);
			}
			else
			{
				Debug.LogError($"duplicate services: {typeof(T).FullName}");
			}
		}

		public T Get<T>() where T : class, IServiceTemplate, new()
		{
			if (_mapper.TryGetValue(typeof(T), out var service))
			{
				return (T)service;
			}

			return null;
		}

		public Sa GetAccessor<Sa>(object callerServices) where Sa : class, IServiceAccessor, new()
		{
			foreach (var keyValuePair in _mapper)
			{
				if (keyValuePair.Key.GetNestedType(typeof(Sa).Name) != null)
				{
					var service = keyValuePair.Value;
					if (service is IServiceTemplateWithDefaultAccessor<Sa> serviceTemplateWithDefaultAccessor)
					{
						return serviceTemplateWithDefaultAccessor.CreateAccessor(callerServices);
					}
				}
			}

			return null;
		}
	}

	public interface IServiceSolution
	{
	}

	public abstract class ServiceSolution : IServiceSolution
	{
	}
}