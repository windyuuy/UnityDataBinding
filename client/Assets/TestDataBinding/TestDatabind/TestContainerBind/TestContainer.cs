using System.Linq;
using System.Linq.Ext;
using DataBinding.CollectionExt;
using UnityEngine.UI;

namespace DataBinding.UIBind.Tests.TestContainer
{
	using number = System.Double;

	class TC2Item : IStdHost
	{
		public string pp { get; set; } = "PP";
		public string QQ { get; set; } = "QQ";
	}
	class TC1Item : IStdHost
	{
		public List<TC2Item> C2 { get; set; } = new List<TC2Item>();
	}
	class TRawData:IStdHost
	{
		public List<TC1Item> C1 { get; set; } = new List<TC1Item>();
	}
	public class TestContainer : TestBase
	{
		TRawData rawData => (TRawData)_rawData;
		protected override void InitTestData()
		{
			_rawData = new TRawData()
			{
				C1 = new List<TC1Item>()
				{
					new TC1Item()
					{
						C2 = new List<TC2Item>()
						{
							new TC2Item()
							{
								QQ="QQ",
								pp ="PP",
							},
							new TC2Item()
							{
								QQ ="QQ2",
								pp="PP2",
							},
						},
					},
					new TC1Item()
					{
						C2 = new List<TC2Item>()
						{
							new TC2Item()
							{
								QQ="QQ3",
								pp ="PP3",
							},
							new TC2Item()
							{
								QQ ="QQ4",
								pp="PP4",
							},
						},
					},
				},
			};
		}
		public override void Test()
		{
			var C1 = this.CN("c")!;
			var items = C1.GetComponentsInChildren<Text>();
			Assert(C1.CNRFF<Text>("Node[0]/sc/item[0]/Label").text == this.rawData.C1[0].C2[0].QQ);
			Assert(C1.CNRFF<Text>("Node[1]/sc/item[1]/Label-001").text == this.rawData.C1[1].C2[1].pp);
			var Node0 = C1.CNRF("Node[0]");
			var Node1 = C1.CNRF("Node[1]");
			//Node1.parent = null;
			// TODO: ����ӳ����Ƴ��ڵ������
			Node1.gameObject.SetActive(false);
			var sample0 = this.rawData.C1[1].C2[0].QQ;
			Assert(Node1.CNRFF<Text>("sc/item[0]/Label").text == sample0);
			var sample1 = "jjj";
			this.rawData.C1[1].C2[0].QQ = sample1;
			this.Tick();
			Assert(Node1.CNRFF<Text>("sc/item[0]/Label").text == sample0);
			//Node1.parent = C1;
			Node1.gameObject.SetActive(true);
			this.Tick();
			Assert(C1.CNRFF<Text>("Node[1]/sc/item[0]/Label").text == sample1);
			this.rawData.C1.Add(new TC1Item()
			{
				C2 = new List<TC2Item>()
				{
					new TC2Item()
					{
						QQ = "QQ5",
						pp = "pp5",
					},

					new TC2Item()
					{
						QQ = "QQ6",
						pp = "pp6",
					},
				},
			});
			this.rawData.C1.Add(new TC1Item()
			{
				C2 = new List<TC2Item>()
				{
					new TC2Item(){
						QQ = "QQ7",
						pp = "pp7",

					},
					new TC2Item(){
						QQ = "QQ8",
						pp = "pp8",
					},
				},
			});
			this.rawData.C1[1].C2.Add(new TC2Item()
			{
				QQ = "QQX1",
				pp = "ppx1",
			});
			this.Tick();
			items = C1.GetComponentsInChildren<Text>();
			Assert(C1.CNRFF<Text>("Node[1]/sc/item[2]/Label-001").text == this.rawData.C1[1].C2[2].pp);
			Assert(C1.CNRFF<Text>("Node[3]/sc/item[1]/Label-001").text == this.rawData.C1[3].C2[1].pp);
			this.Tick();
			this.rawData.C1.RemoveAt(1);
			Assert(C1.CNRS("Node").Select(n=>n.gameObject.activeSelf).ToArray().Length == this.rawData.C1.Count+1);
			this.Tick();
			Assert(C1.CNRS("Node").Where(n => n.gameObject.activeSelf).ToArray().Length == this.rawData.C1.Count);
			this.rawData.C1.RemoveAt(0);
			this.Tick();
			Assert(C1.CNRS("Node").Where(n => n.gameObject.activeSelf).ToArray().Length == this.rawData.C1.Count);
			items = C1.GetComponentsInChildren<Text>(true);
			Assert(items.Length == 18);
			items = items.Where(item => item.IsEnabledInHierarchy()).ToArray();
			Assert(items.Length == 8);
			var sample2 = this.rawData.C1[1].C2[0].QQ;
			Node1 = C1.CNRF("Node[1]");
			//Node1.parent = null;
			Node1.gameObject.SetActive(false);
			this.Tick();
			this.rawData.C1[1].C2[0].QQ = "sample31";
			this.Tick();
			this.rawData.C1[1].C2[0].QQ = "sample32";
			this.Tick();
			this.rawData.C1[1].C2[0].QQ = "sample33";
			this.Tick();
			var sample3 = "jjjxx";
			this.rawData.C1[1].C2[0].QQ = sample3;
			this.Tick();
			Assert(C1.CNRFF<Text>("Node[1]/sc/item[0]/Label").text == sample2);
			//Node1.parent = C1;
			Node1.gameObject.SetActive(true);
			this.Tick();
			Assert(C1.CNRFF<Text>("Node[1]/sc/item[0]/Label").text == sample3);
		}
		public override void TestLazy()
		{
			System.Action checkActive = () =>
			{
				this.gameObject.SetActive(true);
				// check
				{
					var Nodes1 = this.CN("c")!.CNS("Node").Where(n => n.gameObject.activeSelf).ToArray();
					Assert(Nodes1.Length == this.rawData.C1.Count);
					Nodes1.ForEach((node, index) =>
				{
					var items = node.CN("sc")!.CNS("item").Where(n => n.gameObject.activeSelf).ToArray();
					Assert(items.Length == this.rawData.C1[index].C2.Count);
					items.ForEach((item, index1) =>
					{
						var labels = item.GetComponentsInChildren<Text>();
						var index2 = 0;

						Assert(labels[index2].text == this.rawData.C1[index].C2[index1].QQ);
						index2 = 1;

						Assert(labels[index2].text == this.rawData.C1[index].C2[index1].pp);
					});
				});
				}
			};
			this.rawData.C1.Clear();
			this.rawData.C1.Push(
				new TC1Item()
				{
					C2 =
					new List<TC2Item>(){
						new TC2Item(){
							QQ = "QQ",
							pp = "PP",
						},
						new TC2Item(){
							QQ = "QQ2",
							pp = "PP2",
						},
					},
				},
				new TC1Item()
				{
					C2 = new List<TC2Item>()
						{
							new TC2Item(){
							QQ = "QQ3",
							pp = "PP3",

						},
						new TC2Item(){
							QQ = "QQ4",
							pp = "PP4",
						},
					},
				});

			this.Tick();
			checkActive();

			void TestPushPop(double testOrder)
			{
				this.gameObject.SetActive(false);
				var C1_2 = new List<TC1Item>() { new TC1Item() { C2 = new List<TC2Item>() { new TC2Item() { QQ = "QQ5", pp = "pp5", }, new TC2Item() { QQ = "QQ6", pp = "pp6", }, }, } };

				C1_2.ForEach(c => this.rawData.C1.Add(c));
				// this.rawData.C1.push(...C1_2)
				this.Tick();
				// check
				{
					var Nodes1 = this.CN("c")!.CNS("Node").Where(n => n.gameObject.activeSelf).ToArray();
					Assert(Nodes1.Length == this.rawData.C1.Count - C1_2.Count);
					Nodes1.ForEach((node, index) =>
					{
						var items = node.CN("sc")!.CNS("item");
						Assert(items.Where(n => n.gameObject.activeSelf).ToArray().Length == this.rawData.C1[index].C2.length);
					});
				}
				checkActive();
				this.gameObject.SetActive(false);
				var C1_3 = new List<TC1Item>() { new TC1Item() { C2 = new List<TC2Item>() { new TC2Item() { QQ = "QQ7", pp = "pp7", }, }, } };
				this.rawData.C1[2].C2.push(C1_3[0].C2[0]);
				this.Tick();
				// check
				{
					var Nodes1 = this.CN("c")!.CNS("Node").Where(n => n.gameObject.activeSelf).ToArray();
					Assert(Nodes1.Length == this.rawData.C1.Count);
					Nodes1.Slice(0, 2)
						.ForEach((node, index) =>
						{
							var items = node.CN("sc")!.CNS("item");
							Assert(items.Where(n => n.gameObject.activeSelf).ToArray().Length == this.rawData.C1[index].C2.length);
						});
					Nodes1.Slice(2)
						.ForEach((node, index) =>
						{
							var items = node.CN("sc")!.CNS("item");
							Assert(items.Where(n => n.gameObject.activeSelf).ToArray().Length == this.rawData.C1[index + 2].C2.length - C1_3[0].C2.length);
						});
				}
				checkActive();
				this.gameObject.SetActive(false);
				this.rawData.C1[2].C2.RemoveAt(1);
				this.Tick();
				// check
				{
					var Nodes1 = this.CN("c")!.CNS("Node").Where(n => n.gameObject.activeSelf).ToArray();
					Assert(Nodes1.Length == this.rawData.C1.length);
					Nodes1.Slice(0, 2)
						.ForEach((node, index) =>
						{
							var items = node.CN("sc")!.CNS("item");
							Assert(items.Where(n => n.gameObject.activeSelf).ToArray().Length == this.rawData.C1[index].C2.length);
						});
					Nodes1.Slice(2)
						.ForEach((node, index) =>
						{
							var items = node.CN("sc")!.CNS("item");
							Assert(items.Where(n => n.gameObject.activeSelf).ToArray().Length == this.rawData.C1[index + 2].C2.length + 1);
						});
				}
				checkActive();
				this.gameObject.SetActive(false);
				this.rawData.C1[2].C2.pop();
				this.Tick();
				// check
				{
					var Nodes1 = this.CN("c").CNS("Node").Where(n => n.gameObject.activeSelf).ToArray();
					Assert(Nodes1.Length == this.rawData.C1.length);
					Nodes1.Slice(0, 2)
						.ForEach((node, index) =>
						{
							var items = node.CN("sc")!.CNS("item");
							Assert(items.Where(n => n.gameObject.activeSelf).ToArray().Length == this.rawData.C1[index].C2.length);
						});
					Nodes1.Slice(2)
						.ForEach((node, index) =>
						{
							var items = node.CN("sc")!.CNS("item");
							Assert(items.Where(n => n.gameObject.activeSelf).ToArray().Length == this.rawData.C1[index + 2].C2.length + C1_3[0].C2.length);
						});
				}
				checkActive();
				this.gameObject.SetActive(false);
				this.rawData.C1.pop();
				this.Tick();
				// check
				{
					var Nodes1 = this.CN("c")!.CNS("Node").Where(n => n.gameObject.activeSelf).ToArray();
					Assert(Nodes1.Length == this.rawData.C1.length + C1_2.length);
					Nodes1.Slice(0, 2)
						.ForEach((node, index) =>
						{
							var items = node.CN("sc")!.CNS("item");
							Assert(items.Where(n => n.gameObject.activeSelf).ToArray().Length == this.rawData.C1[index].C2.length);
						});
					Nodes1.Slice(2)
						.ForEach((node, index) =>
						{
							var items = node.CN("sc")!.CNS("item");
							Assert(items.Where(n => n.gameObject.activeSelf).ToArray().Length == C1_2[0].C2.length);
						});
				}
				checkActive();
			}

			TestPushPop(1);
			TestPushPop(2);
		}
	}
}
