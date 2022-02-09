
using UnityEngine;

//这个是属于数据绑定DataHost组件负责
//负责挂载在UI的根节点上

public class DataHostCompent:MonoBehaviour
{
    // 可嵌套引用可观察对象类型
    // 注意：必须使用属性字段

    public  CommentHost dataHost { get; set; } = new CommentHost();

    public void registerData(CommentOB data)
    {
        dataHost.dataObject = data;
    }
}
