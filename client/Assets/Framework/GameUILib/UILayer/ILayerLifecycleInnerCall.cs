
using System;
using System.Threading.Tasks;

namespace gcc.layer
{
	/**
	 * 内部调用的对话框接口
	 */
    public interface ILayerLifecycleInnerCall
    {
	    /**
		 * 初次创建调用
		 */
	    Task __callOnCreate();

	    Task __callOnPrepare(object data);

	    void __callOnReady();

	    Task __callOnOpening();

	    /**
		 * 关闭调用
		 */
	    void __callOnOpened();

	    /**
		 * 每次由隐藏变为显示调用
		 */
	    void __callOnExpose();

	    /**
		 * 每次由显示变为隐藏调用
		 */
	    void __callOnShield();

	    /**
		 * 开始关闭调用
		 */
	    Task __callOnClosing();

	    /**
		 * 关闭调用
		 */
	    void __callOnDispose();

	    /**
		 * 关闭调用
		 */
	    void __callOnClosed();

    }
    
}
