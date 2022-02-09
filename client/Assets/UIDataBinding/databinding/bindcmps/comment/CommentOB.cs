
using System;
// 需要引用DataBinding命名空间
using DataBinding;
// 目前只支持监听DataBinding.CollectionExt中的容器类对象，不支持系统内置容器对象（如：System.Collection.Generic.List、System.Collection.Generic.Dictionary）。
using DataBinding.CollectionExt;
using UnityEngine.Events;

// 需要添加Observable特性，使目标成为可观察对象
[Observable]
public class CommentOB
{
	//图片的路径
	public string  imgepath { get; set; }
    //名称
    public string  textname { get; set; }
    //年龄
    public int  age { get; set; }
    //进度
    public float progress{ get; set; }
    //按钮的回掉函数
    public  UnityAction callBack{ get; set; }
    //是否选中
    public  bool isSelect{ get; set; }
    //列表容器数据结构
    public List<myData> intListData { get; set; }
    //字典容器[容器嵌套容器]
    public Dictionary<int, List<myData>> listDictionary { get; set; }
}