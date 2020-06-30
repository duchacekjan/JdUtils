using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;
using resx = JdUtils.Resources.Resources;

namespace JdUtils.Infrastructure
{
    /// <summary>
    /// <see cref="ICommand"/> jehož delegáti mohou být připojeni na <see cref="Execute"/> a <see cref="CanExecute"/>.
    /// </summary>
    public abstract class DelegateCommandBase : ICommand
    {
        private readonly HashSet<string> m_observedPropertiesExpressions = new HashSet<string>();
        private List<WeakReference> m_canExecuteChangedHandlers;

        protected readonly Func<object, Task> m_executeMethod;
        protected readonly Func<object, bool> m_canExecuteMethod;

        /// <summary>
        /// Vytvoří novou instanci <see cref="DelegateCommandBase"/>, specifikující jak metodu při vykonání commandu, tak funkci,
        /// zda lze command vykonat.
        /// </summary>
        /// <param name="executeMethod"><see cref="Action"/>, která se vykoná, když je <see cref="ICommand.Execute"/> vyvoláno.</param>
        /// <param name="canExecuteMethod"><see cref="Func{Object,Bool}"/>, která se vykoná, když je <see cref="ICommand.CanExecute"/> vyvoláno.</param>
        protected DelegateCommandBase(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException(nameof(executeMethod), resx.DelegateCommandDelegatesCannotBeNull);
            }

            m_executeMethod = (arg) => { executeMethod(arg); return Task.CompletedTask; };
            m_canExecuteMethod = canExecuteMethod;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DelegateCommandBase"/>, specifying both the Execute action as an awaitable Task and the CanExecute function.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Func{Object,Task}"/> to execute when <see cref="ICommand.Execute"/> is invoked.</param>
        /// <param name="canExecuteMethod">The <see cref="Func{Object,Bool}"/> to invoked when <see cref="ICommand.CanExecute"/> is invoked.</param>
        protected DelegateCommandBase(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException(nameof(executeMethod), resx.DelegateCommandDelegatesCannotBeNull);
            }

            m_executeMethod = executeMethod;
            m_canExecuteMethod = canExecuteMethod;
        }

        /// <summary>
        /// Vyvoláno když se objeví změna, která ovlivní zda se má command vykonat.
        /// </summary>
        public virtual event EventHandler CanExecuteChanged
        {
            add
            {
                WeakEventHandlerManager.AddWeakReferenceHandler(ref m_canExecuteChangedHandlers, value, 2);
            }
            remove
            {
                WeakEventHandlerManager.RemoveWeakReferenceHandler(m_canExecuteChangedHandlers, value);
            }
        }

        /// <summary>
        /// Vyvolá <see cref="ICommand.CanExecuteChanged"/>, takže každý
        /// kdo command používá může znovu vyhodnotit <see cref="ICommand.CanExecute"/>.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            WeakEventHandlerManager.CallWeakReferenceHandlers(this, m_canExecuteChangedHandlers);
        }

        ///// <summary>
        ///// Vyvolá <see cref="CanExecuteChanged"/>
        ///// </summary>
        ///// <param name="obj">Nepoužitý parametr. Slouží pouze pro signature <see cref="SynchronizationContext.Post"/></param>
        //private void InvokeCanExecuteChanged(object obj = null)
        //{
        //    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        //}

        /// <summary>
        /// Vyvolá <see cref="CanExecuteChanged"/>, takže každy, kdo
        /// commmand používá může znovu vyhodnotit zda lze command vyvolat.
        /// <remarks>
        /// Toto vyvolá spuštění <see cref="CanExecuteChanged"/> jednou pro každého, kdo toto vyvolal.</remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        /// <summary>Defines the method to be called when the command is invoked.</summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        async void ICommand.Execute(object parameter)
        {
            await Execute(parameter);
        }

        /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        /// <summary>
        /// Ošetření vnitřního vyvolání <see cref="ICommand.Execute(object)"/>
        /// </summary>
        /// <param name="parameter">Command Parametr</param>
        protected async Task Execute(object parameter)
        {
            await m_executeMethod(parameter);
        }

        /// <summary>
        /// Ošetření vnitřního vyvolání <see cref="ICommand.CanExecute(object)"/>
        /// </summary>
        /// <param name="parameter">Command Parametr</param>
        /// <returns><see langword="true"/> pokud Command může být spuštěn, jinak <see langword="false" /></returns>
        protected bool CanExecute(object parameter)
        {
            return m_canExecuteMethod?.Invoke(parameter) == true;
        }

        /// <summary>
        /// Hlídá vlastnost, která implementuje INotifyPropertyChanged, a automaticky vyvolává <see cref="DelegateCommandBase.RaiseCanExecuteChanged"/> při změnách této vlastnosti.
        /// </summary>
        /// <typeparam name="TType">Typ návratové hodnoty metody, kterout tento delegát zabaluje</typeparam>
        /// <param name="propertyExpression">Výraz vlastnosti. Příklad: ObservesProperty(() => PropertyName).</param>
        /// <returns>Stávající instance <see cref="DelegateCommand"/></returns>
        protected internal void ObservesPropertyInternal<TType>(Expression<Func<TType>> propertyExpression)
        {
            if (!m_observedPropertiesExpressions.Contains(propertyExpression.ToString()))
            {
                m_observedPropertiesExpressions.Add(propertyExpression.ToString());
                PropertyObserver.Observes(propertyExpression, RaiseCanExecuteChanged);
            }
            else
            {
                throw new ArgumentException(string.Format(resx.DelegateCommandPropertyIsBeeingObserved, propertyExpression), nameof(propertyExpression));
            }
        }
    }
}