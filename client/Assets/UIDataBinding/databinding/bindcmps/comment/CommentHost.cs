
// 需要引用DataBinding命名空间
using DataBinding;
using UnityEngine;

// 需要实现 IStdHost 观察者接口
// 这个好像是不能直接当做组件的
public class CommentHost:IStdHost
{
    // 可嵌套引用可观察对象类型
    // 注意：必须使用属性字段

    public  CommentOB dataObject { get; set; } = new CommentOB();
}
