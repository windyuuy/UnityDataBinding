

using UnityEngine;

namespace DataBind.UIBind
{
	[AddComponentMenu("DataDrive/ContainerItem")]
	public class ContainerItemComp : SubDataHubComp
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
