
using System;
using UnityEngine;
using UnityEngine.UI;
using DataBinding;
using UnityEngine.Events;


public class ButtonBind : BindCompentBase
{
	private Button           _btn;
	public void Start()
	{
		_btn         = transform.GetComponent<Button>(); 

		if(_btn!=null && mainProprity!=null)
		{
			//注册监听器
			initWatcher();
		}
	}

	private void initWatcher()
	{
		//监听图片更新表达式
		//参数1是表达式 实际上就是观测数据的属性
	    //只有当值发生改变时才会触发事件
		if(dataHost==null) return;
		dataHost.Watch(mainProprity,(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log("callback value changed：  ");
			addButtonCallback(value as UnityAction);
		}
		);
	}

	//注册按钮的点击事件
	private void addButtonCallback(UnityAction callback)
	{
		if(callback == null || _btn == null) return;

		//先移除旧事件
		_btn.onClick.RemoveAllListeners();
		//注册一下事件
		_btn.onClick.AddListener(callback);
	}
}
