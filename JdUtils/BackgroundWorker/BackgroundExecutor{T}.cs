using JdUtils.BackgroundWorker.Interfaces;
using System;
using System.Windows.Threading;

namespace JdUtils.BackgroundWorker
{
    internal class BackgroundExecutor<TResult> : ABackgroundExecutor, IBackgroundExecutorFunction<TResult>
    {
        private Func<TResult> m_worker;
        private Action<TResult> m_success;

        private BackgroundExecutor(Dispatcher uiDispatcher)
            : base(uiDispatcher)
        {
        }

        public static IBackgroundExecutorFunction<T> Do<T>(Func<T> workFunction, Dispatcher uiDispatcher)
        {
            return new BackgroundExecutor<T>(uiDispatcher)
            {
                m_worker = workFunction
            };
        }

        public static IBackgroundExecutorFunction<TRes> Do<T, TRes>(Func<T, TRes> workFunction, T param, Dispatcher uiDispatcher)
        {
            var result = new BackgroundExecutor<TRes>(uiDispatcher);

            if (workFunction != null)
            {
                result.m_worker = () => workFunction.Invoke(param);
            }

            return result;
        }

        public IBackgroundExecutorCore OnSuccess(Action<TResult> successHandler)
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
