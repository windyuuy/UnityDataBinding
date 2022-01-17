
using UnityEngine;
using DataBinding;

public class Hello : MonoBehaviour
{
    public void Start()
    {
        Debug.Log("start");
        var sampleHost = new SampleHost();

        // �������ʽ
        sampleHost.Watch("QQ+hello.KKK", (host, value, oldValue) =>
        {
            Debug.Log("value changed;");
            Debug.Log(value);
        });
        sampleHost.QQ = 2134;
        sampleHost.hello.KKK = 3242;
        // ֪ͨ���ʽֵ�仯
        vm.Tick.next();

        // �������ʽ
        sampleHost.Watch("hello.IntList[2]", (host, value, oldValue) =>
        {
            Debug.Log("value changed;");
            Debug.Log(value);
        });
        sampleHost.hello.IntList[2] = 44;
        // ֪ͨ���ʽֵ�仯
        vm.Tick.next();

        // �������ʽ
        sampleHost.Watch("hello.NumDictionary[123]", (host, value, oldValue) =>
        {
            Debug.Log("value changed;");
            Debug.Log(value);
        });
        sampleHost.hello.NumDictionary[123] = "�����";
        // ֪ͨ���ʽֵ�仯
        vm.Tick.next();

    }

    void Update()
    {
        
    }
}
