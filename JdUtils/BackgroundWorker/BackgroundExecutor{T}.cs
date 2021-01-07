using JdUtils.BackgroundWorker.Interfaces;
using System;
using System.Windows.Threading;

namespace JdUtils.BackgroundWorker
{
    /// <summary>
    /// FluentApi volání práce na pozadí
    /// </summary>
    /// <typeparam name="TResult">Návratový typ funckce pracující na pozadí</typeparam>
    internal class BackgroundExecutor<TResult> : ABackgroundExecutor, IBackgroundExecutorFunction<TResult>
    {
        private Func<TResult> m_worker;
        private Action<TResult> m_success;

        private BackgroundExecutor(Dispatcher uiDispatcher)
            : base(uiDispatcher)
        {
        }

        /// <summary>
        /// Funkce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="TResult">Návratový typ funkce</typeparam>
        /// <param name="workFunction">Funkce</param>
        /// <param name="uiDispatcher">UI dispatcher z STA vlákna</param>
        /// <returns></returns>
        public static IBackgroundExecutorFunction<T> Do<T>(Func<T> workFunction, Dispatcher uiDispatcher)
        {
            return new BackgroundExecutor<T>(uiDispatcher)
            {
                m_worker = workFunction
            };
        }

        /// <summary>
        /// Funkce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="TResult">Návratový typ funkce</typeparam>
        /// <param name="workFunction">Funkce</param>
        /// <param name="uiDispatcher">UI dispatcher z STA vlákna</param>
        /// <returns></returns>
        public static IBackgroundExecutorFunction<TRes> Do<T, TRes>(Func<T, TRes> workFunction, T param, Dispatcher uiDispatcher)
        {
            var result = new BackgroundExecutor<TRes>(uiDispatcher);

            if (workFunction != null)
            {
                result.m_worker = () => workFunction.Invoke(param);
            }

            return result;
        }

        /// <summary>
        /// Akce vyvolána po úspěšném dokončení akce na pozadí
        /// </summary>
        /// <param name="successHandler"></param>
        /// <returns></returns>
        public IBackgroundExecutorCore OnSuccess(Action<TResult> successHandler)
        {
            m_success = successHandler;
            return this;
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
                var result = m_worker.Invoke();
                PostToUi(m_success, result);
            };
        }
    }
}
