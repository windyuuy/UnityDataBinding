
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;
using DataBinding;
using DataBinding.CollectionExt;

//需要挂载在每一个有子节点的节点上

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

	public void initWatcher()
	{
		if(dataHost==null) return;
		dataHost.Watch(mainProprity,(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log("contain value changed：  ");
			//更新子节点
			updateContainerChild(value as List<myData>);
		}
		);
	}

	//如果有新的数据更新则需要更新子项
	//主要有两种情况
	//第一种，数据项数量没有变化
	//第二种，数据项变多了【之前的节点用老的，然后再创建新的节点】
	//第三种，数据变少了【多余的节点不要删除 直接隐藏掉即可】
	//有子节点再去注册其内容上的数据更新
	private void updateContainerChild(List<myData> data) 
	{
		var root       = transform;
		if(root == null || data == null) return;
	
		int showCount    = data.Count;
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
			//再去初始化每一个子项身上的数据绑定主键值
			updateContainerChildMainProprity(item,index);
		}
		//如果是有减少的部分则需要进行隐藏
		for (; index < childnumber; index++) 
		{
			root.GetChild(index).gameObject.SetActive(false);
		}
	}

    private void updateContainerChildMainProprity(GameObject item, int index)
    {
		print("当前的index是  ----> "+index);
        //目前组件只有按钮、图片、复选框、进度条、文本
		TextBind[] texts = item.GetComponentsInChildren<TextBind>();
		for(int i = 0;i<texts.Length;i++)
		{
			texts[i].intContainerWatcher(index);
		}
    }

	void Update()
	{
		//在Update中调用
		vm.Tick.next();
	}
}
