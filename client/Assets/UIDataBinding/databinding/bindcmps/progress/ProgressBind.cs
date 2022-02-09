
using UnityEngine;
using UnityEngine.UI;
using DataBinding;


public class ProgressBind : BindCompentBase
{
	private  Slider        _slider;
	public void Start()
	{
		_slider      = transform.GetComponent<Slider>(); 
		if(_slider!=null&& mainProprity!=null && !isContainData)
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
			Debug.Log(" progress value changed：  "+oldValue+"--->"+value);
			showProgress((float)value);
		}
		);
	}

	//更新进度条
	private void showProgress(float value)
	{
		if(_slider!=null)
		{
			_slider.value = value;
		}
	}
}
