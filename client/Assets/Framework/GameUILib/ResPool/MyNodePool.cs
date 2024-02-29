
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using gcc.resloader;
using System;

namespace gcc.respool
{
    using number = System.Double;
    using Node = UnityEngine.Transform;
    using Prefab = UnityEngine.GameObject;
    using Error = System.Exception;

    public class MyNodePool
    {
        // static MyNodePool()
        // {
        //     LoomMG.Init();
        // }
        public static TOnProgress OnProgress(string url, TOnProgress call)
        {
            return TCCResLoader.Inst.onLoadResProgress(url, call);
        }

        public static void OffProgress(string url, TOnProgress call)
        {
            TCCResLoader.Inst.offLoadResProgress(url, call);
        }

        // TODO: make sure loading order
        public static void Load(string url,Node parent, System.Action<Node, Error> call)
        {
            var notifier = TCCResLoader.Inst.getNotifier(url);
            // TODO: 支持添加不可用父节点
            var loading = Addressables.InstantiateAsync(url,parent);
           Action<AsyncOperationHandle<Prefab>> MyCompleted=null;
           MyCompleted= (op) =>
            {
                // Debug.Log($"load-UI-Then: {url}");
                loading.Completed-=MyCompleted;
                
                if (op.Status==AsyncOperationStatus.Succeeded)
                {
                    if (op.Result == null)
                    {
                        var err = new System.Exception("load asset instance failed: " + url);
                        call(null, err);
                        notifier.notifyOnError(err);
                    }
                    else
                    {
                        LoomMG.SharedLoom.RunTask(() =>
                        {
                            var inst = op.Result;

                            // notify progress
                            {
                                var status = loading.GetDownloadStatus();
                                if (status.TotalBytes == 0)
                                {
                                    notifier.notifyOnPrgress(status.Percent, 1);
                                }
                                else
                                {
                                    notifier.notifyOnPrgress(status.DownloadedBytes, status.TotalBytes);
                                }
                            }

                            try
                            {
                                call(inst.transform, null);
                            }
                            catch (Error e)
                            {
                                Debug.LogError("uncaught exception handling layer: " + url);
                                Debug.LogError(e);
                                call(null, e);
                            }
                            notifier.notifyOnLoad(inst);
                        });
                    }
                }
                else
                {
                    call(null, op.OperationException);
                    notifier.notifyOnError(op.OperationException);
                }
            };
            loading.Completed+=MyCompleted;
            // Debug.Log($"load-UI: {url}");
        }

        // TODO: make sure loading order
        public static Task<Node> LoadAsync(string url,Node parent)
        {
            var ts = new TaskCompletionSource<Node>();
            Load(url,parent, (node, err) =>
            {
                if (err != null)
                {
                    ts.SetException(err);
                }
                else
                {
                    ts.SetResult(node);
                }
            });

            return ts.Task;
        }

        public static void DestroyNode(Node node)
        {
            Addressables.ReleaseInstance(node.gameObject);
        }

        public static void Put(Node node, bool close)
        {
            Addressables.ReleaseInstance(node.gameObject);
        }

        public static Prefab GetPrefab(string url)
        {
            return TCCResLoader.Inst.getNotifier(url).getRes();
        }

        public static bool TryReleaseAsset(string url)
        {
            try
            {
                Addressables.Release(url);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            return false;
        }
    }
}
