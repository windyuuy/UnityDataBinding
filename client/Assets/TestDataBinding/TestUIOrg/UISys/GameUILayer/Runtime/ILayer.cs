
public interface ILayerLifeCycle
{
	void onOpened();
	void onOpening();
	void onClosed();
	void onClosing();
	void onShow();
	void onHide();
	void onExposed();
	void onShield();
	void onBeforeClosed();
}

public interface IUILayer : ILayerLifeCycle
{

}

public interface ILayerDelegate : ILayerLifeCycle
{

}
