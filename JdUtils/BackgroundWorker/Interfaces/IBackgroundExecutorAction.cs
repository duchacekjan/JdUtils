using System;

namespace JdUtils.BackgroundWorker.Interfaces
{
    public interface IBackgroundExecutorAction : IBackgroundExecutorCore
    {
        /// <summary>
        /// Akce vyvolána po úspěšném dokončení akce na pozadí
        /// </summary>
        /// <param name="successHandler"></param>
        /// <returns></returns>
        IBackgroundExecutorCore OnSuccess(Action successHandler);

        /// <summary>
        /// Provede na pozadí pouze delay a poté akci, která měla být vyvolána na pozadí,
        /// provede na UI vlákně. Metoda definována v <see cref="OnSuccess(Action)"/> bude ignorována
        /// </summary>
        /// <param name="delay">Delay v milisekundách</param>
        /// <returns></returns>
        IBackgroundExecutorCore AfterDelay(int delay = 0);
    }
}
