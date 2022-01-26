
using UnityEngine;
using UnityEngine.UI;
using DataBinding;


public class ToggleBind : MonoBehaviour
{
	private Toggle            _toggle;
	private ToggleHost        _toggleHost;
	public void Start()
	{
		_toggle      = transform.GetComponent<Toggle>(); 
		_toggleHost  = new ToggleHost(); 

		if(_toggle!=null)
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
		_toggleHost.Watch("toggle.isSelect",(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log(" toggle value changed：  "+oldValue+"--->"+value);
			showToggle((bool)value);
		}
		);
	}

	public void upadeToggleData(bool isSelect)
	{
		print("llllllllllllll"+isSelect);
		if(_toggleHost == null)return;
		_toggleHost.toggle.isSelect = isSelect;
		vm.Tick.next();
	}

	//更新文本
	private void showToggle(bool isSelect)
	{
		if(_toggle!=null)
		{
			_toggle.isOn = isSelect;
		}
	}

	void Update()
	{
		//在Update中调用
		vm.Tick.next();
	}
}
