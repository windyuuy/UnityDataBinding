
// 需要引用DataBinding命名空间
using DataBinding;
// 目前只支持监听DataBinding.CollectionExt中的容器类对象，不支持系统内置容器对象（如：System.Collection.Generic.List、System.Collection.Generic.Dictionary）。
using DataBinding.CollectionExt;

// 需要添加Observable特性，使目标成为可观察对象
[Observable]
public class SampleOB
{
    // 注意：必须使用属性字段
    public double KKK { get; set; } = 234;

    // DataBinding.CollectionExt.List 容器数据
    // 注意：必须使用属性字段
    public List<int> IntList { get; set; } = new List<int> { 1, 2, 3, 4 };

    // DataBinding.CollectionExt.Dictionary 容器数据
    // 注意：必须使用属性字段
    public Dictionary<double, string> NumDictionary { get; set; } = new Dictionary<double, string>();
}
