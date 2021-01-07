using JdUtils.Delegates;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using resx = JdUtils.Resources.Resources;

namespace JdUtils.BackgroundWorker
{
    /// <summary>
    /// Společný předek pro background workery
    /// </summary>
    public abstract class ABackgroundWorker
    {
        protected ABackgroundWorker(Dispatcher uiDispatcher)
        {
            if (uiDispatcher == null)
            {
                throw new ArgumentNullException(nameof(uiDispatcher));
            }

            if (uiDispatcher.Thread.GetApartmentState() != ApartmentState.STA)
            {
                throw new ArgumentOutOfRangeException(nameof(uiDispatcher), resx.BackgroundWorkerDispatherNotSTA);
            }

            Dispatcher = uiDispatcher;
        }

        /// <summary>
        /// Event vyvolán, pokud se objeví neošetřená chyba
        /// </summary>
        public event ExceptionHandler UnhandledError;

        /// <summary>
        /// Event vyvolán při chybě
        /// </summary>
        public event ExceptionHandler LogError;

        /// <summary>
        /// UI dispatcher
        /// </summary>
        protected Dispatcher Dispatcher { get; }

        /// <summary>
        /// Vyvolá akci na UI vlákně pomoci předaného <paramref name="dispatcher"/>
        /// </summary>
        /// <param name="dispatcher">UI dispatcher</param>
        /// <param name="action">Akce</param>
        public static void PostToUi(Dispatcher dispatcher, Action action)
        {
            try
            {
                if (dispatcher?.CheckAccess() == true)
                {
                    action?.Invoke();
                }
                else
                {
                    dispatcher?.Invoke(action);
                }
            }
            catch (OperationCanceledException)
            {
                ;//INFO Silent handling task cancellation
            }
        }

        /// <summary>
        /// Vyvolá akci na UI vlákně pomoci předaného <paramref name="dispatcher"/>
        /// </summary>
        /// <param name="dispatcher">UI dispatcher</param>
        /// <param name="action">Akce</param>
        /// <param name="parameter">Parametr akce</param>
        public static void PostToUi<T>(Dispatcher dispatcher, Action<T> action, T parameter)
        {
            void Wrapper()
            {
                action?.Invoke(parameter);
            }

            PostToUi(dispatcher, Wrapper);
        }

        /// <summary>
        /// Vyvolá akci na UI vlákně
        /// </summary>
        /// <param name="action">Akce</param>
        /// <param name="parameter">Parametr akce</param>
        public void PostToUi<T>(Action<T> action, T parameter)
        {
            PostToUi(Dispatcher, action, parameter);
        }

        /// <summary>
        /// Vyvolá akci na UI vlákně
        /// </summary>
        /// <param name="action">Akce</param>
        public void PostToUi(Action action)
        {
            PostToUi(Dispatcher, action);
        }

        /// <summary>
        /// Provede akci <paramref name="worker"/> na pozadí. Akce je obalena v
        /// try/catch klauzuli. Pokud <paramref name="delay"/> je větší než 0 je vyvoláno <see cref="Task.Delay(int)"/>,
        /// jinak je vyvoláno <see cref="Task.CompletedTask"/> před spuštěním akce
        /// </summary>
        /// <param name="worker">Akce, která má být provedena na pozadí</param>
        /// <param name="failure">Custom error handler</param>
        /// <param name="delay">Delay v milisekundách</param>
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
        /// Ošetření chyby. Metoda vyvolá <see cref="LogError"/> event
        /// a pokud není definován (<paramref name="failure"/>), vyvolá event <see cref="UnhandledError"/>
        /// </summary>
        /// <param name="exception">Výjimka, která nastala</param>
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
