using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
            : base(o => executeMethod((T)o), o => canExecuteMethod((T)o))
        {
            CheckIsAssigned(executeMethod, canExecuteMethod);

            if (!IsObjectOrNullable(typeof(T)))
            {
                throw new InvalidCastException(resx.DelegateCommandInvalidGenericPayloadType);
            }
        }

        /// <summary>
        /// Vytvoří novou instanci <see cref="DelegateCommand{T}"/> s <see cref="Func{TResult}"/>, která bude zavolána při vykonávání.
        /// </summary>
        /// <param name="executeMethod"><see cref="Func{TResult}"/>,která bude vyvolána, když bude zavoláno <see cref="Execute(T)"/>.
        /// <see cref="Action{T}"/> může být <see langword="null"/>, pokud se chce napojit pouze <see cref="CanExecute(T)"/>.</param>
        /// <remarks><see cref="CanExecute(T)"/> vždy vrátí <see langword="true"/>.</remarks>
        private DelegateCommand(Func<T, Task> executeMethod)
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
        private DelegateCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
            : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o))
        {
            CheckIsAssigned(executeMethod, canExecuteMethod);
        }

        /// <summary>
        /// Factory method to create a new instance of <see cref="DelegateCommand{T}"/> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.</param>
        /// <returns>Constructed instance of <see cref="DelegateCommand{T}"/></returns>
        public static DelegateCommand<T> FromAsyncHandler(Func<T, Task> executeMethod)
        {
            return new DelegateCommand<T>(executeMethod);
        }

        /// <summary>
        /// Factory method to create a new instance of <see cref="DelegateCommand{T}"/> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command. This can be null to just hook up a CanExecute delegate.</param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <returns>Constructed instance of <see cref="DelegateCommand{T}"/></returns>
        public static DelegateCommand<T> FromAsyncHandler(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
        {
            return new DelegateCommand<T>(executeMethod, canExecuteMethod);
        }

        ///<summary>
        ///Spouští command a vyvolává <see cref="Action{T}"/> dodanou v konstuktoru.
        ///</summary>
        ///<param name="parameter">Data používaná commandem.</param>
        public virtual async Task Execute(T parameter)
        {
            await base.Execute(parameter);
        }

        ///<summary>
        ///Určuje, zda command může být vyvolán pomocí <see cref="Func{TBool,TResult}"/> dodané v konstruktoru.
        ///</summary>
        ///<param name="parameter">Data používaná commandem.</param>
        ///<returns>
        ///<see langword="true" /> pokud command může být vyvolán; jinak, <see langword="false" />.
        ///</returns>
        public virtual bool CanExecute(T parameter)
        {
            return base.CanExecute(parameter);
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