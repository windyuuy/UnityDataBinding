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
			var aReq = new CheckinReq();
			var ret = await checkin.ReqSampleAsync2(aReq);
			var ret2 = await checkin.BroadRequestAsync<CheckinResp>(aReq).WhenAll();

			Assert.Pass();
		}
	}
}