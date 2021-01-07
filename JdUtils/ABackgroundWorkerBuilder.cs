using JdUtils.Delegates;
using System;
using System.Windows.Threading;

namespace JdUtils
{
    /// <summary>
    /// Společný předek pro Background Worker buildry
    /// </summary>
    public abstract class ABackgroundWorkerBuilder : ABackgroundWorker
    {
        protected ABackgroundWorkerBuilder(Dispatcher uiDispatcher)
            : base(uiDispatcher)
        {
        }

        protected ABackgroundWorkerBuilder(ABackgroundWorkerBuilder builder)
            : base(builder.Dispatcher)
        {
        }

        /// <summary>
        /// Delay akce
        /// </summary>
        protected int Delay { get; set; }

        /// <summary>
        /// Custom error handler
        /// </summary>
        protected ExceptionHandler Failure { get; set; }

        /// <summary>
        /// Provede zadané akce
        /// </summary>
        public void Execute()
        {
            ExecuteSafe(GetWorker(), Failure, Delay);
        }

        /// <summary>
        /// Definice Delay před spuštěním akce na pozadí. Pokud je <paramref name="delay"/> větší než 0,
        /// použije se <see cref="System.Threading.Tasks.Task.Delay(int)"/>, jinak <see cref="System.Threading.Tasks.Task.CompletedTask"/>
        /// </summary>
        /// <param name="delay">Delay v milisekundách</param>
        /// <returns></returns>
        public ABackgroundWorker WithDelay(int delay)
        {
            Delay = delay;
            return this;
        }

        /// <summary>
        /// Nastavení custom error handleru. Není-li definován nebo je <see langword="null"/>,
        /// tak se vyvolá event <see cref="UnhandledError"/>.
        /// </summary>
        /// <param name="failureHandler">Custom error handler</param>
        /// <returns></returns>
        public ABackgroundWorker OnError(ExceptionHandler failureHandler)
        {
            Failure = failureHandler;
            return this;
        }

        /// <summary>
        /// Akce, která má být vyvolána na pozadí. Spojuje volání akce na pozadí a volání
        /// výsledku zpět na UI vlákně
        /// </summary>
        /// <returns></returns>
        protected abstract Action GetWorker();
    }
}
