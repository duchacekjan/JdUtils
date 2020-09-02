using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using JdUtils.Delegates;

namespace JdUtils
{
    /// <summary>
    /// Utility for work on background thread
    /// </summary>
    public class BackgroundWorker
    {
        private readonly Dispatcher m_uiDispatcher;

        /// <summary>
        /// Constructor. Creates new instance of <see cref="BackgroundWorker"/>
        /// </summary>
        /// <param name="uiDispatcher"><see cref="Dispatcher"/> from UI thread</param>
        public BackgroundWorker(Dispatcher uiDispatcher)
        {
            if (uiDispatcher == null)
            {
                throw new ArgumentNullException(nameof(uiDispatcher));
            }

            if (uiDispatcher.Thread.GetApartmentState() != ApartmentState.STA)
            {
                throw new ArgumentOutOfRangeException(nameof(uiDispatcher), "Dispatcher is not from STA thread");
            }

            m_uiDispatcher = uiDispatcher;
        }

        /// <summary>
        /// Event invoked when unhandled error occured
        /// </summary>
        public event ExceptionHandler UnhandledError;

        /// <summary>
        /// Event invoked when error occured
        /// </summary>
        public event ExceptionHandler LogError;

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
        /// Invokes action on UI thread
        /// </summary>
        /// <param name="action"></param>
        /// <param name="parameter"></param>
        public void PostToUi<T>(Action<T> action, T parameter)
        {
            void Wrapper()
            {
                action?.Invoke(parameter);
            }

            PostToUi(Wrapper);
        }

        /// <summary>
        /// Invokes action on UI thread
        /// </summary>
        /// <param name="action"></param>
        public void PostToUi(Action action)
        {
            try
            {
                if (m_uiDispatcher.CheckAccess())
                {
                    action?.Invoke();
                }
                else
                {
                    m_uiDispatcher.Invoke(action);
                }
            }
            catch(OperationCanceledException)
            {
                ;//INFO Silent handling task cancellation
            }
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

        /// <summary>
        /// Executes action <paramref name="worker"/> on background thread wrapped in
        /// try/catch clause. If <paramref name="delay"/> is 0 is invoked Task.CompletedTask,
        /// otherwise is invoked Task.Delay
        /// </summary>
        /// <param name="worker">Action to be executed on background thread</param>
        /// <param name="failure">Custom error handler</param>
        /// <param name="delay">Delay in milliseconds</param>
        /// <exception cref="ArgumentOutOfRangeException">Negative delay</exception>
        private void ExecuteSafe(Action worker, ExceptionHandler failure, int delay = 0)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (delay == 0)
                    {
                        await Task.CompletedTask;
                    }
                    else if (delay > 0)
                    {
                        await Task.Delay(delay);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(delay), "Delay cannot be negative");
                    }

                    worker.Invoke();
                }
                catch (Exception e)
                {
                    OnException(e, failure);
                }
            });
        }

        /// <summary>
        /// Hadling error on background thread. Method invokes <see cref="LogError"/> event
        /// and if is not defined custom handler (<paramref name="failure"/>), invokes <see cref="UnhandledError"/>
        /// </summary>
        /// <param name="exception">Occured exception</param>
        /// <param name="failure">Custom handler</param>
        private void OnException(Exception exception, ExceptionHandler failure)
        {
            LogError?.Invoke(exception);
            if (failure != null)
            {
                PostToUi(failure.Invoke, exception);
            }
            else if (UnhandledError != null)
            {
                PostToUi(UnhandledError.Invoke, exception);
            }
        }
    }
}
