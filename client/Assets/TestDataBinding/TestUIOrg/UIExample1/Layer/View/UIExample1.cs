using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding.UIBind;
using TestingCode;

public class UIExample1 : UILayer
{
	private CCDataHost _dataHost;

	public CCDataHost DataHost
	{
		get { return this._dataHost; }
	}

	protected virtual void InitViewModel()
	{
	}
	
	//#region data host
	protected void IntegrateDataBind()
	{
		if (this._dataHost == null)
		{
			var ccDataHost = this.GetComponent<CCDataHost>();
			if (ccDataHost == null)
			{
				ccDataHost = this.gameObject.AddComponent<CCDataHost>();
			}

			this._dataHost = ccDataHost;
		}
	}

	/**
	 * 观测数据
	 * @param data
	 */
	public void ObserveData(object data, bool updateChildren = true)
	{
		if (this.DataHost)
		{
			this.DataHost.ObserveData(data);
		}
	}

	public UIExample1ViewModel ViewModel
	{
		get => (_layerModel as UIExample1LayerModel)?.ViewModel;
	}

	private void Awake()
	{
		this.IntegrateDataBind();
		this.InitViewModel();
		this.ObserveData(this.ViewModel);
	}

	// protected override void onInit(object data = null)
	// {
	// 	Invoke("DFE",1);
	// }
	//
	// void DFE()
	// {
	// }

	protected override void doCreateLayerModel()
	{
		this.LayerModel = new UIExample1LayerModel();
	}
}