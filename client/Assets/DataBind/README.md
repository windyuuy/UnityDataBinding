<h1 style="text-align:center;">数据绑定后端使用说明</h1>



### 接入方法：暂时略

### 使用方法：

使用方法分三步：

- 设置被观察者

```C#

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

```



- 设置观察者

```C#
// 需要引用DataBinding命名空间
using DataBinding;

// 需要实现 IStdHost 观察者接口，实现 IStdHost 后，该观察者也会同时成为可观察对象。
public class SampleHost:IStdHost
{
    // 可嵌套引用可观察对象类型
    // 注意：必须使用属性字段
    public SampleOB hello { get; set; }=new SampleOB();

    public double QQ { get; set; } = 234;

}

```



- 监听表达式

```C#

using UnityEngine;
using DataBinding;

public class Hello : MonoBehaviour
{
    public void Start()
    {
        Debug.Log("start");
        var sampleHost = new SampleHost();

        // 监听表达式
        sampleHost.Watch("QQ+hello.KKK", (host, value, oldValue) =>
        {
            Debug.Log("value changed;");
            Debug.Log(value);
        });
        sampleHost.QQ = 2134;
        sampleHost.hello.KKK = 3242;
        // 通知表达式值变化
        vm.Tick.next();

        // 监听表达式
        sampleHost.Watch("hello.IntList[2]", (host, value, oldValue) =>
        {
            Debug.Log("value changed;");
            Debug.Log(value);
        });
        sampleHost.hello.IntList[2] = 44;
        // 通知表达式值变化
        vm.Tick.next();

        // 监听表达式
        sampleHost.Watch("hello.NumDictionary[123]", (host, value, oldValue) =>
        {
            Debug.Log("value changed;");
            Debug.Log(value);
        });
        sampleHost.hello.NumDictionary[123] = "你变了";
        // 通知表达式值变化
        vm.Tick.next();

    }

    void Update()
    {
        
    }
}

```

