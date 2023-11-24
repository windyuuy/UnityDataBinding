using number = System.Single;
using Action = System.Action;

namespace TestingCode
{
	public class UIAOut4: vm.Host
	{
		/// <note>
		/// env::AV2
		/// </note>
		public object AV2 {get;set;}
		/// <note>
		/// env::C1
		/// </note>
		public TC1 C1 {get;set;} = new TC1();
		public class TC1
		{
			/// <usecase>
			/// C1.AV2<br/>
			/// </usecase>
			public object AV2 {get;set;}
		}
	}
}
