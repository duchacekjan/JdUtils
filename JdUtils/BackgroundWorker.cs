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

        public void Execute(Action action, Action success = null, ErrorHandler failure = null)
        {
            ExecuteSafe(Worker(action, success), failure);
        }

        private Action Worker(Action worker, Action success)
        {
            return () => 
            { 
                worker.Invoke();
                PostToUi(success);
            };
        }

        private async Task ExecuteSafe<T, TResult>(Func<T, Task<TResult>> worker, Action<TResult> success, ErrorHandler failure, T args = default(T))
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

        private async Task ExecuteSafe<T>(Func<Task<T>> worker, Action<T> success, ErrorHandler failure)
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

        private void ExecuteSafe(Action worker, ErrorHandler failure, int delay = 0)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (delay == 0)
                    {
                        await Task.CompletedTask;
                    }
                    else
                    {
                        await Task.Delay(delay);
                    }

                    worker.Invoke();
                }
                catch (Exception e)
                {
                    OnException(e, failure);
                }
            });
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
