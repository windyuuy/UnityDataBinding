using NUnit.Framework;
using ProjectLoader;
using Sample;
using SampleSolutionMaintainer.Config;
using System.Threading.Tasks;

namespace Tests
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public async Task Test1()
		{
			AppLoader loader = new AppLoader();
			loader.Load();
			var checkin = UIServiceConfig.Inst.GetAccessor<CheckinService.Accessor>(this);
			await checkin.ReqSampleAsync2(new CheckinService.AReq());
			Assert.Pass();
		}
	}
}