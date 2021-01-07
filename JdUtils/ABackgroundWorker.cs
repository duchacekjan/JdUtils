using JdUtils.Delegates;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace JdUtils
{
    public abstract class ABackgroundWorker
    {
        /// <summary>
        /// Constructor. Creates new instance of <see cref="BackgroundWorker"/>
        /// </summary>
        /// <param name="uiDispatcher"><see cref="Dispatcher"/> from UI thread</param>
        protected ABackgroundWorker(Dispatcher uiDispatcher)
        {
            if (uiDispatcher == null)
            {
                throw new ArgumentNullException(nameof(uiDispatcher));
            }

            if (uiDispatcher.Thread.GetApartmentState() != ApartmentState.STA)
            {
                throw new ArgumentOutOfRangeException(nameof(uiDispatcher), "Dispatcher is not from STA thread");
            }

            Dispatcher = uiDispatcher;
        }

        /// <summary>
        /// Event invoked when unhandled error occured
        /// </summary>
        public event ExceptionHandler UnhandledError;

        /// <summary>
        /// Event invoked when error occured
        /// </summary>
        public event ExceptionHandler LogError;

        protected Dispatcher Dispatcher { get; }

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
                if (Dispatcher.CheckAccess())
                {
                    action?.Invoke();
                }
                else
                {
                    Dispatcher.Invoke(action);
                }
            }
            catch (OperationCanceledException)
            {
                ;//INFO Silent handling task cancellation
            }
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
        protected void ExecuteSafe(Action worker, ExceptionHandler failure, int delay = 0)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (delay > 0)
                    {
                        await Task.Delay(delay);
                    }
                    else
                    {
                        await Task.CompletedTask;
                    }

                    worker?.Invoke();
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
