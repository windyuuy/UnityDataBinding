
using RSG;
using System;

namespace gcc.layer
{
    public partial class PPromise
    {
        public static PPromise<T2> createPPromise<T2>(Action<Action<T2>, Action<Exception>> executor)
        {
            return new PPromise<T2>(executor);
        }

        public static PPromise<T2> toPPromise<T2>(IPromise<T2> promise)
        {
            return new PPromise<T2>((resolve, reject) =>
            {
                promise.Then(resolve, reject);
            });
        }
    }
}
