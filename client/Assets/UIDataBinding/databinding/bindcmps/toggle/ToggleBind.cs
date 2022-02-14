
using UnityEngine;
using UnityEngine.UI;
using DataBinding;


public class ToggleBind : BindCompentBase
{
	private Toggle            _toggle;
	public void Start()
	{
		_toggle      = transform.GetComponent<Toggle>(); 
		if(_toggle!=null&& mainProprity!=null && !isContainData)
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
		var watcher = dataHost.Watch(mainProprity,(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log(" toggle value changed：  "+oldValue+"--->"+value);
			showToggle((bool)value);
		}
		);

		if(watcher!=null)
		{

			var currentData  = (bool)watcher.value;
			showToggle((bool)currentData);
			watcherlist.Add(watcher);
		}
	}

	//更新文本
	private void showToggle(bool isSelect)
	{
		if(_toggle!=null)
		{
			_toggle.isOn = isSelect;
		}
	}


	//容器复选框初始化 绑定的组件需要进行拼接
	public void intContainerWatcher(int index)
	{
		_toggle = GetComponent<Toggle>();
		if(_toggle!=null&& mainProprity!=null && isContainData)
		{
			print("初始化index "+index);
			if(dataHost==null) return;
			string fathetexp = GetFatherMainProprity(transform);

			string  exp      = fathetexp+'['+index+']'+'.'+mainProprity;
			currentExpress = exp;

			print("the vlue is :"+exp);

			//注册watcher
		    var watcher = dataHost.Watch(exp,(host, value, oldValue) =>
			{
				//数值发生了改变
				Debug.Log(" toggle value changed：  "+oldValue+"--->"+value);
				showToggle((bool)value);
			});

			if(watcher!=null)
			{

				var currentData  = (bool)watcher.value;
				showToggle((bool)currentData);
				watcherlist.Add(watcher);
			}
		}
	}
}
