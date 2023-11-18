

namespace DataBinding.UIBind
{
	public class CCContainerItem : CCSubDataHub
	{
		protected override ISubDataHub subDataHub { get; set; } = new ContainerItem();
		public ContainerItem ContainerItem
		{
			get
			{
				return (ContainerItem)this.subDataHub;
			}
			set
			{
				this.subDataHub = value;
			}
		}

	}
}
