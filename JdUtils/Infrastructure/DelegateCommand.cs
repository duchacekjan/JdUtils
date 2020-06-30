using System;
using System.Linq.Expressions;
using System.Windows.Input;
using resx = JdUtils.Resources.Resources;

namespace JdUtils.Infrastructure
{
    /// <summary>
    /// <see cref="ICommand"/> jehož delegáti mohou být připojeni na <see cref="Execute()"/> a <see cref="CanExecute()"/>.
    /// </summary>
    /// <see cref="DelegateCommandBase"/>
    /// <see cref="DelegateCommand{T}"/>
    public class DelegateCommand : DelegateCommandBase
    {
        private readonly Action m_executeMethod;
        private Func<bool> m_canExecuteMethod;

        /// <summary>
        /// Vytvoří novou instanci <see cref="DelegateCommand"/> s <see cref="Action"/>, která bude zavolána při vykonávání.
        /// </summary>
        /// <param name="executeMethod"><see cref="Action"/>,která bude vyvolána, když bude zavoláno <see cref="Execute()"/>.
        /// <see cref="Action"/> může být <see langword="null"/>, pokud se chce napojit pouze <see cref="CanExecute()"/>.</param>
        /// <remarks><see cref="CanExecute()"/> vždy vrátí <see langword="true"/>.</remarks>
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
        /// <exception cref="ArgumentNullException">Pokud oba parametry <paramref name="executeMethod"/>
        /// a <paramref name="canExecuteMethod"/> jsou <see langword="null" />.</exception>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            if (executeMethod == null && canExecuteMethod == null)
            {
                throw new ArgumentNullException(nameof(executeMethod), resx.DelegateCommandDelegatesCannotBeNull);
            }

            m_executeMethod = executeMethod;
            m_canExecuteMethod = canExecuteMethod;
        }

        ///<summary>
        /// Spouští command a vyvolává <see cref="Action"/> dodanou v konstuktoru.
        ///</summary>
        public void Execute()
        {
            m_executeMethod();
        }

        ///<summary>
        ///Určuje, zda command může být vyvolán pomocí <see cref="Func{TResult}"/> dodané v konstruktoru.
        ///</summary>
        ///<returns>
        ///<see langword="true" /> pokud command může být vyvolán; jinak, <see langword="false" />.
        ///</returns>
        public bool CanExecute()
        {
            return m_canExecuteMethod();
        }

        /// <summary>
        /// Ošetření vnitřního vyvolání <see cref="ICommand.Execute(object)"/>
        /// </summary>
        /// <param name="parameter">Command Parametr</param>
        protected override void Execute(object parameter)
        {
            Execute();
        }

        /// <summary>
        /// Ošetření vnitřního vyvolání <see cref="ICommand.CanExecute(object)"/>
        /// </summary>
        /// <param name="parameter">Command Parametr</param>
        /// <returns><see langword="true"/> pokud Command může být spuštěn, jinak <see langword="false" /></returns>
        protected override bool CanExecute(object parameter)
        {
            return CanExecute();
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

        /// <summary>
        /// Hlídá vlastnost, která je použitá k určení, zda se command má vykonat a pokud implementuje INotifyPropertyChanged, tak
        /// automaticky vyvolává <see cref="DelegateCommandBase.RaiseCanExecuteChanged"/> při změnách této vlastnosti.
        /// </summary>
        /// <param name="canExecuteExpression">Výraz vlastnosti. Příklad: ObservesCanExecute(() => PropertyName).</param>
        /// <returns>Stávající instance <see cref="DelegateCommand"/></returns>
        public DelegateCommand ObservesCanExecute(Expression<Func<bool>> canExecuteExpression)
        {
            m_canExecuteMethod = canExecuteExpression.Compile();
            ObservesPropertyInternal(canExecuteExpression);
            return this;
        }
    }
}