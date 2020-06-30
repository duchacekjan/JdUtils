using System;
using System.Linq.Expressions;
using System.Windows.Input;
using resx = JdUtils.Resources.Resources;

namespace JdUtils.Infrastructure
{
    /// <summary>
    /// <see cref="ICommand"/> jehož delegáti mohou být připojeni na <see cref="Execute(T)"/> a <see cref="CanExecute(T)"/>.
    /// </summary>
    /// <typeparam name="T">Typ CommandParameter.</typeparam>
    /// <remarks>
    /// Konstruktor schválně zabraňuje použití hodnotových typů.
    /// Protože ICommand přijímá <see cref="object"/>, tak hodnotový typ způsobuje neočekávané chování, v případě, kdy se volá CanExecute(null) při XAML inicializaci pro
    /// bindování commandů. Použití default(T) bylo zváženo a nepoužito, protože implementátor by nebyl schopen rozlišit mezi validní a výchozí hodnotou.
    /// <para/>
    /// Místo toho by se měly používat nullable typy a testovat na HasValue před použitím hodnoty.
    /// <example>
    ///     <code>
    /// public MyClass()
    /// {
    ///     this.submitCommand = new DelegateCommand&lt;int?&gt;(this.Submit, this.CanSubmit);
    /// }
    /// 
    /// private bool CanSubmit(int? customerId)
    /// {
    ///     return (customerId.HasValue &amp;&amp; customers.Contains(customerId.Value));
    /// }
    ///     </code>
    /// </example>
    /// </remarks>
    public class DelegateCommand<T> : DelegateCommandBase
    {
        private readonly Action<T> m_executeMethod;
        private Func<T, bool> m_canExecuteMethod;

        /// <summary>
        /// Vytvoří novou instanci <see cref="DelegateCommand{T}"/> s <see cref="Action{T}"/>, která bude zavolána při vykonávání.
        /// </summary>
        /// <param name="executeMethod"><see cref="Action{T}"/>,která bude vyvolána, když bude zavoláno <see cref="Execute(T)"/>.
        /// <see cref="Action{T}"/> může být <see langword="null"/>, pokud se chce napojit pouze <see cref="CanExecute(T)"/>.</param>
        /// <remarks><see cref="CanExecute(T)"/> vždy vrátí <see langword="true"/>.</remarks>
        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, (o) => true)
        {
        }

        /// <summary>
        /// Inicializuje novou instanci <see cref="DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="executeMethod"><see cref="Action{T}"/>,která bude vyvolána, když bude zavoláno <see cref="Execute(T)"/>.
        /// <see cref="Action{T}"/> může být <see langword="null"/>, pokud se chce napojit pouze <see cref="CanExecute(T)"/>.</param>
        /// <param name="canExecuteMethod"><see cref="Func{TBool, TResult}"/>, která bude vyvolána, když bude zavoláno <see cref="CanExecute(T)"/>.
        /// <see cref="Func{TBool, TResult}"/> může být <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Pokud oba parametry <paramref name="executeMethod"/>
        /// a <paramref name="canExecuteMethod"/> jsou <see langword="null" />.</exception>
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            if (executeMethod == null && canExecuteMethod == null)
            {
                throw new ArgumentNullException(nameof(executeMethod), resx.DelegateCommandDelegatesCannotBeNull);
            }

            if (!IsObjectOrNullable(typeof(T)))
            {
                throw new InvalidCastException(resx.DelegateCommandInvalidGenericPayloadType);
            }

            m_executeMethod = executeMethod;
            m_canExecuteMethod = canExecuteMethod;
        }

        ///<summary>
        ///Spouští command a vyvolává <see cref="Action{T}"/> dodanou v konstuktoru.
        ///</summary>
        ///<param name="parameter">Data používaná commandem.</param>
        public void Execute(T parameter)
        {
            m_executeMethod(parameter);
        }

        ///<summary>
        ///Určuje, zda command může být vyvolán pomocí <see cref="Func{TBool,TResult}"/> dodané v konstruktoru.
        ///</summary>
        ///<param name="parameter">Data používaná commandem.</param>
        ///<returns>
        ///<see langword="true" /> pokud command může být vyvolán; jinak, <see langword="false" />.
        ///</returns>
        public bool CanExecute(T parameter)
        {
            return m_canExecuteMethod(parameter);
        }

        /// <summary>
        /// Hlídá vlastnost, která implementuje INotifyPropertyChanged, a automaticky vyvolává <see cref="DelegateCommandBase.RaiseCanExecuteChanged"/> při změnách této vlastnosti.
        /// </summary>
        /// <typeparam name="TType">Typ návratové hodnoty metody, kterout tento delegát zabaluje</typeparam>
        /// <param name="propertyExpression">Výraz vlastnosti. Příklad: ObservesProperty(() => PropertyName).</param>
        /// <returns>Stávající instance <see cref="DelegateCommand{T}"/></returns>
        public DelegateCommand<T> ObservesProperty<TType>(Expression<Func<TType>> propertyExpression)
        {
            ObservesPropertyInternal(propertyExpression);
            return this;
        }

        /// <summary>
        /// Hlídá vlastnost, která je použitá k určení, zda se command má vykonat a pokud implementuje INotifyPropertyChanged, tak
        /// automaticky vyvolává <see cref="DelegateCommandBase.RaiseCanExecuteChanged"/> při změnách této vlastnosti.
        /// </summary>
        /// <param name="canExecuteExpression">Výraz vlastnosti. Příklad: ObservesCanExecute(() => PropertyName).</param>
        /// <returns>Stávající instance <see cref="DelegateCommand{T}"/></returns>
        public DelegateCommand<T> ObservesCanExecute(Expression<Func<bool>> canExecuteExpression)
        {
            var expression = Expression.Lambda<Func<T, bool>>(canExecuteExpression.Body, Expression.Parameter(typeof(T), "o"));
            m_canExecuteMethod = expression.Compile();
            ObservesPropertyInternal(canExecuteExpression);
            return this;
        }

        /// <summary>
        /// Ošetření vnitřního vyvolání <see cref="ICommand.Execute(object)"/>
        /// </summary>
        /// <param name="parameter">Command Parametr</param>
        protected override void Execute(object parameter)
        {
            Execute((T)parameter);
        }

        /// <summary>
        /// Ošetření vnitřního vyvolání <see cref="ICommand.CanExecute(object)"/>
        /// </summary>
        /// <param name="parameter">Command Parametr</param>
        /// <returns><see langword="true"/> pokud Command může být spuštěn, jinak <see langword="false" /></returns>
        protected override bool CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }

        /// <summary>
        /// Metoda zjistí, zda je typ <see cref="object"/> nebo <see cref="Nullable{T}"/>
        /// </summary>
        /// <param name="type">Zkoumaný typ</param>
        /// <returns></returns>
        private static bool IsObjectOrNullable(Type type)
        {
            var result = true;
            if (type.IsValueType)
            {
                //TODO Extension Type.IsNullable
                result = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            return result;
        }
    }
}