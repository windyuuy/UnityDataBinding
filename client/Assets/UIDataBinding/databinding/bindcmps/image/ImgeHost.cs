﻿// 需要引用DataBinding命名空间
using DataBinding;

// 需要实现 IStdHost 观察者接口
public class ImgeHost:IStdHost
{
    // 可嵌套引用可观察对象类型
    // 注意：必须使用属性字段
    public ImgeOB imge { get; set; } = new ImgeOB(); 

}
