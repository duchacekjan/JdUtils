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

        public event FailureHandler Error;

        public event LogHandler Log;

        public void Execute(Action action, Action success = null, FailureHandler failure = null)
        {
            ExecuteSafe(Worker(action, success), failure);
        }

        public void Execute<T>(Func<T> action, Action<T> success = null, FailureHandler failure = null)
        {
            ExecuteSafe(Worker(action, success), failure);
        }

        public void Execute<T, TResult>(Func<T, TResult> action, T param, Action<TResult> success = null, FailureHandler failure = null)
        {
            ExecuteSafe(Worker(action, param, success), failure);
        }

        private Action Worker<T, TResult>(Func<T, TResult> worker, T param, Action<TResult> success)
        {
            return () =>
            {
                var result = worker.Invoke(param);
                PostToUi(() =>
                {
                    success?.Invoke(result);
                });
            };
        }

        private Action Worker<T>(Func<T> worker, Action<T> success)
        {
            return () =>
            {
                var result = worker.Invoke();
                PostToUi(() =>
                {
                    success?.Invoke(result);
                });
            };
        }

        private Action Worker(Action worker, Action success)
        {
            return () =>
            {
                worker.Invoke();
                PostToUi(success);
            };
        }
        
        private void ExecuteSafe(Action worker, FailureHandler failure, int delay = 0)
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

        private void OnException(Exception exception, FailureHandler failure)
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
