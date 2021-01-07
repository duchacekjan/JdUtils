using System;

namespace JdUtils.BackgroundWorker.Interfaces
{
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
