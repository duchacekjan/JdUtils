using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using JdUtils.Delegates;

namespace JdUtils
{
    public class BackgroundWorker
    {
        private readonly Dispatcher m_uiDispatcher;

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

        public event ErrorHandler Error;

        public event LogHandler Log;

        private async Task WaitWorker(int delay)
        {
            await Task.Delay(delay);
        }

        private async Task WorkerWrapper(Action action)
        {
            await Task.CompletedTask;
            action.Invoke();
        }

        private async Task ExecuteSafe<T, TResult>(Func<T, Task<TResult>> worker, Action<TResult> success = null, ErrorHandler failure = null, T args = default(T))
        {
            try
            {
                var result = await worker.Invoke(args);
                PostToUi(() =>
                {
                    success?.Invoke(result);
                });
            }
            catch (Exception e)
            {
                OnException(e, failure);
            }
        }

        private async Task ExecuteSafe<T>(Func<Task<T>> worker, Action<T> success = null, ErrorHandler failure = null)
        {
            try
            {
                var result = await worker.Invoke();
                PostToUi(() =>
                {
                    success?.Invoke(result);
                });
            }
            catch (Exception e)
            {
                OnException(e, failure);
            }
        }

        private async Task ExecuteSafe(Func<Task> worker, Action success = null, ErrorHandler failure = null)
        {
            try
            {
                await worker.Invoke();
                PostToUi(() =>
                {
                    success?.Invoke();
                });
            }
            catch (Exception e)
            {
                OnException(e, failure);
            }
        }

        private void OnException(Exception exception, ErrorHandler failure)
        {
            Log?.Invoke(exception.Message, exception);
            PostToUi(() =>
            {
                if (failure == null)
                {
                    Error?.Invoke(exception);
                }
                else
                {
                    failure.Invoke(exception);
                }
            });
        }
        
        private void PostToUi(Action action)
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
    }
}
