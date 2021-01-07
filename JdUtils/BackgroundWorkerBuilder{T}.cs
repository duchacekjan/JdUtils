using System;

namespace JdUtils
{
    public class BackgroundWorkerBuilder<TResult> : ABackgroundWorkerBuilder
    {
        private Func<TResult> m_worker;
        private Action<TResult> m_success;

        internal BackgroundWorkerBuilder(ABackgroundWorkerBuilder builder)
            : base(builder)
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

        public BackgroundWorkerBuilder<TResult> Do(Func<TResult> workFunction)
        {
            m_worker = workFunction;
            return this;
        }

        public BackgroundWorkerBuilder<TResult> Do<T>(Func<T, TResult> workFunction, T param)
        {
            if (workFunction != null)
            {
                m_worker = () => workFunction.Invoke(param);
            }
            return this;
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
