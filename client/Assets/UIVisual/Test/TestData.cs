using System;
using Unity.VisualScripting;
using DataBinding;
using UnityEngine;

// TODO: adapt AutoFieldProperty for visualscripting

// [AutoFieldProperty]
[Serializable]
public class TestData2 : IStdHost
{
	public int testkey2 { get; set; } = 342;
}

[Inspectable]
// [AutoFieldProperty]
[Serializable]
public class TestData : IStdHost
{
	public string testkey { get; set; } = "nonekey";

	public System.Func<object, object, object> testfunc { get; set; } = (object a, object b) =>
	{
		return TestData.Inst.data2.testkey2;
	};

	public TestData2 data2 { get; set; } = new TestData2();

	public static readonly TestData Inst = new TestData();
}

[StdHost]
// [AutoFieldProperty]
[Serializable]
public class TestData3
{
	public string testkey { get; set; } = "nonekey3";
}