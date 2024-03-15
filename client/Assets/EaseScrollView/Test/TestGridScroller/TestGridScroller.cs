using System.Collections;
using System.Linq;
using DataBind;
using DataBind.CollectionExt;
using TestDataBind.TestBasic;
using UnityEngine;
using UnityEngine.UI;

namespace TestDataBind.Tests.TestGridScroller
{
	class TC1Item : IStdHost
	{
		public string Title { get; set; }
	}

	class TRawData : IStdHost
	{
		public List<TC1Item> C1 { get; set; } = new List<TC1Item>();
	}

	public class TestGridScroller : TestBase
	{
		TRawData rawData => (TRawData)_rawData;

		public int createCount = 0;
		protected override void InitTestData()
		{
			_rawData = new TRawData()
			{
				C1 = new List<TC1Item>()
				{
				},
			};

			for (var i = 0; i < createCount; i++)
			{
				rawData.C1.Add(new()
				{
					Title = $"title_{i}",
				});
			}

			StartCoroutine(DelayTest());
		}

		protected IEnumerator DelayTest()
		{
			yield return new WaitForSeconds(1);
			for (var i = 0; i < rawData.C1.Count; i++)
			{
				rawData.C1[i].Title = $"cc_x{i}";
			}
			
			yield return new WaitForSeconds(1);
			for (var i = rawData.C1.Count; i <= 12; i++)
			{
				rawData.C1.Add(new()
				{
					Title = $"title_{i}",
				});
			}
			
			// rawData.C1.Add(rawData.C1[3]);
			// rawData.C1.Add(rawData.C1[3]);
			
			yield return new WaitForSeconds(1);
			(rawData.C1[6], rawData.C1[4]) = (rawData.C1[4], rawData.C1[6]);
			
			yield return new WaitForSeconds(1);
			(rawData.C1[5], rawData.C1[4], rawData.C1[2]) = (rawData.C1[4], rawData.C1[2], rawData.C1[5]);
			
			yield return new WaitForSeconds(1);
			(rawData.C1[6], rawData.C1[7], rawData.C1[2]) = (rawData.C1[4], rawData.C1[2], rawData.C1[7]);
			
			{
				yield return new WaitForSeconds(1);
				var di5 = rawData.C1[5];
				var di3 = rawData.C1[3];
				rawData.C1.RemoveAt(5);
				rawData.C1.Insert(5, di3);
				rawData.C1.RemoveAt(3);
				rawData.C1.Insert(3, di5);
			}
			
			yield return new WaitForSeconds(5);
			rawData.C1.RemoveAt(5);
			rawData.C1.RemoveAt(3);
			rawData.C1.RemoveAt(1);
			
			{
				yield return new WaitForSeconds(1);
				var di5 = rawData.C1[5];
				var di3 = rawData.C1[3];
				rawData.C1.RemoveAt(5);
				rawData.C1.RemoveAt(3);
				
				yield return new WaitForSeconds(3);
				rawData.C1.Insert(5, di3);
				rawData.C1.Insert(5, di3);
				rawData.C1.Insert(3, di5);
				rawData.C1.RemoveAt(2);
				(rawData.C1[6], rawData.C1[7], rawData.C1[2]) = (rawData.C1[4], rawData.C1[2], rawData.C1[7]);
			}
			
			
			yield return new WaitForSeconds(1);
			rawData.C1.RemoveAt(5);
			rawData.C1.RemoveAt(3);
			rawData.C1.RemoveAt(1);
			
			yield return new WaitForSeconds(3);
			rawData.C1.Insert(5, rawData.C1[3]);
			rawData.C1.Insert(3, rawData.C1[3]);
			rawData.C1.Insert(1, rawData.C1[3]);
			
			yield return new WaitForSeconds(2);
			rawData.C1.Insert(5, rawData.C1[3]);
			rawData.C1.Insert(3, rawData.C1[3]);
			rawData.C1.Insert(1, rawData.C1[3]);
			rawData.C1.Insert(1, rawData.C1[3]);
			rawData.C1.Insert(1, rawData.C1[3]);
			rawData.C1.Insert(1, rawData.C1[3]);
			
			
			yield return new WaitForSeconds(5);
			for (var i = 0; i < 30; i++)
			{
				rawData.C1.Insert(8, rawData.C1[3]);
			}
		}

		public void TestA()
		{
			var content = transform.Find("Scroll View").Find("Content");
			var loc = content.localPosition;
			loc.x -= 700;
			content.localPosition = loc;
		}
	}
}