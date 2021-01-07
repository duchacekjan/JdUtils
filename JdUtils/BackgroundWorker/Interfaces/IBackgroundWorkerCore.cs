using JdUtils.Delegates;

namespace JdUtils.BackgroundWorker.Interfaces
{
    public interface IBackgroundExecutorCore
    {
        /// <summary>
        /// Provede zadané akce
        /// </summary>
        void Execute();

        /// <summary>
        /// Definice Delay před spuštěním akce na pozadí. Pokud je <paramref name="delay"/> větší než 0,
        /// použije se <see cref="System.Threading.Tasks.Task.Delay(int)"/>,
        /// jinak <see cref="System.Threading.Tasks.Task.CompletedTask"/>
        /// </summary>
        /// <param name="delay">Delay v milisekundách</param>
        /// <returns></returns>
        IBackgroundExecutorCore WithDelay(int delay);

        /// <summary>
        /// Nastavení custom error handleru. Není-li definován nebo je <see langword="null"/>,
        /// tak se vyvolá event <see cref="UnhandledError"/>.
        /// </summary>
        /// <param name="failureHandler">Custom error handler</param>
        /// <returns></returns>
        IBackgroundExecutorCore OnError(ExceptionHandler failureHandler);
    }
}
