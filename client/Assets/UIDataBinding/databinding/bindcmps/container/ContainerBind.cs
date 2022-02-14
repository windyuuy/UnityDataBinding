
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;
using DataBinding;
using DataBinding.CollectionExt;


//需要挂载在每一个有子节点的节点上
//容器的根节点需要勾选上 isContainRoot属性
//该组件不需要勾选上     isContainData属性
//根节点的目的是当容器数据有嵌套时方便查找每一项节点的父节点主键值
public class ContainerBind : BindCompentBase
{
	public void Start()
	{
		if(transform!=null && mainProprity!=null &&!isContainData)
		{
			//注册监听器
			initWatcher();
		}
	}

	//主要用于能够直接的使用的
	public void initWatcher()
	{
		if(dataHost==null) return;
		var watcher = dataHost.Watch(mainProprity,(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log("$ contain value changed：  ");
			//类型信息应该是host 对象在的地址mainProprity处的对象类型
			var data = value as Dictionary<int, myData>;
			updateContainerChild<Dictionary<int, myData>>(data,data.Count);
		}
		);

		if(watcher!=null)
		{
			//watcher中是可以直接获取观测值的新值和旧值
			var currentData  = watcher.value as Dictionary<int, myData>;
			updateContainerChild(currentData,currentData.Count);
			watcherlist.Add(watcher);
		}
	}

	//如果有新的数据更新则需要更新子项
	//主要有两种情况
	//第一种，数据项数量没有变化
	//第二种，数据项变多了【之前的节点用老的，然后再创建新的节点】
	//第三种，数据变少了【多余的节点不要删除 直接隐藏掉即可】
	//有子节点再去注册其内容上的数据更新
	private void updateContainerChild<T>(T data,int len) 
	{
		var root       = transform;
		if(root == null || data == null) return;
	
		int showCount    = len;
		GameObject child = root.GetChild(0).gameObject;
		int childnumber  = root.childCount;
		int index = 0;
		for(;index<showCount;index++)
		{
			GameObject item = null;
			if(index<childnumber)
			{
				item = root.GetChild(index).gameObject;
			}
			else
			{
				item = Instantiate<GameObject>(child);
				item.transform.SetParent(root.transform,false);

			}
			item.SetActive(true);
			//再去初始化每一个子项身上的数据绑定主键值,已经绑定的应该不需要重新绑定
			updateContainerChildMainProprity(item,index);
		}

		//如果是有减少的部分则需要进行隐藏
		for (; index < childnumber; index++) 
		{
			root.GetChild(index).gameObject.SetActive(false);
			//不在的项需要调用watcher的teardown进行清理
		}
	}

    public void updateContainerChildMainProprity(GameObject item, int index)
    {
		//依次的检擦字节的节点容器、图片、按钮、复选框、文本、进度条
		//如果有容器组件说明还没有到达最底层需要再进行遍历
		if(item.GetComponentsInChildren<ContainerBind>()!=null && item.GetComponentsInChildren<ContainerBind>().Length>0)
		{
			updateContainerMainProprity(item,index);
		}
		else
		{
			updateImageMainProprity(item,index);
			updateTextMainProprity(item,index);
			updateToggleMainProprity(item,index);
			updateProgressMainProprity(item,index);
			updateButtonMainProprity(item,index);
		}	
    }

	//更新容器绑定组件的主键值
	private void updateContainerMainProprity(GameObject item, int index)
	{
		ContainerBind[] containers = item.GetComponentsInChildren<ContainerBind>();

		print("the len is " +containers.Length);
		for(int i = 0;i<containers.Length;i++)
		{
			containers[i].intContainerWatcher(index);
		}

	}

	//更新图片绑定组件的主键值
	private void updateImageMainProprity(GameObject item, int index)
	{
		ImgeBind[] imges = item.GetComponentsInChildren<ImgeBind>();
		for(int i = 0;i<imges.Length;i++)
		{
			imges[i].intContainerWatcher(index);
		}
	}

	//更新文本绑定组件的主键值
	private void updateTextMainProprity(GameObject item, int index)
	{
		TextBind[] texts = item.GetComponentsInChildren<TextBind>();
		for(int i = 0;i<texts.Length;i++)
		{
			texts[i].intContainerWatcher(index);
		}
	}


	//更新复选框绑定组件的主键值
	private void updateToggleMainProprity(GameObject item, int index)
	{
		ToggleBind[] toggles = item.GetComponentsInChildren<ToggleBind>();
		for(int i = 0;i<toggles.Length;i++)
		{
			toggles[i].intContainerWatcher(index);
		}
	}

	//更新进度条绑定组件上的主键值
	private void updateProgressMainProprity(GameObject item, int index)
	{
		ProgressBind[] progress = item.GetComponentsInChildren<ProgressBind>();
		for(int i = 0;i<progress.Length;i++)
		{
			progress[i].intContainerWatcher(index);
		}
	}

	//更新按钮绑定组件上的主键值
	private void updateButtonMainProprity(GameObject item, int index)
	{
		ButtonBind[] btns = item.GetComponentsInChildren<ButtonBind>();
		for(int i = 0;i<btns.Length;i++)
		{
			btns[i].intContainerWatcher(index);
		}
	}


	//有子容器时需要进行拼接操作
	//容器进度条初始化 绑定的组件需要进行拼接
	public void intContainerWatcher(int index)
	{
		if(transform!=null&& mainProprity!=null && isContainData)
		{
			if(dataHost==null) return;
			string fathetexp = GetFatherMainProprity(transform);

			string  exp      = fathetexp+'['+index+']'+'.'+mainProprity;

			currentExpress = exp;

			print("the vlue is :"+exp);
			//代码去注册容器绑定主键是 导致触发更改时值是上一次的值 有延后需要收动先执行一次刷新，要并且的容器子节点都有这个问题
			//绑定插件记录的值是绑定代码调用时的属性值，如果还没有赋值那就是空值
			//print("#################################--> :"+dataHost.dataObject.listDictionary[0].myheadList.Count);
			//注册watcher
		    var watcher = dataHost.Watch(exp,(host, value, oldValue) =>
			{
				//数值发生了改变
				Debug.Log(" Container value changed：  ");

				var data = value as List<myShowData>;
				updateContainerChild<List<myShowData>>(data,data.Count);
			});
			if(watcher!=null)
			{
				//watcher中是可以直接获取观测值的新值和旧值
				var currentData  = watcher.value as List<myShowData>;
				updateContainerChild(currentData,currentData.Count);
				watcherlist.Add(watcher);
			}
		}
	}








}
