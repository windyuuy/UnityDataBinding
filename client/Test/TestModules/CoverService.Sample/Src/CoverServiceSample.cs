using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoverService;
using CoverService.Runtime;
using Sample;
using SampleSolutionMaintainer.Config;
using SampleSolutionMaintainer.Builder;
using SampleSolutionMaintainer.Launcher;
using SampleSolutionMaintainer.Solution;
using UnityEngine;

namespace SampleSolutionMaintainer.Config
{
	public interface IWithMainConfig
	{
		public void Init(MainServiceConfig config);
	}

	/// <summary>
	/// services map
	/// </summary>
	public class MainServiceConfig : ServiceSet
	{
		private static readonly MainServiceConfig Inst = new();

		public static MainServiceConfig Create()
		{
			return Inst;
		}

		public readonly List<ServiceSolution> ServiceSolutions = new();
	}

	public static class UIServiceConfig
	{
		public static IWithAccessorGetter Inst { get; internal set; }

		public static IWithAccessorGetter Create()
		{
			Inst = MainServiceConfig.Create();
			return Inst;
		}
	}
}

namespace Sample
{
	public class CheckinService
		: ServiceTemplate<CheckinService, InternalAccessorSet, CheckinService.Accessor>
	{
		public class Accessor : InternalServiceAccessor
		{
			public class InternalIocContainer : AIocContainer
			{
				public override void Init()
				{
					this.Register<ICheckinSampleHandlerTestIoc>(() => new AsyncCheckinSampleHandler());
				}
			}

			protected override AIocContainer IocContainer { get; set; } = new InternalIocContainer();

			public Task<CheckinResp> ReqSampleAsync(CheckinReq req)
			{
				return Service.SendRequest(req, new AsyncCheckinSampleHandler());
			}

			public CheckinResp ReqSample(CheckinReq req)
			{
				return Service.SendRequest(req, new CheckinSampleHandler());
			}

			public Task<CheckinResp> ReqSampleAsync2(CheckinReq req)
			{
				return Service.SendRequest(req, _ => Task.FromResult(new CheckinResp()));
			}

			public CheckinResp ReqSample2(CheckinReq req)
			{
				return Service.SendRequest(req, _ => new CheckinResp());
			}

			public CheckinViewModel CreateCheckinViewModel() => new CheckinViewModel(this);
		}
	}

	public record InternalAccessorSet(CheckinService.Accessor OtherCheckinAccessor) : ServiceAccessorSet;

	public interface ICheckinSampleHandlerTestIoc
	{
	}

	[Serializable]
	public struct CheckinReq : IRequest, ICheckinSampleHandlerTestIoc
	{
	}

	[Serializable]
	public struct CheckinResp
	{
	}

	public class AsyncCheckinSampleHandler
		: CheckinService.AsyncRequestHandler<CheckinReq, CheckinResp>
	{
		protected override Task<CheckinResp> OnHandle(InternalAccessorSet accessorSet, CheckinReq req)
		{
			// return accessorSet.OtherCheckinAccessor.ReqSampleAsync(req);
			Debug.LogError("lwkjfe");
			return Task.FromResult(new CheckinResp());
		}
	}

	public class CheckinSampleHandler
		: CheckinService.RequestHandler<CheckinReq, CheckinResp>
	{
		protected override CheckinResp OnHandle(InternalAccessorSet accessorSet, CheckinReq req)
		{
			return accessorSet.OtherCheckinAccessor.ReqSample(req);
		}
	}

	public class CheckinViewModel : CheckinService.ViewModelBase
	{
		public CheckinResp Data;

		public override async Task Load()
		{
			Data = await Accessor.ReqSampleAsync(new());
		}

		public async Task CheckinToday()
		{
			await Accessor.ReqSampleAsync2(new());
		}

		public CheckinViewModel(CheckinService.Accessor accessor) : base(accessor)
		{
		}
	}

	public class CheckinUIController
	{
		protected CheckinService.Accessor CheckinAccessor;

		public void Init()
		{
			CheckinAccessor = UIServiceConfig.Inst.GetAccessor<CheckinService.Accessor>(this);
		}

		public void LoadCheckinViewModel()
		{
			ViewModel = CheckinAccessor.CreateCheckinViewModel();
			ObserveData(ViewModel);
		}

		private void ObserveData(CheckinViewModel viewModel)
		{
			throw new System.NotImplementedException();
		}

		public CheckinViewModel ViewModel { get; set; }

		public async Task OnClick()
		{
			await ViewModel.CheckinToday();
		}
	}
}

namespace SampleSolutionMaintainer.Solution
{
	public class SampleSolution : ServiceSolution, IWithMainConfig
	{
		public void Init(MainServiceConfig config)
		{
			var checkinService = config.Get<CheckinService>();
			checkinService.Init(new(
				config.GetAccessor<CheckinService.Accessor>(checkinService)
			));
		}
	}
}

namespace SampleSolutionMaintainer.Builder
{
	public class ServiceColonyBuilder
	{
		public MainServiceConfig BuildConfig()
		{
			var config = MainServiceConfig.Create();

			UIServiceConfig.Create();

			// register services
			config.RegisterInstance<CheckinService>();

			// register systems
			config.ServiceSolutions.Add(new SampleSolution());

			return config;
		}
	}
}

namespace SampleSolutionMaintainer.Launcher
{
	public class ServiceSolutionLauncher
	{
		public void Launch(MainServiceConfig config)
		{
			foreach (var serviceSolution in config.ServiceSolutions)
			{
				if (serviceSolution is IWithMainConfig withMainConfig)
				{
					withMainConfig.Init(config);
				}
			}
		}
	}
}

namespace ProjectLoader
{
	public class AppLoader
	{
		public void Load()
		{
			var builder = new ServiceColonyBuilder();
			var launcher = new ServiceSolutionLauncher();
			var mainConfig = builder.BuildConfig();
			launcher.Launch(mainConfig);
		}
	}
}