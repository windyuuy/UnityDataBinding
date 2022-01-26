

namespace UI.DataBinding
{
	public class CCContainerItem : CCSubDataHub
	{
		protected override ISubDataHub _subDataHub { get; set; } = new ContainerItem();
		public ContainerItem containerItem
		{
			get
			{
				return (ContainerItem)this._subDataHub;
			}
			set
			{
				this._subDataHub = value;
			}
		}

	}
}
