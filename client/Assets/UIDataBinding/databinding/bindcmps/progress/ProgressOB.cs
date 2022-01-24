
// 需要引用DataBinding命名空间
using DataBinding;
// 目前只支持监听DataBinding.CollectionExt中的容器类对象，不支持系统内置容器对象（如：System.Collection.Generic.List、System.Collection.Generic.Dictionary）。
using DataBinding.CollectionExt;

// 需要添加Observable特性，使目标成为可观察对象
[Observable]
public class ProgressOB
{
	//是否被选中
	public float  value { get; set; }

}
