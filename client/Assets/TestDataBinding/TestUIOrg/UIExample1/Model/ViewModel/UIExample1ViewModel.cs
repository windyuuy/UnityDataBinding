using DataBinding;
using UnityEngine;
using number = System.Single;
using Action = System.Action;

namespace TestingCode
{
	public class UserData : IStdHost
	{
		public string Name = "lkwje";

		public static UserData Inst = new();
	}
	public class UIExample1ViewModel : IStdHost
	{
		/// <note>
		/// env::ItemIcon
		/// </note>
		public string ItemIcon { get; set; }

		/// <note>
		/// env::Ttile
		/// </note>
		public string Title => UserData.Inst.Name;

		/// <note>
		/// env::OnClick
		/// </note>
		// public Action OnClick { get; set; }= ()=>{
		// 	Debug.Log("lkwje");
		// };
		public void OnClick()
		{
			Debug.Log("OnClick");
			UserData.Inst.Name = "title2";
		}
}
}
