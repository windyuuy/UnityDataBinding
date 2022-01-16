
namespace DataBinding { 

	public interface IObservableAttrs
	{
		event vm.PropertyChangedEventHandler PropertyChanged
		{
			add
			{
				((vm.IObservableEvents)this).PropertyChanged += value;
			}
			remove
			{
				((vm.IObservableEvents)this).PropertyChanged -= value;
			}
		}
		event vm.PropertyGetEventHandler PropertyGot
		{
			add
			{
				((vm.IObservableEvents)this).PropertyGot += value;
			}
			remove
			{
				((vm.IObservableEvents)this).PropertyGot -= value;
			}
		}
	}

	public interface IHostAttrs
	{
		vm.IFullHost Host=>(vm.IFullHost)this;
	}
}
