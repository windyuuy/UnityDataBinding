using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoverService;
using Sample;
using SampleSolutionMaintainer.Config;
using SampleSolutionMaintainer.Builder;
using SampleSolutionMaintainer.Launcher;

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
		: ServiceTemplate<CheckinService, CheckinService.InternalAccessorSet, CheckinService.Accessor>
	{
		public record InternalAccessorSet(Accessor OtherCheckinAccessor) : ServiceAccessorSet;

		public struct AReq
		{
		}

		public struct BResp
		{
		}

		public class AsyncCheckinSampleHandler
			: InternalAsyncRequestHandler<AReq, BResp>
		{
			protected override Task<BResp> OnHandle(InternalAccessorSet accessorSet, AReq req)
			{
				return accessorSet.OtherCheckinAccessor.ReqSampleAsync(req);
			}
		}

		public class CheckinSampleHandler
			: InternalRequestHandler<AReq, BResp>
		{
			protected override BResp OnHandle(InternalAccessorSet accessorSet, AReq req)
			{
				return accessorSet.OtherCheckinAccessor.ReqSample(req);
			}
		}

		// [IStdHost]
		public class CheckinViewModel : ViewModelBase
		{
			public BResp Data;

			public override async Task Load()
			{
				Data = await Accessor.ReqSampleAsync(new());
			}

			public async Task CheckinToday()
			{
				await Accessor.ReqSampleAsync2(new());
			}

			public CheckinViewModel(Accessor accessor) : base(accessor)
			{
			}
		}

		public class Accessor : ServiceAccessor<CheckinService>
		{
			public Task<BResp> ReqSampleAsync(AReq req)
			{
				return Service.SendRequest(req, new AsyncCheckinSampleHandler());
			}

			public BResp ReqSample(AReq req)
			{
				return Service.SendRequest(req, new CheckinSampleHandler());
			}

			public Task<BResp> ReqSampleAsync2(AReq req)
			{
				return Service.SendRequest(req, _ => Task.FromResult(new BResp()));
			}

			public BResp ReqSample2(AReq req)
			{
				return Service.SendRequest(req, _ => new BResp());
			}

			public CheckinViewModel CreateCheckinViewModel() => new CheckinViewModel(this);
		}
	}

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

		private void ObserveData(CheckinService.CheckinViewModel viewModel)
		{
			throw new System.NotImplementedException();
		}

		public CheckinService.CheckinViewModel ViewModel { get; set; }

		public async Task OnClick()
		{
			await ViewModel.CheckinToday();
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
		public void Launcher(MainServiceConfig config)
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
			launcher.Launcher(mainConfig);
		}
	}
}