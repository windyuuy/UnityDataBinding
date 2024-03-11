using DataBinding;
using UnityEngine;

namespace TestingCode
{
	public class UserData : IStdHost
	{
		public static UserData Inst = new();
		public string Name { get; set; } = "wkjjkewf";

		public UserData()
		{
			// 必须实现基础方法，实现外部监听
			VM.Utils.ImplementStdHost(this);
		}

	}
	public class UIExample1ViewModel : IStdHost
	{
		// public UserData UserData { get; set; } = new();

		/// <note>
		/// env::ItemIcon
		/// </note>
		public string ItemIcon { get; set; }

		/// <note>
		/// env::Ttile
		/// </note>
		public string Title
		{
			get
			{
				return UserData.Inst.Name;
			}
		}

		/// <note>
		/// env::OnClick
		/// </note>
		// public Action OnClick { get; set; }= ()=>{
		// 	Debug.Log("lkwje");
		// };
		public void OnClick()
		{
			Debug.Log("OnClick");
			UserData.Inst.Name = "hello2";
		}
}
}
