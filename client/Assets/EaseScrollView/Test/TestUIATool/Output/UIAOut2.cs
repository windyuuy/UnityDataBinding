using DataBinding.CollectionExt;
using Action = System.Action;

namespace TestingCode
{
	public class UIAOut2: DataBinding.IStdHost
	{
		/// <note>
		/// env::C1
		/// </note>
		public TC1_Ele[] C1 {get;set;}
		public class TC1_Ele
		{
			/// <usecase>
			/// C2<br/>
			/// </usecase>
			public TC1_Ele.TC2_Ele[] C2 {get;set;}
			public class TC2_Ele
			{
				/// <usecase>
				/// QQ<br/>
				/// </usecase>
				public object QQ {get;set;}
				/// <usecase>
				/// pp<br/>
				/// </usecase>
				public object pp {get;set;}
			}
		}
	}
}
