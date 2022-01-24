
using UnityEngine;
using UnityEngine.UI;
using DataBinding;

public class TextBind : MonoBehaviour
{
	private Text            _text;
	private TextHost        _textHost;
	public void Start()
	{
		initText();
	}


	public void initText()
	{
		_text      = transform.GetComponent<Text>(); 

		if(_text!=null && _textHost == null)
		{
			//注册监听器
			initWatcher();
		}
	}

	public void initWatcher()
	{
		//监听图片更新表达式
		//参数1是表达式 实际上就是观测数据的属性
	    //只有当值发生改变时才会触发事件
		_textHost  = new TextHost(); 
		_textHost.Watch("text.textString",(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log(" text value changed：  "+oldValue+"--->"+value);
			showText(value as string);
		}
		);
	}

	public void upadeTextData(string str)
	{
		if(str == null)return;

		initText();
	
		//print("lllllllllllllllll  "+str);
		// print(_textHost);
		// print(_textHost.text);
		// print(_textHost.text.textString);
		_textHost.text.textString = str;
		// vm.Tick.next();
		
	}

	//更新文本
	private void showText(string str)
	{
		if(str == null || str=="" || _text == null) return;
		_text.text = str;
	}
	
	void Update()
	{
		//在Update中调用
		vm.Tick.next();
	}
}
