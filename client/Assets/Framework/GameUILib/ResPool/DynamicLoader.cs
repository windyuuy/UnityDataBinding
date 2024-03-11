
/**
 * 目标：
 * - 支持失败的资源重新加载生效
 * - 对于异步加载成功的节点，支持坐标更改应用
 */

namespace gcc.resloader
{
    using System.Linq.MyExt;
    using Error = System.Exception;
    using System.Collections.Generic;

    public interface IResLoadListener<T>
    {
        void onLoad(ResCallback<T> call);
        void onError(ErrorCallback call);
        T getRes();
    }
    public interface IResReleaseListener<T>
    {
        void onUnload(ResCallback<T> call);
        void onUnloadError(ErrorCallback call);
    }

    public delegate void ResCallback<T>(T res);
    public delegate void ErrorCallback(Error res);
    public delegate void TOnProgress(float count, float total);

    /**
	 * 资源加载通知
	 */
    public class ResLoadNotifier<T> : IResLoadListener<T>, IResReleaseListener<T>
    {
        /**
		 * 加载结束, 不一定成功
		 */
        public bool isFinished = false;
        /**
			 * 资源加载结束, 并且成功
		 */
        public bool isLoaded = false;
        public Error err;
        public Error unloadErr;
        protected T res;
        protected List<ResCallback<T>> onLoadList = new List<ResCallback<T>>();
        public virtual bool isResReady
        {
            get
            {
                return this.isFinished && this.isLoaded;
            }
        }
        public T getRes()
        {
            if (this.isResReady)
            {
                return this.res;
            }
            return default(T);
        }

        public bool isUnloaded
        {
            get
            {
                return !(this.isLoaded || this.isLoading);
            }
        }
        protected void unload()
        {
            this.isFinished = false;
            this.isLoaded = false;
            this.err = null;
            this.unloadErr = null;
        }
        protected List<ResCallback<T>> onUnloadList = new List<ResCallback<T>>();
        public void onUnload(ResCallback<T> call)
        {
            if (this.isUnloaded)
            {
                if (this.unloadErr == null)
                {
                    call(this.res!);
                }
            }
            else
            {
                this.onLoadList.Add(call);
            }
        }
        public void notifyOnUnload(T res)
        {
            this.unload();
            var ls = this.onUnloadList.ToArray();
            this.clearUnloadListeners();
            ls.ForEach((call) =>
            {
                call(res);
            });
            this.onUnloadList.Clear();
        }

        protected List<ErrorCallback> onUnloadErrorList = new List<ErrorCallback>();
        public void onUnloadError(ErrorCallback call)
        {
            if (this.isUnloaded)
            {
                if (this.unloadErr != null)
                {
                    call(this.unloadErr!);
                }
            }
            else
            {
                this.onUnloadErrorList.Add(call);
            }
        }
        public void notifyOnUnloadError(Error err)
        {
            this.unloadErr = err;
            var ls = this.onUnloadErrorList.ToArray();
            this.clearUnloadListeners();
            ls.ForEach((call) =>
            {
                call(err);
            });
        }

        protected void clearUnloadListeners()
        {
            this.onUnloadList.Clear();
            this.onUnloadErrorList.Clear();
        }

        public void onLoad(ResCallback<T> call)
        {

            if (this.isFinished && this.isLoaded)
            {
                call(this.res!);
            }
            else
            {
                this.onLoadList.Add(call);
            }
        }
        public void notifyOnLoad(T res)
        {
            if (this.isLoaded)
            {
                return;
            }

            this.isLoading = false;
            this.isFinished = true;
            this.isLoaded = true;
            this.res = res;
            this.err = null;
            var ls = this.onLoadList.ToArray();
            this.clearLoadListeners();
            ls.ForEach((call) =>
            {
                call(res);
            });
        }

        protected float count = 0;
        protected float total = 0.1f;
        protected bool isProgressChanged = false;
        protected List<TOnProgress> onProgressList = new List<TOnProgress>();
        public void notifyOnPrgress(float count, float total)
        {
            this.isProgressChanged = true;
            this.onProgressList.ToArray().ForEach((call) =>
            {
                call(count, total);
            });
        }
        public void onProgress(TOnProgress call)
        {
            this.onProgressList.Add(call);
            if (this.isLoaded || this.isProgressChanged)
            {
                call(this.count, this.total);
            }
        }
        public void offProgress(TOnProgress call)
        {
            var index = this.onProgressList.IndexOf(call);
            if (index >= 0)
            {
                this.onProgressList.RemoveAt(index);
            }
        }

        protected List<ErrorCallback> onErrorList = new List<ErrorCallback>();
        public void onError(ErrorCallback call)
        {

            if (this.isFinished && (!this.isLoaded))
            {
                call(this.err!);
            }
            else
            {
                this.onErrorList.Add(call);
            }
        }
        public void notifyOnError(Error err)
        {
            this.isLoading = false;
            this.isFinished = true;
            this.isLoaded = false;
            this.err = err;
            var ls = this.onErrorList.ToArray();
            this.clearLoadListeners();
            ls.ForEach((call) =>
            {
                call(err);
            });
        }

        protected void clearLoadListeners()
        {
            this.onErrorList.Clear();
            this.onLoadList.Clear();
            this.onProgressList.Clear();
        }

        public bool isLoading = false;
        public void notifyOnLoading()
        {
            this.isLoading = true;
        }
    }

}
