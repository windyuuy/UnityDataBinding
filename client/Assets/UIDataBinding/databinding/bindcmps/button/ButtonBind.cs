
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
			addButtonCallback(value as Action);
		}
		);
	}

	//注册按钮的点击事件
	private void addButtonCallback(Action callback)
	{
		if(callback == null || _btn == null) return;

		//先移除旧事件
		_btn.onClick.RemoveAllListeners();
		//注册一下事件
		_btn.onClick.AddListener(()=>{callback();});
	}


	//容器复选框初始化 绑定的组件需要进行拼接
	public void intContainerWatcher(int index)
	{
		_btn         = transform.GetComponent<Button>();
		if(_btn!=null&& mainProprity!=null && isContainData)
		{
			if(dataHost==null) return;
			string fathetexp = GetFatherMainProprity(transform);

			string  exp      = fathetexp+'['+index+']'+'.'+mainProprity;
			currentExpress = exp;

			print("the vlue is :"+exp);

			//注册watcher
		    var watcher = dataHost.Watch(exp,(host, value, oldValue) =>
			{
				//数值发生了改变
				Debug.Log(" callback value changed：  "+oldValue+"--->"+value);
				addButtonCallback(value as Action);
			});

			if(watcher!=null)
			{
				watcherlist.Add(watcher);
			}
		}
	}
}
