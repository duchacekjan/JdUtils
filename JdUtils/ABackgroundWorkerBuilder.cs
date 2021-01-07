using JdUtils.Delegates;
using System;
using System.Windows.Threading;

namespace JdUtils
{
    public abstract class ABackgroundWorkerBuilder : ABackgroundWorker
    {
        protected ABackgroundWorkerBuilder(Dispatcher uiDispatcher)
            : base(uiDispatcher)
        {
        }

        protected ABackgroundWorkerBuilder(ABackgroundWorkerBuilder builder)
            : base(builder.Dispatcher)
        {
        }

        protected int Delay { get; set; }

        protected ExceptionHandler Failure { get; set; }

        /// <summary>
        /// Runs execution
        /// </summary>
        public void Execute()
        {
            ExecuteSafe(GetWorker(), Failure, Delay);
        }

        /// <summary>
        /// Executes data on background with defined delay. Values 0 or less means no delay
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public ABackgroundWorker WithDelay(int delay)
        {
            Delay = delay;
            return this;
        }

        /// <summary>
        /// Action when execution fails. When not assigned, <see cref="UnhandledError"/> is invoked.
        /// </summary>
        /// <param name="failureHandler"></param>
        /// <returns></returns>
        public ABackgroundWorker OnError(ExceptionHandler failureHandler)
        {
            Failure = failureHandler;
            return this;
        }

        protected abstract Action GetWorker();
    }
}
