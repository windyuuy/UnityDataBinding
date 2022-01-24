
using UnityEngine;
using UnityEngine.UI;
using DataBinding;

//这个是绑定组件，需要挂载在，Image节点上
//负责监听图片的数据更新，然后更新图片
//ImgeHost是负责注册事件的中间件，是观测数据的实体
//vm.Tick.next();则是负责通知数据发生改变的事件函数
//所有需要更新数据必须要在观测数据中声明属性
//这个数据绑定本质上就是一个单独的观察者模式，一旦数据发生了改变，就通知组件执行相应的事件函数来更新UI
//核心就是一个抽像，把页面需要随数据变化的元素写进数据绑定
//需要每一个组件单独定制这样的数据绑定，需要日积月累的添加。

public class ImgeBind : MonoBehaviour
{
	private Image           _img;
	private ImgeHost        _imgeHost;
	public void Start()
	{
		_img       = transform.GetComponent<Image>(); 
		_imgeHost  = new ImgeHost(); 

		if(_img!=null)
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
		_imgeHost.Watch("imge.imgepath",(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log("value changed：  "+oldValue+"--->"+value);
			loadSprite(value as string);
		}
		);
	}

	public void upadeImageData(string path)
	{
		if(path == null && path == "")return;
		_imgeHost.imge.imgepath = path;
		vm.Tick.next();
	}

	//加载纹理
	private void loadSprite(string path)
	{
		if(path == null || path=="") return;
		//导入纹理

		var sp = Resources.Load<Sprite>(path);	
		if(sp!=null && _img!=null)
		{
			_img.sprite = sp;
		}
	}


	void Update()
	{
		//在Update中调用
		vm.Tick.next();
	}
	
}
