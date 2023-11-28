
using System;
using RSG;

namespace gcc.layer
{
    using number = System.Double;
    public delegate void PPromiseProgress(number count, number total, number diff, number totalDiff, bool isFirst);

    public interface IPPromise
    {
        bool IsWithProgress { get; set; }
        number count { get; }
        number total { get; }

        bool isFirstTimeUpdateProgress { get; }
        number progress { get; }
        bool supportProgressAttr { get; }
        PPromiseProgress onProgress(PPromiseProgress call);
        PPromiseProgress onProgress(Action<number, number> call);
        void offProgress(PPromiseProgress key);
        void notifyProgress(number count, number total);
    }

    public partial class PPromise<T> : Promise<T>, IPPromise
    {
        public bool IsWithProgress { get; set; }
        public number count { get; protected set; }

        public number total { get; protected set; }

        public bool isFirstTimeUpdateProgress { get; protected set; }
        public virtual number progress
        {
            get
            {
                if (this.total == 0) { return 0; }
                return this.count / this.total;
            }
        }
        public bool supportProgressAttr { get; protected set; } = true;
        public PPromise(Action<Action<T>, Action<Exception>> executor) : base(executor)
        {
            this.isFirstTimeUpdateProgress = true;
            this.count = 0;
            this.total = 0;
        }

        protected PPromiseProgress calls;
        public virtual PPromiseProgress onProgress(PPromiseProgress call)
        {
            this.calls += call;
            return call;
        }
        public virtual PPromiseProgress onProgress(Action<number, number> call)
        {
            PPromiseProgress call1 = (count, total, diff, totalDiff, isFirst) =>
            {
                call(count, total);
            };
            this.calls += call1;
            return call1;
        }
        public virtual void offProgress(PPromiseProgress key)
        {
            this.calls -= key;
        }
        public virtual void notifyProgress(number count, number total)
        {
            var isFirst = this.isFirstTimeUpdateProgress;

            if (isFirst)
            {
                this.isFirstTimeUpdateProgress = false;
            }
            var diff = count - this.count;

            var totalDiff = total - this.total;

            this.count = count;

            this.total = total;

            this.calls?.Invoke(count, total, diff, totalDiff, isFirst);
        }
    }
}
