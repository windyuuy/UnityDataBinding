
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
		vm.Tick.Next();

		// 监听表达式
		sampleHost.Watch("hello.IntList[2]", (host, value, oldValue) =>
		{
			Debug.Log("value changed;");
			Debug.Log(value);
		});
		sampleHost.hello.IntList[2] = 44;
		// 通知表达式值变化
		vm.Tick.Next();

		// 监听表达式
		sampleHost.Watch("hello.NumDictionary[123]", (host, value, oldValue) =>
		{
			Debug.Log("value changed;");
			Debug.Log(value);

			GameObject.Find("GameObject/Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = $"{value}";
		});
		sampleHost.hello.NumDictionary[123] = "你变了";
		// 通知表达式值变化
		vm.Tick.Next();

	}

	void Update()
	{

	}
}
