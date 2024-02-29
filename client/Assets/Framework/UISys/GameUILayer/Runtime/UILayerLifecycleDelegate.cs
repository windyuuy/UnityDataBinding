using System;
using System.Threading.Tasks;
using gcc.layer;

namespace UISys.Runtime
{
	public class UILayerLifecycleDelegate : ILayerLifecycleInnerCall
	{
		public UILayer Layer;

		public Task __callOnCreate(object data = null)
		{
			return Layer.__callOnCreate();
		}

		public Task __callOnPrepare()
		{
			return Layer.__callOnPrepare();
		}

		public void __callOnReady()
		{
			Layer.__callOnReady();
		}

		public Task __callOnOpening()
		{
			return Layer.__callOnOpening();
		}

		public void __callOnOpened()
		{
			Layer.__callOnOpened();
		}

		public void __callOnExpose()
		{
			Layer.__callOnExpose();
		}

		public void __callOnShield()
		{
			Layer.__callOnShield();
		}

		public Task __callOnClosing()
		{
			return Layer.__callOnClosing();
		}

		public void __callOnDispose()
		{
			Layer.__callOnDispose();
		}

		public void __callOnClosed()
		{
			Layer.__callOnClosed();
		}
	}
}

