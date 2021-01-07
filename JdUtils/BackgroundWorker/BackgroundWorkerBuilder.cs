using System;
using System.Windows.Threading;

namespace JdUtils.BackgroundWorker
{
    /// <summary>
    /// Builder pro FluentApi volání práce na pozadí
    /// </summary>
    public class BackgroundWorkerBuilder : ABackgroundWorkerBuilder
    {
        private Action m_worker;
        private Action m_success;

        /// <summary>
        /// Konstruktor. Vytvoří novou instanci <see cref="BackgroundWorkerBuilder"/>.
        /// </summary>
        /// <param name="uiDispatcher">UI dispatcher z STA vlákna</param>
        private BackgroundWorkerBuilder(Dispatcher uiDispatcher)
            : base(uiDispatcher)
        {
        }

        /// <summary>
        /// Akce, která se má provést na pozadí
        /// </summary>
        /// <param name="workAction">Akce</param>
        /// <returns></returns>
        public static BackgroundWorkerBuilder Do(Action workAction, Dispatcher uiDispatcher)
        {
            var result = new BackgroundWorkerBuilder(uiDispatcher)
            {
                Failure = null,
                m_success = null,
                Delay = 0,
                m_worker = workAction
            };
            return result;
        }

        /// <summary>
        /// Akce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="T">Typ parametru akce</typeparam>
        /// <param name="workAction">Akce</param>
        /// <param name="param">Parametr akce</param>
        /// <returns></returns>
        public static BackgroundWorkerBuilder Do<T>(Action<T> workAction, T param, Dispatcher uiDispatcher)
        {
            return Do(() => workAction?.Invoke(param), uiDispatcher);
        }

        /// <summary>
        /// Funkce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="TResult">Návratový typ funkce</typeparam>
        /// <param name="workFunction">Funkce</param>
        /// <returns></returns>
        public static BackgroundWorkerBuilder<TResult> Do<TResult>(Func<TResult> workFunction, Dispatcher uiDispatcher)
        {
            return BackgroundWorkerBuilder<TResult>.Do(workFunction, uiDispatcher);
        }

        /// <summary>
        /// Funkce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="T">Typ parametru funkce</typeparam>
        /// <typeparam name="TResult">Návratový typ funkce</typeparam>
        /// <param name="workFunction">Funkce</param>
        /// <param name="param">Parametr funkce</param>
        /// <returns></returns>
        public static BackgroundWorkerBuilder<TResult> Do<T, TResult>(Func<T, TResult> workFunction, T param, Dispatcher uiDispatcher)
        {
            return BackgroundWorkerBuilder<TResult>.Do(workFunction, param, uiDispatcher);
        }

        /// <summary>
        /// Akce vyvolána po úspěšném dokončení akce na pozadí
        /// </summary>
        /// <param name="successHandler"></param>
        /// <returns></returns>
        public BackgroundWorkerBuilder OnSuccess(Action successHandler)
        {
            m_success = successHandler;
            return this;
        }

        /// <summary>
        /// Provede na pozadí pouze delay a poté akci, která měla být vyvolána na pozadí,
        /// provede na UI vlákně. Metoda definována v <see cref="OnSuccess(Action)"/> bude ignorována
        /// </summary>
        /// <param name="delay">Delay v milisekundách</param>
        /// <returns></returns>
        public BackgroundWorkerBuilder AfterDelay(int delay = 0)
        {
            m_success = m_worker;
            m_worker = null;
            Delay = delay;
            return this;
        }

        /// <summary>
        /// Definice Delay před spuštěním akce na pozadí. Pokud je <paramref name="delay"/> větší než 0,
        /// použije se <see cref="System.Threading.Tasks.Task.Delay(int)"/>,
        /// jinak <see cref="System.Threading.Tasks.Task.CompletedTask"/>
        /// </summary>
        /// <param name="delay">Delay v milisekundách</param>
        /// <returns></returns>
        public new BackgroundWorkerBuilder WithDelay(int delay)
        {
            return (BackgroundWorkerBuilder)base.WithDelay(delay);
        }

        /// <summary>
        /// Akce, která má být vyvolána na pozadí. Spojuje volání akce na pozadí a volání
        /// výsledku zpět na UI vlákně
        /// </summary>
        /// <returns></returns>
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
