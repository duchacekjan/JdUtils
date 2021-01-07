using System;
using System.Windows.Threading;

namespace JdUtils
{
    public class BackgroundWorkerBuilder : ABackgroundWorkerBuilder
    {
        private Action m_worker;
        private Action m_success;

        public BackgroundWorkerBuilder(Dispatcher uiDispatcher)
            : base(uiDispatcher)
        {
        }

        public BackgroundWorkerBuilder Do(Action workAction)
        {
            Failure = null;
            m_success = null;
            Delay = 0;
            m_worker = workAction;
            return this;
        }

        public BackgroundWorkerBuilder Do<T>(Action<T> workAction, T param)
        {
            return Do(() => workAction?.Invoke(param));
        }

        public BackgroundWorkerBuilder<TResult> Do<T, TResult>(Func<TResult> workFunction)
        {
            return new BackgroundWorkerBuilder<TResult>(this)
                .Do(workFunction);
        }

        public BackgroundWorkerBuilder<TResult> Do<T, TResult>(Func<T, TResult> workFunction, T param)
        {
            return new BackgroundWorkerBuilder<TResult>(this)
                .Do(workFunction, param);
        }

        public BackgroundWorkerBuilder OnSuccess(Action successHandler)
        {
            m_success = successHandler;
            return this;
        }

        public BackgroundWorkerBuilder AfterDelay(int delay = 0)
        {
            m_success = m_worker;
            m_worker = null;
            Delay = delay;
            return this;
        }

        /// <summary>
        /// Executes data on background with defined delay. Values 0 or less means no delay
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public new BackgroundWorkerBuilder WithDelay(int delay)
        {
            return (BackgroundWorkerBuilder)base.WithDelay(delay);
        }

        protected override Action GetWorker()
        {
            return () =>
            {
                m_worker?.Invoke();
                PostToUi(m_success);
            };
        }
    }
}
