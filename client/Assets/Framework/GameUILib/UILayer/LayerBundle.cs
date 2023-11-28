
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RSG;

namespace gcc.layer
{
    using number = System.Double;

    using TLayerUri = System.String;
    using Node = UnityEngine.Transform;
    using boolean = System.Boolean;
    using Prefab = UnityEngine.GameObject;
    using TLayerBundleId = System.String;

    // using LayerBundleInputItem = TLayerUri | LayerBundle | TLayerBundleId;
    using LayerBundleItem = System.String;
    using LayerBundleMap = Dictionary<string, List<string>>;

    public class LayerBundleInputItem
    {
        public LayerBundleInputItem(string uid)
        {
            this._uid = uid;
        }
        public LayerBundleInputItem(LayerBundle item)
        {
            this._item = item;
            this._uid = item.Uid;
        }
        protected LayerBundle _item;
        protected string _uid;
        public string Uid
        {
            get
            {
                return this._uid;
            }
        }

        public static implicit operator LayerBundleInputItem(string item)
        {
            return new LayerBundleInputItem(item);
        }
        public static implicit operator LayerBundleInputItem(LayerBundle item)
        {
            return new LayerBundleInputItem(item);
        }

        public static implicit operator string(LayerBundleInputItem item)
        {
            return item.Uid;
        }

    }


    public class ShowBundleParams
    {
        public string Name;
        public object Data;
        // public ShowBundleParams(string name)
        // {
        // 	this.name = name;
        // 	this.data = null;
        // }
        public ShowBundleParams(string name, object data = null)
        {
            this.Name = name;
            this.Data = data;
        }
    }

    /**
	 * 管理图层约束
	 */
    public class LayerBundle
    {
        public const string DefaultBundleName = "default";

        public string Uid = "";

        public LayerMG LayerMG;

        protected LayerBundleMap layerBundleMap = new Dictionary<string, List<LayerBundleItem>>();


        public LayerBundle Init(LayerMG layerMG)
        {
            this.LayerMG = layerMG;
            return this;
        }

        /**
		 * 构建图层束
		 */
        public void SetupOneBundle(string name, LayerBundleInputItem[] items)
        {
            this.layerBundleMap[name] = items.Select(item => item.Uid).ToList();
        }

        /**
		 * 构建图层束
		 */
        public void SetupBundles(Dictionary<string, LayerBundleInputItem[]> map)
        {
            foreach (var pair in map)
            {
                this.SetupOneBundle(pair.Key, pair.Value);
            }
        }

        /**
		 * 获取bundle列表
		 * @param bundleName 
		 * @returns 
		 */
        public List<TLayerUri> GetBundle(string bundleName = DefaultBundleName)
        {
            List<TLayerUri> bundle;
            if (false == this.layerBundleMap.ContainsKey(bundleName))
            {
                bundle = new List<string>();
                this.layerBundleMap.Add(bundleName, bundle);
            }
            else
            {
                bundle = this.layerBundleMap[bundleName];
            }
            return bundle;
        }

        /// <summary>
        /// 展开层数
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public TLayerUri[] ExpandBundle(string bundleName = DefaultBundleName)
        {
            if (bundleName == null)
            {
                bundleName = DefaultBundleName;
            }
            var layers = new List<string>();
            this._foreachLayerBundleItems(bundleName, (layerUri) =>
            {
                layers.Add(layerUri);
            });
            return layers.Distinct().ToArray();
        }


        protected void _addBundleItem(string item, List<string> bundle)
        {
            if (!bundle.Contains(item))
            {
                bundle.Add(item);
            }
        }
        public void AddBundleItem(LayerBundleInputItem item, string bundleName = DefaultBundleName)
        {
            var bundle = this.GetBundle(bundleName);

            this._addBundleItem(item.Uid, bundle);
        }

        public void RemoveBundleItem(LayerBundleInputItem item, string bundleName = DefaultBundleName)
        {
            var bundle = this.GetBundle(bundleName);
            bundle.Remove(item.Uid);
        }

        /**
		 * 展开层束
		 */
        private void _foreachLayerBundleItems(string name, System.Action<TLayerUri> call)
        {
            if (this.layerBundleMap.ContainsKey(name))
            {
                var bundle = this.layerBundleMap[name];
                foreach (var subName in bundle)
                {
                    this._foreachLayerBundleItems(subName, call);
                }
            }
            else
            {
                call(name);
            }
        }

        /**
		 * 展开层束
		 */
        private List<Promise<T>> _foreachLayerBundleItems<T>(string name, System.Func<TLayerUri, Promise<T>> call, List<Promise<T>> ls)
        {
            if (this.layerBundleMap.ContainsKey(name))
            {
                var bundle = this.layerBundleMap[name];
                foreach (var subName in bundle.ToArray())
                {
                    this._foreachLayerBundleItems(subName, call, ls);
                }
            }
            else
            {
                var task = call(name);
                ls.Add(task);
            }
            return ls;
        }
        public IPromise<IEnumerable<T>> ForeachLayerBundleItems<T>(string name, System.Func<TLayerUri, Promise<T>> call)
        {
            var tasks = this._foreachLayerBundleItems(name, call, new List<Promise<T>>());
            var tasks1 = tasks as IEnumerable<IPromise<T>>;
            var task = Promise<T>.All(tasks1);
            return task;
        }

        public IPromise<IEnumerable<T>> ShowBundle<T>(string sp) where T : LayerModel
        {
            var bsp = new ShowBundleParams(sp);
            return this.ShowBundle<T>(bsp);
        }
        public IPromise<IEnumerable<T>> ShowBundle<T>(ShowBundleParams bsp) where T : LayerModel
        {
            return this.ForeachLayerBundleItems<T>(bsp.Name, (item) =>
            {
                var dsp = new ShowLayerParam(item, bsp.Data);
                return LayerMG.ShowLayer(dsp) as Promise<T>;
            });
        }

        public IPromise<IEnumerable<T>> CloseBundle<T>(string name) where T : LayerModel
        {
            return this.ForeachLayerBundleItems<T>(name, (item) =>
            {
                return LayerMG.CloseLayer(item) as Promise<T>;
            });
        }

        public IPromise<IEnumerable<LayerModel>> CloseBundle(string name)
        {
            return this.ForeachLayerBundleItems<LayerModel>(name, (item) =>
            {
                return LayerMG.CloseLayer(item) as Promise<LayerModel>;
            });
        }

        public IPromise<IEnumerable<LayerModel>> DestroyBundle(string name)
        {
            return this.ForeachLayerBundleItems<LayerModel>(name, (item) =>
            {
                return LayerMG.DestroyLayer(item) as Promise<LayerModel>;
            });
        }

        public IPromise<IEnumerable<T>> DestroyBundle<T>(string name) where T : LayerModel
        {
            return this.ForeachLayerBundleItems<T>(name, (item) =>
            {
                return LayerMG.DestroyLayer(item) as Promise<T>;
            });
        }

        // public IPromise<IEnumerable<T>> HideBundle<T>(string name) where T : LayerModel
        // {
        //     return this.ForeachLayerBundleItems<T>(name, (item) =>
        //     {
        //         return LayerMG.HideLayer(item) as Promise<T>;
        //     });
        // }

        public PPromise<IEnumerable<LayerModel>> PreloadBundle(string name)
        {
            return PreloadBundle<LayerModel>(name) as PPromise<IEnumerable<LayerModel>>;
        }
        number Max(number a, number b)
        {
            return a >= b ? a : b;
        }
        number Min(number a, number b)
        {
            return a <= b ? a : b;
        }
        public IPromise<IEnumerable<T>> PreloadBundle<T>(string name) where T : LayerModel
        {
            var uris = this.GetBundle(name);
            var urls = uris.Select(uri => LayerUriUtil.WrapUri(uri));
            long totalMin = 0;
            var p0=new Promise(async (resolve,reject)=>{
                var tasks=urls.ToArray().Select(url=>UnityEngine.AddressableAssets.Addressables.GetDownloadSizeAsync(url).Task);
                var sizes=await Task.WhenAll(tasks);
                totalMin=sizes.Aggregate((total,size)=>{
                    if (size == 0)
                    {
                        total += 1;
                    }
                    else
                    {
                        total += size;
                    }
                    return total;
                });
                resolve();
            });
            // PPromise<LayerModel>[]
            var ls1 = new List<Promise<LayerModel>>();
            var ls2 = this._foreachLayerBundleItems<LayerModel>(name, (item) =>
            {
                var ppromise = LayerMG.PreloadLayer(item);
                return ppromise;
            }, ls1);
            var ls = ls2;

            var promise0 = p0.Then(()=>Promise<LayerModel>.All(ls));
            var promise = PPromise.toPPromise(promise0);
            number count = 0;
            number total = 0;
            foreach (var p in ls)
            {
                if (p is IPPromise ppromise)
                {
                    ppromise.onProgress((c, t, diff, tdiff, isFirst) =>
                    {
                        count += diff;
                        total += tdiff;

                        promise.notifyProgress(count, Max(count, totalMin));
                    });
                }
            }

            promise.IsWithProgress = true;
            return promise as IPromise<IEnumerable<T>>;
        }

        public IPromise<IEnumerable<T>> CreateBundleItems<T>(string name) where T : LayerModel
        {
            return this.ForeachLayerBundleItems<T>(name, (item) =>
            {
                return LayerMG.CreateLayer(item) as Promise<T>;
            });
        }

        /// <summary>
        /// 当前正在录制的LayerBundle
        /// </summary>
        public string RecordBundleName;
        public void SetRecordBundle(string name)
        {
            this.RecordBundleName = name;
        }
        public List<TLayerUri> GetRecordBundle()
        {
            if (false == string.IsNullOrEmpty(this.RecordBundleName))
            {
                return this.GetBundle(this.RecordBundleName);
            }
            return null;
        }
        public void AddRecordItem(LayerBundleInputItem item)
        {
            if (false == string.IsNullOrEmpty(this.RecordBundleName))
            {
                this.AddBundleItem(item, this.RecordBundleName);
            }
        }
        public IPromise<IEnumerable<T>> CloseRecordBundle<T>() where T : LayerModel
        {
            if (false == string.IsNullOrEmpty(this.RecordBundleName))
            {
                return this.CloseBundle<T>(this.RecordBundleName);
            }

            return Promise<IEnumerable<T>>.Resolved(new T[0]);
        }

    }
}
