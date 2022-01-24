
using UnityEngine;
using UnityEngine.UI;
using DataBinding;


public class ProgressBind : MonoBehaviour
{
	private  Slider        _slider;
	private ProgressHost     _sliderHost;
	public void Start()
	{
		_slider      = transform.GetComponent<Slider>(); 
		_sliderHost  = new ProgressHost(); 

		if(_slider!=null)
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
		_sliderHost.Watch("progress.value",(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log(" progress value changed：  "+oldValue+"--->"+value);
			showProgress((float)value);
		}
		);
	}

	public void upadeProgressData(float value)
	{
		if(_sliderHost == null)return;
		_sliderHost.progress.value = value;
		vm.Tick.next();
	}

	//更新进度条
	private void showProgress(float value)
	{
		if(_sliderHost!=null)
		{
			_slider.value = value;
		}
	}
	
	void Update()
	{
		//在Update中调用
		vm.Tick.next();
	}
}
