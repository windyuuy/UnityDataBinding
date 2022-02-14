using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBinding;

//这个是绑定组件的基类
//主要是为了抽象出绑定组件一些公共的属性和方法
//比如一个最重要的就是获取当前UI对象的绑定组件_dataHost
public class BindCompentBase : MonoBehaviour
{

    //需要绑定的数据字符串表达式
	[SerializeField][Header("绑定的主属性地址")]
	public  string mainProprity = null;

    //容器数据需要进行数据的拼接
    [SerializeField][Header("是否属于容器数据")]
	public  bool isContainData = false;
    [SerializeField][Header("是否属于容器的根节点")]
    public  bool isContainRoot = false;

    //父亲节点的主键表达式
    private string fatherExp = "";

    //当前节点的表达式
    [SerializeField][Header("当前节点的表达式（用于测试结果）")]
    public  string currentExpress;


    //需要建立一个静态的watcher注册表，当一个节点被隐藏或者删除是需要主动的清理掉它的
    public List<vm.Watcher> watcherlist = new List<vm.Watcher>();

    //数据的观测者对象
    CommentHost  _dataHost;
    public CommentHost dataHost 
    {
        get{
            //要去遍历的字节点找到UI的CommentHost组件
            FindDataHostComp(transform);
            //print("the father name is : "+_dataHost.transform.name);
            return _dataHost;
        }
    }

    public void FindDataHostComp(Transform child)
    { 
        if(child!=null && child.GetComponent<DataHostCompent>()!=null)
        {
            _dataHost =  child.GetComponent<DataHostCompent>().dataHost;
            return;
        }
        else
        {
            if(child!=null && child.parent!=null)
            {
                FindDataHostComp(child.parent);
            }
        }   
    }

    //查找到子节点的父节点[容器节点的]的绑定主键
    public void FindFatherMainProprity(Transform child)
    { 
        if(child!=null && child.GetComponent<ContainerBind>()!=null && child.GetComponent<ContainerBind>().isContainRoot)
        {
            fatherExp = child.GetComponent<ContainerBind>().mainProprity;
            return;
        }
        if(child!=null && child.parent!=null)
        {
            if(child.parent.GetComponent<ContainerBind>()!=null)
            {
                if(child.parent.GetComponent<ContainerBind>().isContainRoot)
                {
                    fatherExp = child.parent.GetComponent<ContainerBind>().mainProprity;
                }
                else
                {
                    fatherExp = child.parent.GetComponent<ContainerBind>().currentExpress;
                }
                return;
            }
            else
            {
                FindFatherMainProprity(child.parent);
            }
        }
    }

    //获取父节点的主键
    public string GetFatherMainProprity(Transform child)
    {
        fatherExp = "";
        FindFatherMainProprity(child);
        return fatherExp;
    }


    void ClearAllWatcher()
    {
        foreach (var item in watcherlist)
        {
            if(item!=null)
            {
                item.teardown();
            } 
        }
    }

    //当对象没有激活时也需要清理掉所有watcher
    void OnDisable()
    {
        ClearAllWatcher();
    }


    //当对象被销毁时需要注销掉对象身上的所有watcher
    void OnDestroy()
    {
        ClearAllWatcher();
    }


    //监听数据更新项
    void Update()
    { 
        //在Update中调用
        if(_dataHost!=null)
        {
            vm.Tick.next(); 
        }
    }
}
