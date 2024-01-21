using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using gcc.layer;
using RSG;
using UnityEngine;

public class HomeAPI
{
	public static Promise<LayerModel> ShowUIExample1()
	{
		return UIMain.LayerManager.ShowLayer(new ShowLayerParam(
			"UIExample1", null, "Assets/TestDataBinding/TestUIOrg/UIExample1/Layer/Viarant/UIExample1.prefab"));
	}
}
