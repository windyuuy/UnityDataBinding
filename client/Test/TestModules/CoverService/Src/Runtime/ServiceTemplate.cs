using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoverService.Utils;
using UnityEngine;

namespace CoverService
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

	internal interface IRequestHandler
	{
	}

	/// <summary>
	/// RequestHandler
	/// </summary>
	/// <typeparam name="S"></typeparam>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="R">返回值，如果是void则改用bool</typeparam>
	public abstract class RequestHandler<S, T, R> : IRequestHandler
		where T : struct where R : struct where S : IServiceAccessorSet
	{
		internal R Handle(S accessors, T req)
		{
			return OnHandle(accessors, req);
		}

		protected abstract R OnHandle(S accessor, T req);
	}

	internal interface IAsyncRequestHandler
	{
	}

	/// <summary>
	/// RequestHandler
	/// </summary>
	/// <typeparam name="S"></typeparam>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="R">返回值，如果是void则改用bool</typeparam>
	public abstract class AsyncRequestHandler<S, T, R> : IAsyncRequestHandler
		where T : struct where R : struct where S : IServiceAccessorSet
	{
		internal Task<R> Handle(S accessors, T req)
		{
			return OnHandle(accessors, req);
		}

		protected abstract Task<R> OnHandle(S accessors, T req);
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
		Task<R> SendRequest<T, R>(T req, AsyncRequestHandler<Ss, T, R> handler)
			where T : struct where R : struct;

		/// <summary>
		/// default sender
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="req"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <returns></returns>
		R SendRequest<T, R>(T req, RequestHandler<Ss, T, R> handler)
			where T : struct where R : struct;

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

	public class ServiceAccessor<St> : IServiceAccessor where St : IServiceTemplate
	{
		protected St Service;

		internal void Init(St service, object caller)
		{
			Service = service;
			Caller = caller;
		}

		public object Caller { get; protected set; }
	}

	public abstract class ServiceTemplate<St, Ss, Sa> : IServiceTemplate<Ss>, IServiceTemplateWithDefaultAccessor<Sa>
		where Ss : ServiceAccessorSet where Sa : ServiceAccessor<St>, new() where St : ServiceTemplate<St, Ss, Sa>
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

		public interface IViewModel
		{
			public Task Load();
		}

		public abstract class ViewModelBase : IViewModel
		{
			protected Sa Accessor;

			public ViewModelBase(Sa accessor)
			{
				Accessor = accessor;
			}

			public abstract Task Load();
		}

		public abstract class InternalAsyncRequestHandler<T, R> : AsyncRequestHandler<Ss, T, R>
			where T : struct where R : struct
		{
		}

		public abstract class InternalRequestHandler<T, R> : RequestHandler<Ss, T, R>
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
		public virtual async Task<R> SendRequest<T, R>(T req, AsyncRequestHandler<Ss, T, R> handler)
			where T : struct where R : struct
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
		public virtual R SendRequest<T, R>(T req, RequestHandler<Ss, T, R> handler)
			where T : struct where R : struct
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
			if(!_mapper.TryGetValue(typeof(T), out _))
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
			if(!_mapper.TryGetValue(typeof(T), out _))
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