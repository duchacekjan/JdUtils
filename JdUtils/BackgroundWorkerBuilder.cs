using JdUtils.Delegates;
using System;
using System.Windows.Threading;

namespace JdUtils
{
    public class BackgroundWorkerBuilder : ABackgroundWorker
    {
        private Action m_worker;
        private ExceptionHandler m_failure;
        private int m_delay;

        public BackgroundWorkerBuilder(Dispatcher uiDispatcher)
            : base(uiDispatcher)
        {
        }

        private Action Worker
        {
            get => m_worker;
            set => SetWorker(value);
        }

        private void SetWorker(Action value)
        {
            m_delay = 0;
            m_failure = null;
            m_worker = value;
        }

        /// <summary>
        /// Runs execution
        /// </summary>
        public void Execute()
        {
            ExecuteSafe(Worker, m_failure, m_delay);
        }

        /// <summary>
        /// Executes data on background with defined delay. Values 0 or less means no delay
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public BackgroundWorkerBuilder WithDelay(int delay)
        {
            m_delay = delay;
            return this;
        }

        /// <summary>
        /// Action when execution fails. When not assigned, <see cref="UnhandledError"/> is invoked.
        /// </summary>
        /// <param name="failure"></param>
        /// <returns></returns>
        public BackgroundWorkerBuilder OnException(ExceptionHandler failure)
        {
            m_failure = failure;
            return this;
        }

        /// <summary>
        /// Creates wrapping action to handle <paramref name="worker"/> on background thread and
        /// then synchronize <paramref name="success"/> on UI thread
        /// </summary>
        /// <typeparam name="T">Type of function parameter</typeparam>
        /// <typeparam name="TResult">Type of returning value</typeparam>
        /// <param name="worker">Function to be executed on background thread</param>
        /// <param name="param">Parameter of function</param>
        /// <param name="success">Action to be executed on UI thread, when <paramref name="worker"/> action is successful</param>
        /// <returns></returns>
        public BackgroundWorkerBuilder DoWork<T, TResult>(Func<T, TResult> worker, T param, Action<TResult> success)
        {
            Worker = () =>
            {
                var result = worker.Invoke(param);
                PostToUi(success, result);
            };

            return this;
        }

        /// <summary>
        /// Creates wrapping action to handle <paramref name="worker"/> on background thread and
        /// then synchronize <paramref name="success"/> on UI thread
        /// </summary>
        /// <typeparam name="T">Type of returning value</typeparam>
        /// <param name="worker">Function to be executed on background thread</param>
        /// <param name="success">Action to be executed on UI thread, when <paramref name="worker"/> action is successful</param>
        /// <returns></returns>
        public BackgroundWorkerBuilder DoWork<T>(Func<T> worker, Action<T> success)
        {
            Worker = () =>
            {
                var result = worker.Invoke();
                PostToUi(success, result);
            };

            return this;
        }

        /// <summary>
        /// Creates wrapping action to handle <paramref name="worker"/> on background thread and
        /// then synchronize <paramref name="success"/> on UI thread
        /// </summary>
        /// <typeparam name="T">Type of returning value</typeparam>
        /// <param name="worker">Function to be executed on background thread</param>
        /// <param name="param">Parameter of action</param>
        /// <param name="success">Action to be executed on UI thread, when <paramref name="worker"/> action is successful</param>
        /// <returns></returns>
        public BackgroundWorkerBuilder DoWork<T>(Action<T> worker, T param, Action success)
        {
            Worker = () =>
            {
                worker.Invoke(param);
                PostToUi(success);
            };

            return this;
        }

        /// <summary>
        /// Creates wrapping action to handle <paramref name="worker"/> on background thread and
        /// then synchronize <paramref name="success"/> on UI thread
        /// </summary>
        /// <param name="worker">Action to be executed on background thread</param>
        /// <param name="success">Action to be executed on UI thread, when <paramref name="worker"/> action is successful</param>
        /// <returns></returns>
        public BackgroundWorkerBuilder DoWork(Action worker, Action success)
        {
            Worker = () =>
            {
                worker.Invoke();
                PostToUi(success);
            };

            return this;
        }

        /// <summary>
        /// Calls action on UI thread
        /// </summary>
        /// <param name="success">Action to be executed back on UI thread</param>
        /// <returns></returns>
        public BackgroundWorkerBuilder AfterDelay(Action success)
        {
            Worker = () =>
            {
                PostToUi(success);
            };

            return this;
        }
    }
}
