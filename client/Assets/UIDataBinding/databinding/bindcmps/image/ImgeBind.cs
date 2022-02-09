
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
//实际上就是把数据绑定的部分暴露给UI属性配置栏
//如果是容器数据，需要有父节代码来绑定实际的属性

public class ImgeBind : BindCompentBase
{
	private Image           _img;
	private RawImage         _rimg;
	public void Start()
	{
		_img       = transform.GetComponent<Image>(); 
        _rimg      = transform.GetComponent<RawImage>(); 
		if(_img != null || _rimg != null && mainProprity!=null && !isContainData)
		{
			initWatcher();
		}
	}

	private void initWatcher()
	{
		//这个数据绑定本质上就是需要把手
		//监听图片更新表达式
		//参数1是表达式 实际上就是观测数据的属性
	    //只有当值发生改变时才会触发事件
		if(dataHost==null) return;
		dataHost.Watch(mainProprity,(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log("image value changed：  "+oldValue+"--->"+value);
			if(_img!=null)
			{
				loadSprite(value as string);
			}
			else
			{
				loadTexture(value as string);
			}
		}
		);
	}

	//加载精灵
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

	//加载纹理
	private void loadTexture(string path)
	{
		if(path == null || path=="") return;
		var tes = Resources.Load<Texture>(path);	
		if(tes!=null && _rimg!=null)
		{
			_rimg.texture = tes;
		}
	}
	
}
