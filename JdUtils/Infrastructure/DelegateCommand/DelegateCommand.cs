using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JdUtils.Infrastructure
{
    /// <summary>
    /// <see cref="ICommand"/> jehož delegáti mohou být připojeni na <see cref="Execute()"/> a <see cref="CanExecute()"/>.
    /// </summary>
    /// <see cref="DelegateCommandBase"/>
    /// <see cref="DelegateCommand{T}"/>
    public class DelegateCommand : DelegateCommandBase
    {
        /// <summary>
        /// Vytvoří novou instanci <see cref="DelegateCommand"/> s <see cref="Action"/>, která bude zavolána při vykonávání.
        /// </summary>
        /// <param name="executeMethod"><see cref="Action"/>,která bude vyvolána, když bude zavoláno <see cref="Execute()"/>.
        /// <see cref="Action"/> může být <see langword="null"/>, pokud se chce napojit pouze <see cref="CanExecute()"/>.</param>
        /// <remarks><see cref="CanExecute()"/> vždy vrátí <see langword="true"/>.</remarks>
        /// <exception cref="ArgumentNullException">Pokud je parametr <paramref name="executeMethod"/> <see langword="null" />.</exception>
        public DelegateCommand(Action executeMethod)
            : this(executeMethod, () => true)
        {
        }

        /// <summary>
        /// Vytvoří novou instanci <see cref="DelegateCommand"/> s <see cref="Action"/>, která bude zavolána při vyvolání
        /// a <see cref="Func{TResult}"/> pro určení, zda se může command vykonat.
        /// </summary>
        /// <param name="executeMethod"><see cref="Action"/>,která bude vyvolána, když bude zavoláno <see cref="Execute()"/>.
        /// <see cref="Action"/> může být <see langword="null"/>, pokud se chce napojit pouze <see cref="CanExecute()"/>.</param>
        /// <param name="canExecuteMethod"><see cref="Func{TResult}"/>, která bude vyvolána, když bude zavoláno <see cref="CanExecute()"/>.
        /// <see cref="Func{TResult}"/> může být <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Pokud je jeden z parametrů <paramref name="executeMethod"/>
        /// a <paramref name="canExecuteMethod"/> <see langword="null" />.</exception>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
            : base(o => executeMethod(), o => canExecuteMethod())
        {
            CheckIsAssigned(executeMethod, canExecuteMethod);
        }

        /// <summary>
        /// Vytvoří novou instanci <see cref="DelegateCommand"/> s <see cref="Func{TResult}"/>, která bude zavolána při vyvolání
        /// a <see cref="Func{TResult}"/> pro určení, zda se může command vykonat.
        /// </summary>
        /// <param name="executeMethod"><see cref="Func{TResult}"/>,která bude vyvolána, když bude zavoláno <see cref="Execute()"/>.
        /// <see cref="Action"/> může být <see langword="null"/>, pokud se chce napojit pouze <see cref="CanExecute()"/>.</param>
        /// <exception cref="ArgumentNullException">Pokud je parametr <paramref name="executeMethod"/> <see langword="null" />.</exception>
        private DelegateCommand(Func<Task> executeMethod)
            : this(executeMethod, () => true)
        {
        }

        /// <summary>
        /// Vytvoří novou instanci <see cref="DelegateCommand"/> s <see cref="Func{TResult}"/>, která bude zavolána při vyvolání
        /// a <see cref="Func{TResult}"/> pro určení, zda se může command vykonat.
        /// </summary>
        /// <param name="executeMethod"><see cref="Func{TResult}"/>,která bude vyvolána, když bude zavoláno <see cref="Execute()"/>.
        /// <see cref="Action"/> může být <see langword="null"/>, pokud se chce napojit pouze <see cref="CanExecute()"/>.</param>
        /// <param name="canExecuteMethod"><see cref="Func{TResult}"/>, která bude vyvolána, když bude zavoláno <see cref="CanExecute()"/>.
        /// <see cref="Func{TResult}"/> může být <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Pokud je jeden z parametrů <paramref name="executeMethod"/>
        /// a <paramref name="canExecuteMethod"/> <see langword="null" />.</exception>
        private DelegateCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
            : base((o) => executeMethod(), (o) => canExecuteMethod())
        {
            CheckIsAssigned(executeMethod, canExecuteMethod);
        }

        /// <summary>
        /// Factory method to create a new instance of <see cref="DelegateCommand"/> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.</param>
        /// <returns>Constructed instance of <see cref="DelegateCommand"/></returns>
        public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod)
        {
            return new DelegateCommand(executeMethod);
        }

        /// <summary>
        /// Factory method to create a new instance of <see cref="DelegateCommand"/> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command. This can be null to just hook up a CanExecute delegate.</param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <returns>Constructed instance of <see cref="DelegateCommand"/></returns>
        public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod)
        {
            return new DelegateCommand(executeMethod, canExecuteMethod);
        }

        ///<summary>
        /// Spouští command a vyvolává <see cref="Action"/> dodanou v konstuktoru.
        ///</summary>
        public virtual async Task Execute()
        {
            await Execute(null);
        }

        ///<summary>
        ///Určuje, zda command může být vyvolán pomocí <see cref="Func{TResult}"/> dodané v konstruktoru.
        ///</summary>
        ///<returns>
        ///<see langword="true" /> pokud command může být vyvolán; jinak, <see langword="false" />.
        ///</returns>
        public virtual bool CanExecute()
        {
            return CanExecute(null);
        }

        /// <summary>
        /// Hlídá vlastnost, která implementuje INotifyPropertyChanged, a automaticky vyvolává <see cref="DelegateCommandBase.RaiseCanExecuteChanged"/> při změnách této vlastnosti.
        /// </summary>
        /// <typeparam name="TType">Typ návratové hodnoty metody, kterout tento delegát zabaluje</typeparam>
        /// <param name="propertyExpression">Výraz vlastnosti. Příklad: ObservesProperty(() => PropertyName).</param>
        /// <returns>Stávající instance <see cref="DelegateCommand"/></returns>
        public DelegateCommand ObservesProperty<TType>(Expression<Func<TType>> propertyExpression)
        {
            ObservesPropertyInternal(propertyExpression);
            return this;
        }
    }
}