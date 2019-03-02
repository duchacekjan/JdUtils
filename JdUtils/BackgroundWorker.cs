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
        
        private async Task ExecuteSafe(Func<Task> worker, Action success = null, Action<Exception> failure = null)
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
                Log?.Invoke(e.Message, e);
                PostToUi(() =>
                {
                    if (failure == null)
                    {
                        Error?.Invoke(e);
                    }
                    else
                    {
                        failure.Invoke(e);
                    }
                });
            }
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
