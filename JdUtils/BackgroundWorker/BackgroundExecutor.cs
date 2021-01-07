using JdUtils.BackgroundWorker.Interfaces;
using System;
using System.Windows.Threading;

namespace JdUtils.BackgroundWorker
{
    /// <summary>
    /// FluentApi volání práce na pozadí
    /// </summary>
    public class BackgroundExecutor : ABackgroundExecutor, IBackgroundExecutorInstance, IBackgroundExecutorAction
    {
        private Action m_worker;
        private Action m_success;

        /// <summary>
        /// Konstruktor. Vytvoří novou instanci <see cref="BackgroundExecutor"/>.
        /// </summary>
        /// <param name="uiDispatcher">UI dispatcher z STA vlákna</param>
        private BackgroundExecutor(Dispatcher uiDispatcher)
            : base(uiDispatcher)
        {
        }

        /// <summary>
        /// Vrátí instanci executora
        /// </summary>
        /// <param name="uiDispatcher">UI dispatcher z STA vlákna</param>
        /// <returns></returns>
        public static IBackgroundExecutorInstance Instance(Dispatcher uiDispatcher)
        {
            return new BackgroundExecutor(uiDispatcher);
        }

        /// <summary>
        /// Akce, která se má provést na pozadí
        /// </summary>
        /// <param name="workAction">Akce</param>
        /// <param name="uiDispatcher">UI dispatcher z STA vlákna</param>
        /// <returns></returns>
        public static IBackgroundExecutorAction Do(Action workAction, Dispatcher uiDispatcher)
        {
            var result = Instance(uiDispatcher);
            return result.Do(workAction);
        }

        /// <summary>
        /// Akce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="T">Typ parametru akce</typeparam>
        /// <param name="workAction">Akce</param>
        /// <param name="param">Parametr akce</param>
        /// <param name="uiDispatcher">UI dispatcher z STA vlákna</param>
        /// <returns></returns>
        public static IBackgroundExecutorAction Do<T>(Action<T> workAction, T param, Dispatcher uiDispatcher)
        {
            var result = Instance(uiDispatcher);
            return result.Do(workAction, param);
        }

        /// <summary>
        /// Funkce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="TResult">Návratový typ funkce</typeparam>
        /// <param name="workFunction">Funkce</param>
        /// <param name="uiDispatcher">UI dispatcher z STA vlákna</param>
        /// <returns></returns>
        public static IBackgroundExecutorFunction<TResult> Do<TResult>(Func<TResult> workFunction, Dispatcher uiDispatcher)
        {
            return BackgroundExecutor<TResult>.Do(workFunction, uiDispatcher);
        }

        /// <summary>
        /// Funkce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="T">Typ parametru funkce</typeparam>
        /// <typeparam name="TResult">Návratový typ funkce</typeparam>
        /// <param name="workFunction">Funkce</param>
        /// <param name="param">Parametr funkce</param>
        /// <param name="uiDispatcher">UI dispatcher z STA vlákna</param>
        /// <returns></returns>
        public static IBackgroundExecutorFunction<TResult> Do<T, TResult>(Func<T, TResult> workFunction, T param, Dispatcher uiDispatcher)
        {
            return BackgroundExecutor<TResult>.Do(workFunction, param, uiDispatcher);
        }

        /// <summary>
        /// Akce, která se má provést na pozadí
        /// </summary>
        /// <param name="workAction">Akce</param>
        /// <returns></returns>
        public IBackgroundExecutorAction Do(Action workAction)
        {
            Failure = null;
            m_success = null;
            Delay = 0;
            m_worker = workAction;
            return this;
        }

        /// <summary>
        /// Akce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="T">Typ parametru akce</typeparam>
        /// <param name="workAction">Akce</param>
        /// <param name="param">Parametr akce</param>
        /// <returns></returns>
        public IBackgroundExecutorAction Do<T>(Action<T> workAction, T param)
        {
            return Do(() => workAction?.Invoke(param));
        }

        /// <summary>
        /// Funkce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="TResult">Návratový typ funkce</typeparam>
        /// <param name="workFunction">Funkce</param>
        /// <returns></returns>
        public IBackgroundExecutorFunction<TResult> Do<TResult>(Func<TResult> workFunction)
        {
            return Do(workFunction, Dispatcher);
        }

        /// <summary>
        /// Funkce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="T">Typ parametru funkce</typeparam>
        /// <typeparam name="TResult">Návratový typ funkce</typeparam>
        /// <param name="workFunction">Funkce</param>
        /// <param name="param">Parametr funkce</param>
        /// <returns></returns>
        public IBackgroundExecutorFunction<TResult> Do<T, TResult>(Func<T, TResult> workFunction, T param)
        {
            return Do(workFunction, param, Dispatcher);
        }

        /// <summary>
        /// Akce vyvolána po úspěšném dokončení akce na pozadí
        /// </summary>
        /// <param name="successHandler"></param>
        /// <returns></returns>
        public IBackgroundExecutorCore OnSuccess(Action successHandler)
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
        public IBackgroundExecutorCore AfterDelay(int delay = 0)
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
        public new BackgroundExecutor WithDelay(int delay)
        {
            return (BackgroundExecutor)base.WithDelay(delay);
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
