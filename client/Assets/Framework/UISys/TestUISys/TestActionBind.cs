using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding;
using DataBinding.UIBind;
using UnityEngine;

public class TestActionBindData: IStdHost
{
	public bool TestValue {get; set; } = false;
}

public class TestActionBind : MonoBehaviour
{
	public void Print(bool x)
	{
		Debug.Log($"Print: {x}");
	}

	protected TestActionBindData Data;
	private void Awake()
	{
		Data = new TestActionBindData();
		var host = this.GetComponentInParent<DataHostComp>();
		host.ObserveData(Data);
	}

	public void Test()
	{
		this.Data.TestValue = !this.Data.TestValue;
	}

	private void Update()
	{
		VM.Tick.Next();
	}
}
