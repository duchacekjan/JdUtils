using System;

namespace JdUtils.BackgroundWorker.Interfaces
{
    public interface IBackgroundExecutorInstance
    {
        /// <summary>
        /// Akce, která se má provést na pozadí
        /// </summary>
        /// <param name="workAction">Akce</param>
        /// <returns></returns>
        IBackgroundExecutorAction Do(Action workAction);

        /// <summary>
        /// Akce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="T">Typ parametru akce</typeparam>
        /// <param name="workAction">Akce</param>
        /// <param name="param">Parametr akce</param>
        /// <returns></returns>
        IBackgroundExecutorAction Do<T>(Action<T> workAction, T param);

        /// <summary>
        /// Funkce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="TResult">Návratový typ funkce</typeparam>
        /// <param name="workFunction">Funkce</param>
        /// <returns></returns>
        IBackgroundExecutorFunction<TResult> Do<TResult>(Func<TResult> workFunction);

        /// <summary>
        /// Funkce, která se má provést na pozadí
        /// </summary>
        /// <typeparam name="T">Typ parametru funkce</typeparam>
        /// <typeparam name="TResult">Návratový typ funkce</typeparam>
        /// <param name="workFunction">Funkce</param>
        /// <param name="param">Parametr funkce</param>
        /// <returns></returns>
        IBackgroundExecutorFunction<TResult> Do<T, TResult>(Func<T, TResult> workFunction, T param);
    }
}
