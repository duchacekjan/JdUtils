using System;
using System.Windows.Threading;
using JdUtils.Delegates;

namespace JdUtils.BackgroundWorker
{
    /// <summary>
    /// Utility for work on background thread
    /// </summary>
    public class BackgroundWorker : ABackgroundWorker
    {
        /// <summary>
        /// Constructor. Creates new instance of <see cref="BackgroundWorker"/>
        /// </summary>
        /// <param name="uiDispatcher"><see cref="Dispatcher"/> from UI thread</param>
        public BackgroundWorker(Dispatcher uiDispatcher)
            : base(uiDispatcher)
        {
        }

        /// <summary>
        /// Function executes <paramref name="action"/> on background thread. When function is finished successfully, then
        /// is executed <paramref name="success"/> on UI thread, otherwise is executed <paramref name="failure"/> method.
        /// If failure method is not specified, then is invoked <see cref="UnhandledError"/> event
        /// </summary>
        /// <param name="action">Action to be executed on background thread</param>
        /// <param name="success">Action to be executed on UI thread</param>
        /// <param name="failure">Custom error handler</param>
        public void Execute(Action action, Action success = null, ExceptionHandler failure = null)
        {
            ExecuteSafe(Worker(action, success), failure);
        }

        /// <summary>
        /// Function executes <paramref name="action"/> on background thread. When function is finished successfully, then
        /// is executed <paramref name="success"/> on UI thread, otherwise is executed <paramref name="failure"/> method.
        /// If failure method is not specified, then is invoked <see cref="UnhandledError"/> event
        /// </summary>
        /// <typeparam name="T">Type of parameter of action</typeparam>
        /// <param name="action">Action to be executed on background thread</param>
        /// <param name="param">Parameter of action</param>
        /// <param name="success">Action to be executed on UI thread</param>
        /// <param name="failure">Custom error handler</param>
        public void Execute<T>(Action<T> action, T param, Action success = null, ExceptionHandler failure = null)
        {
            ExecuteSafe(Worker(action, param, success), failure);
        }

        /// <summary>
        /// Function executes <paramref name="action"/> on background thread. When function is finished successfully, then
        /// is executed <paramref name="success"/> on UI thread, otherwise is executed <paramref name="failure"/> method.
        /// If failure method is not specified, then is invoked <see cref="UnhandledError"/> event
        /// </summary>
        /// <typeparam name="T">Type of function result</typeparam>
        /// <param name="action">Function to be executed on background thread</param>
        /// <param name="success">Action to be executed on UI thread</param>
        /// <param name="failure">Custom error handler</param>
        public void Execute<T>(Func<T> action, Action<T> success = null, ExceptionHandler failure = null)
        {
            ExecuteSafe(Worker(action, success), failure);
        }

        /// <summary>
        /// Function executes <paramref name="action"/> on background thread. When function is finished successfully, then
        /// is executed <paramref name="success"/> on UI thread, otherwise is executed <paramref name="failure"/> method.
        /// If failure method is not specified, then is invoked <see cref="UnhandledError"/> event
        /// </summary>
        /// <typeparam name="T">Type of parameter</typeparam>
        /// <typeparam name="TResult">Type of function result</typeparam>
        /// <param name="action">Function to be executed on background thread</param>
        /// <param name="param">Input parameter</param>
        /// <param name="success">Action to be executed on UI thread</param>
        /// <param name="failure">Custom error handler</param>
        public void Execute<T, TResult>(Func<T, TResult> action, T param, Action<TResult> success = null, ExceptionHandler failure = null)
        {
            ExecuteSafe(Worker(action, param, success), failure);
        }

        /// <summary>
        /// Function waits on background thread and then executes selected action
        /// </summary>
        /// <param name="action">Delayed action</param>
        /// <param name="delay">Requested delay</param>
        /// <param name="failure">Custom error handler</param>
        public void WaitAndExecute(Action action, int delay, ExceptionHandler failure = null)
        {
            ExecuteSafe(action, failure, delay);
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
        private Action Worker<T, TResult>(Func<T, TResult> worker, T param, Action<TResult> success)
        {
            return () =>
            {
                var result = worker.Invoke(param);
                PostToUi(success, result);
            };
        }

        /// <summary>
        /// Creates wrapping action to handle <paramref name="worker"/> on background thread and
        /// then synchronize <paramref name="success"/> on UI thread
        /// </summary>
        /// <typeparam name="T">Type of returning value</typeparam>
        /// <param name="worker">Function to be executed on background thread</param>
        /// <param name="success">Action to be executed on UI thread, when <paramref name="worker"/> action is successful</param>
        /// <returns></returns>
        private Action Worker<T>(Func<T> worker, Action<T> success)
        {
            return () =>
            {
                var result = worker.Invoke();
                PostToUi(success, result);
            };
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
        private Action Worker<T>(Action<T> worker, T param, Action success)
        {
            return () =>
            {
                worker.Invoke(param);
                PostToUi(success);
            };
        }

        /// <summary>
        /// Creates wrapping action to handle <paramref name="worker"/> on background thread and
        /// then synchronize <paramref name="success"/> on UI thread
        /// </summary>
        /// <param name="worker">Action to be executed on background thread</param>
        /// <param name="success">Action to be executed on UI thread, when <paramref name="worker"/> action is successful</param>
        /// <returns></returns>
        private Action Worker(Action worker, Action success)
        {
            return () =>
            {
                worker.Invoke();
                PostToUi(success);
            };
        }
    }
}
