
using UnityEngine;
using UnityEngine.UI;
using DataBinding;

public class TextBind : BindCompentBase
{
	private Text            _text;
	public void Start()
	{
		_text = GetComponent<Text>();
		if(_text!=null&& mainProprity!=null && !isContainData)
		{
			initWatcher();
		}
	}

	public void initWatcher()
	{
		//监听图片更新表达式
		//参数1是表达式 实际上就是观测数据的属性
	    //只有当值发生改变时才会触发事件
		if(dataHost==null) return;
		var watcher = dataHost.Watch(mainProprity,(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log(" text value changed：  "+oldValue+"--->"+value);
			showText(value as string);
		}
		);

		if(watcher!=null)
		{

			var currentData  = watcher.value as string;
			showText(currentData);
			watcherlist.Add(watcher);
		}	
	}

	//更新文本
	private void showText(string str)
	{
		if(str == null || str=="" || _text == null) return;
		_text.text = str;
	}


	//容器文本初始化 绑定的组件需要进行拼接
	public void intContainerWatcher(int index)
	{
		_text = GetComponent<Text>();
		print("初始化index "+index);
		print(_text);
		if(_text!=null&& mainProprity!=null && isContainData)
		{
			print("初始化index "+index);
			if(dataHost==null) return;
			string fathetexp = GetFatherMainProprity(transform);

			string  exp      = fathetexp+'['+index+']'+'.'+mainProprity;
			currentExpress   = exp;

			print("the vlue is :"+exp);

			//注册watcher
		    var watcher = dataHost.Watch(exp,(host, value, oldValue) =>
			{
				//数值发生了改变
				Debug.Log(" text value changed：  "+oldValue+"--->"+value);
				showText(value as string);
			});

			if(watcher!=null)
			{

				var currentData  = watcher.value as string;
				showText(currentData);
				watcherlist.Add(watcher);
			}
		}
	}
}
