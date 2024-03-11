using Action = System.Action;

namespace TestingCode
{
	public class UIAOut1: DataBinding.IStdHost
	{
		/// <note>
		/// env::enabled
		/// </note>
		public bool enabled {get;set;}
		/// <note>
		/// env::gray
		/// </note>
		public object gray {get;set;}
		/// <note>
		/// env::doClick
		/// </note>
		public Action doClick {get;set;}
		/// <note>
		/// env::label
		/// </note>
		public string label {get;set;}
		/// <note>
		/// env::spriteUrl
		/// </note>
		public string spriteUrl {get;set;}
		/// <note>
		/// env::visible
		/// </note>
		public bool visible {get;set;}
		/// <note>
		/// env::progress
		/// </note>
		public float progress {get;set;}
		/// <note>
		/// env::isToggleCheck
		/// </note>
		public bool isToggleCheck {get;set;}
	}
}
