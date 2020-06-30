using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using resx = JdUtils.Resources.Resources;

// INFO: Komentáře z PrismLibrary (PL:)
namespace JdUtils.Infrastructure
{
    /// <summary>
    /// Poskytuje možnost hlídat změny objektů implementujících <see cref="INotifyPropertyChanged"/> a
    /// vyvolává volitelné metody, když nastane změna požadovaných vlastností.
    /// </summary>
    internal class PropertyObserver
    {
        private readonly Action m_action;

        /// <summary>
        /// Vnitřní konstruktor. Vytvoří novou instanci <see cref="PropertyObserver"/>
        /// </summary>
        /// <param name="propertyExpression">Výraz vlastnosti.</param>
        /// <param name="action"><see cref="Action"/>, která se má vyvolat při změně vlastnosti.</param>
        private PropertyObserver(Expression propertyExpression, Action action)
        {
            m_action = action;
            SubscribeListeners(propertyExpression);
        }

        /// <summary>
        /// Přihlášení se k odběru změny vlastnosti
        /// </summary>
        /// <param name="propertyExpression">Výraz vlastnosti.</param>
        private void SubscribeListeners(Expression propertyExpression)
        {
            var propNameStack = new Stack<string>();
            while (propertyExpression is MemberExpression temp) // PL: Gets the root of the property chain.
            {
                propertyExpression = temp.Expression;
                propNameStack.Push(temp.Member.Name); // PL: Records the name of each property.
            }

            if (propertyExpression is ConstantExpression constantExpression)
            {
                var propObserverNodeRoot = new PropertyObserverNode(propNameStack.Pop(), m_action);
                var previousNode = propObserverNodeRoot;
                foreach (var propName in propNameStack) // PL: Create a node chain that corresponds to the property chain.
                {
                    var currentNode = new PropertyObserverNode(propName, m_action);
                    previousNode.Next = currentNode;
                    previousNode = currentNode;
                }

                var propOwnerObject = constantExpression.Value;

                propObserverNodeRoot.SubscribeListenerFor(propOwnerObject);
            }
            else
            {
                throw new NotSupportedException(resx.PropertyObserverIncorrectExpression);
            }
        }

        /// <summary>
        /// Hlídá vlastnost, která implementuje <see cref="INotifyPropertyChanged"/> a automaticky vyvolává zvolenou
        /// akci, při její změně. Daný výraz musí být ve tvaru: "() => Prop.NestedProp.PropToObserve".
        /// </summary>
        /// <param name="propertyExpression">Výraz reprezentující vlastnot, která bude hlídána. Př.: "() => Prop.NestedProp.PropToObserve".</param>
        /// <param name="action"><see cref="Action"/>, která buude vyvolána při změně.</param>
        internal static PropertyObserver Observes<T>(Expression<Func<T>> propertyExpression, Action action)
        {
            return new PropertyObserver(propertyExpression.Body, action);
        }
    }
}