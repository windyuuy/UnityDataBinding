using System;
using System.Threading.Tasks;
using gcc.layer;

public class UILayerLifecycleDelegate : ILayerLifecycleInnerCall
{
	public UILayer Layer;

	public void __callOnEnter()
	{
		Layer.__callOnEnter();
	}

	public void __callDoClose(Action resolve, Action<Exception> reject)
	{
		Layer.__callDoClose(resolve,reject);
	}

	public void __callDoOpen(Action finished, Action<Exception> reject)
	{
		Layer.__callDoOpen(finished,reject);
	}

	public void __callOnExposed()
	{
		Layer.__callOnExposed();
	}

	public void __callOnShield()
	{
		Layer.__callOnShield();
	}

	public void __callOnCreate(object data = null)
	{
		Layer.onCreate(data);
	}

	public Task __callOnOpening()
	{
		return Layer.__callOnOpening();
	}

	public void __callOnOpened()
	{
		Layer.__callOnOpened();
	}

	public Task __callOnShow()
	{
		return Layer.__callOnShow();
	}

	public Task __callOnOverlapShow(ShowLayerParam paras)
	{
		return Layer.__callOnOverlapShow(paras);
	}

	public Task __callOnHide()
	{
		return Layer.__callOnHide();
	}

	public Task __callOnClosing()
	{
		return Layer.__callOnClosing();
	}

	public void __callOnBeforeClosed()
	{
		Layer.__callOnBeforeClosed();
	}

	public void __callOnClosed()
	{
		Layer.__callOnClosed();
	}

	public void __callOnBeforeDestroy()
	{
		Layer.__callOnBeforeDestroy();
	}

	public void __callOnCoverChanged()
	{
		Layer.__callOnCoverChanged();
	}

	public void __callOnAnyFocusChanged(bool focus)
	{
		Layer.__callOnAnyFocusChanged(focus);
	}

	public void __callOnBeforeRelease()
	{
		Layer.__callOnBeforeRelease();
	}
}
