
using System.Collections.Generic;

namespace gcc.resloader
{
    /**
	 * 预制体动态加载工具
	 */
    public class ResLoader<T>
    {
        protected Dictionary<string, ResLoadNotifier<T>> loadMap = new Dictionary<string, ResLoadNotifier<T>>();
        public virtual ResLoadNotifier<T> getNotifier(string uri)
        {
            ResLoadNotifier<T> notifier;
            if (!this.loadMap.TryGetValue(uri,out notifier))
            {
                notifier = new ResLoadNotifier<T>();
                this.loadMap[uri] = notifier;
            }
            return notifier;
        }
        public virtual bool existNotifier(string uri)
        {
            var b = this.loadMap.ContainsKey(uri);
            return b;
        }
        public virtual void addNotifier(string uri, ResLoadNotifier<T> notifier)
        {
            this.loadMap[uri] = notifier;
        }
        public virtual void removeNotifier(string uri)
        {
            if (this.loadMap.ContainsKey(uri))
            {
                this.loadMap.Remove(uri);
            }
        }

        public virtual TOnProgress onLoadResProgress(string url, TOnProgress call)
        {
            var notifier = this.getNotifier(url);
            notifier.onProgress(call);
            return call;
        }

        public virtual void offLoadResProgress(string url, TOnProgress call)
        {
            var notifier = this.getNotifier(url);
            notifier.offProgress(call);
        }

    }
}
