using System;

namespace JdUtils.BackgroundWorker.Interfaces
{
    /// <summary>
    /// Rozhraní pro výsledek funkce zpracovávané na pozadí
    /// </summary>
    /// <typeparam name="TResult">Návratový typ funkce zpracovávané na pozadí</typeparam>
    public interface IBackgroundExecutorFunction<TResult> : IBackgroundExecutorCore
    {
        /// <summary>
        /// Akce vyvolána po úspěšném dokončení akce na pozadí
        /// </summary>
        /// <param name="successHandler"></param>
        /// <returns></returns>
        IBackgroundExecutorCore OnSuccess(Action<TResult> successHandler);
    }
}
