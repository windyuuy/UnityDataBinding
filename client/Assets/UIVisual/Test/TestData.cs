using Unity.VisualScripting;
using DataBinding;

[AutoFieldProperty]
public class TestData2: IStdHost
{
    public int testkey2  = 342;
}

[Inspectable]
[AutoFieldProperty]
public class TestData: IStdHost
{
    public string testkey = "nonekey";

    public System.Func<object, object, object> testfunc  =(object a,object b) =>
    {
        return TestData.Inst.data2.testkey2;
    };

    public TestData2 data2 =new TestData2();

    public static TestData Inst=new TestData();
}

[StdHost]
[AutoFieldProperty]
public class TestData3
{
    public string testkey  = "nonekey3";
}
