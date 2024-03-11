

namespace DataBinding.UIBind
{
	public class CCContainerItem : CCSubDataHub
	{
		protected override ISubDataHub SubDataHub { get; set; } = new ContainerItem();
		public ContainerItem ContainerItem
		{
			get
			{
				return (ContainerItem)this.SubDataHub;
			}
			set
			{
				this.SubDataHub = value;
			}
		}

	}
}
