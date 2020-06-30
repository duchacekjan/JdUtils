using System;
using System.ComponentModel;
using System.Reflection;
using resx = JdUtils.Resources.Resources;

// INFO: Komentáře z PrismLibrary (PL:)
namespace JdUtils.Infrastructure
{
    /// <summary>
    /// Reprezentuje každý uzel zanořeného property expression a stará se
    /// o přihlašování/odhlašování eventu <see cref="INotifyPropertyChanged.PropertyChanged"/>.
    /// </summary>
    internal class PropertyObserverNode
    {
        private readonly Action m_action;
        private INotifyPropertyChanged m_notifyPropertyChangedObject;

        public string PropertyName { get; }
        public PropertyObserverNode Next { get; set; }

        /// <summary>
        /// Vytvoří novou instanci <see cref="PropertyObserverNode"/>
        /// </summary>
        /// <param name="propertyName">Jméno hlídané vlastnosti</param>
        /// <param name="action"><see cref="Action"/>, akce, která bude vyvolána při změně hlídané vlastnosti</param>
        public PropertyObserverNode(string propertyName, Action action)
        {
            PropertyName = propertyName;
            m_action = () =>
            {
                action?.Invoke();
                if (Next != null)
                {
                    Next.UnsubscribeListener();
                    GenerateNextNode();
                }
            };
        }

        /// <summary>
        /// Přihlásí se k odběru <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// na předaném objektu
        /// </summary>
        /// <param name="inpcObject">Objekt implementující <see cref="INotifyPropertyChanged"/></param>
        public void SubscribeListenerFor(INotifyPropertyChanged inpcObject)
        {
            m_notifyPropertyChangedObject = inpcObject;
            m_notifyPropertyChangedObject.PropertyChanged += OnPropertyChanged;

            if (Next != null)
            {
                GenerateNextNode();
            }
        }

        /// <summary>
        /// Zkontroluje, zda objekt implementuje <see cref="INotifyPropertyChanged"/> a poté se
        /// přihlásí k odběru <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// na předaném objektu
        /// </summary>
        /// <param name="obj">Objekt, který by měl implementovat<see cref="INotifyPropertyChanged"/></param>
        /// <exception cref="InvalidOperationException">Pokud objekt neimplementuje <see cref="INotifyPropertyChanged"/></exception>
        public void SubscribeListenerFor(object obj)
        {
            if (obj is INotifyPropertyChanged inpcObject)
            {
                SubscribeListenerFor(inpcObject);
            }
            else
            {
                throw new InvalidOperationException(string.Format(resx.PropertyObserverNodeObjectIsNotINotifyPropertyChanged, PropertyName));
            }
        }

        /// <summary>
        /// Vytvoří navazující uzel
        /// </summary>
        private void GenerateNextNode()
        {
            // PL:TODO: To cache, if the step consume significant performance.
            // PL:The type of m_notifyPropertyChangedObject may become its base type or derived type.
            var propertyInfo = m_notifyPropertyChangedObject.GetType().GetRuntimeProperty(PropertyName);
            var nextProperty = propertyInfo.GetValue(m_notifyPropertyChangedObject);
            if (nextProperty != null)
            {
                Next.SubscribeListenerFor(nextProperty);
            }
        }

        /// <summary>
        /// Odhlásí odběr <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// </summary>
        private void UnsubscribeListener()
        {
            if (m_notifyPropertyChangedObject != null)
            {
                m_notifyPropertyChangedObject.PropertyChanged -= OnPropertyChanged;
            }

            Next?.UnsubscribeListener();
        }

        /// <summary>
        /// Vyvolání akce při změně požadované vlastnosti
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // PL: Invoke action when e.PropertyName == null in order to satisfy:
            // PL: - DelegateCommandFixture.GenericDelegateCommandObservingPropertyShouldRaiseOnEmptyPropertyName
            // PL: - DelegateCommandFixture.NonGenericDelegateCommandObservingPropertyShouldRaiseOnEmptyPropertyName
            if (e?.PropertyName == PropertyName || e?.PropertyName == null)
            {
                m_action?.Invoke();
            }
        }
    }
}