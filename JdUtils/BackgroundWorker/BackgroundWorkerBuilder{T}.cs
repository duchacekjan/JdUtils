using System;
using System.Windows.Threading;

namespace JdUtils.BackgroundWorker
{
    public class BackgroundWorkerBuilder<TResult> : ABackgroundWorkerBuilder
    {
        private Func<TResult> m_worker;
        private Action<TResult> m_success;

        private BackgroundWorkerBuilder(Dispatcher uiDispatcher)
            : base(uiDispatcher)
        {
        }

        /// <summary>
        /// Executes data on background with defined delay. Values 0 or less means no delay
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public new BackgroundWorkerBuilder<TResult> WithDelay(int delay)
        {
            return (BackgroundWorkerBuilder<TResult>)base.WithDelay(delay);
        }

        public static BackgroundWorkerBuilder<T> Do<T>(Func<T> workFunction, Dispatcher uiDispatcher)
        {
            return new BackgroundWorkerBuilder<T>(uiDispatcher)
            {
                m_worker = workFunction
            };
        }

        public static BackgroundWorkerBuilder<TRes> Do<T, TRes>(Func<T, TRes> workFunction, T param, Dispatcher uiDispatcher)
        {
            var result = new BackgroundWorkerBuilder<TRes>(uiDispatcher);

            if (workFunction != null)
            {
                result.m_worker = () => workFunction.Invoke(param);
            }

            return result;
        }

        public BackgroundWorkerBuilder<TResult> OnSuccess(Action<TResult> successHandler)
        {
            m_success = successHandler;
            return this;
        }

        protected override Action GetWorker()
        {
            return () =>
            {
                var result = m_worker.Invoke();
                PostToUi(m_success, result);
            };
        }
    }
}
