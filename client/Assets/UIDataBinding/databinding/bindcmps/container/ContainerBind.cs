
using System.Transactions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataBinding;
using DataBinding.CollectionExt;

public class ContainerBind : MonoBehaviour
{
	private ScrollRect        _scrollRect;
	private ContainerHost    _scrollviewHost;
	public void Start()
	{
		_scrollRect      = transform.GetComponent<ScrollRect>(); 
		_scrollviewHost  = new ContainerHost(); 

		if(_scrollRect!=null)
		{
			//注册监听器
			initWatcher();
		}
	}

	public void initWatcher()
	{
		_scrollviewHost.Watch("scrollview.IntList",(host, value, oldValue) =>
		{
			//数值发生了改变
			Debug.Log(" scrollview value changed：  ");
			//更新子节点
			updateContainerChild(value as DataBinding.CollectionExt.List<int>);
		}
		);
	}

	public void upadeScrollViewData(System.Collections.Generic.List<int> list)
	{
		print("列表的长度："+list.Count);
		if(_scrollRect == null || list == null)return;
		var curlist = new DataBinding.CollectionExt.List<int>();
		foreach (var item in list)
		{
			curlist.Add(item);
		}
		_scrollviewHost.scrollview.IntList = curlist;
	}

	//如果有新的数据更新则需要更新子项
	//主要有两种情况
	//第一种，数据项数量没有变化
	//第二种，数据项变多了【之前的节点用老的，然后再创建新的节点】
	//第三种，数据变少了【多余的节点不要删除 直接隐藏掉即可】
	//有子节点再去注册其内容上的数据更新
	private void updateContainerChild(DataBinding.CollectionExt.List<int> data) 
	{
		var root       = _scrollRect.content;
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
			//再去更新子节点上内容的数据绑定
			updateContainerChildBindComps(item,data[index]);
		}
		//如果是有减少的部分则需要进行隐藏
		for (; index < childnumber; index++) 
		{
			root.GetChild(index).gameObject.SetActive(false);
		}
	}

    private void updateContainerChildBindComps(GameObject item, int value)
    {
        //目前组件只有按钮、图片、复选框、进度条、文本
		TextBind[] texts = item.GetComponentsInChildren<TextBind>();
		print("##################################"+texts.Length);
		for(int i = 0;i<texts.Length;i++)
		{
			texts[i].upadeTextData(value.ToString());
		}
    }

	void Update()
	{
		//在Update中调用
		vm.Tick.next();
	}
}
