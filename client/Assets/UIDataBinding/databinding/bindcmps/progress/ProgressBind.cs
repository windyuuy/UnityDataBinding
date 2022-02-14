
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
		var watcher = dataHost.Watch(mainProprity,(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log(" progress value changed：  "+oldValue+"--->"+value);
			showProgress((float)value);
		}
		);

		if(watcher!=null)
		{

			var currentData  =(float) watcher.value;
			showProgress(currentData);
			watcherlist.Add(watcher);
		}



	}

	//更新进度条
	private void showProgress(float value)
	{
		if(_slider!=null)
		{
			_slider.value = value;
		}
	}

	//容器进度条初始化 绑定的组件需要进行拼接
	public void intContainerWatcher(int index)
	{
		_slider = GetComponent<Slider>();
		if(_slider!=null&& mainProprity!=null && isContainData)
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
				Debug.Log(" progress value changed：  "+oldValue+"--->"+value);
				showProgress((float)value);
			});

			if(watcher!=null)
			{

				var currentData  =(float) watcher.value;
				showProgress(currentData);
				watcherlist.Add(watcher);
			}
		}
	}
}
