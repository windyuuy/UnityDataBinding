
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
		dataHost.Watch(mainProprity,(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log(" toggle value changed：  "+oldValue+"--->"+value);
			showToggle((bool)value);
		}
		);
	}

	//更新文本
	private void showToggle(bool isSelect)
	{
		if(_toggle!=null)
		{
			_toggle.isOn = isSelect;
		}
	}
}
