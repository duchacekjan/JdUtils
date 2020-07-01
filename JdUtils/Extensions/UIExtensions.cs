using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace JdUtils.Extensions
{
    public static class UIExtensions
    {
        /// <summary>
        /// Finds part of Template with given name and given type
        /// </summary>
        /// <typeparam name="T">Type of searched part</typeparam>
        /// <param name="source">Control in which Template should be searched part</param>
        /// <param name="partName">PartName</param>
        /// <returns></returns>
        public static T FindTemplatePart<T>(this Control source, string partName)
            where T : DependencyObject
        {
            var part = source?.Template.FindName(partName, source);
            return part as T;
        }

        /// <summary>
        /// Finds part of Control with given name and given type
        /// </summary>
        /// <typeparam name="T">Type of searched part</typeparam>
        /// <param name="source">Control in which should be searched part</param>
        /// <param name="partName">PartName</param>
        /// <returns></returns>
        public static T FindName<T>(this Control source, string partName)
            where T : DependencyObject
        {
            var part = source?.FindName(partName);
            return part as T;
        }

        /// <summary>
        /// Sets property(<typeparamref name="TProperty"/>) value, when parent (<typeparamref name="TSource"/> <paramref name="source"/>) is not <see langword="null"/> 
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="propertyExpression">Expression which selects property</param>
        /// <param name="value">New value of property</param>
        /// <remarks>Only <see cref="MemberExpression"/> supported<para>Main use is on CustomControl parts</para></remarks>
        /// <exception cref="NotSupportedException">Occurs when <see cref="Expression{TDelegate}"/> is not <see cref="MemberExpression"/></exception>
        public static void SetValueSafe<TSource, TProperty>(this TSource source, Expression<Func<TSource, TProperty>> propertyExpression, TProperty value)
        {
            if (source != null && propertyExpression != null)
            {
                var propertyInfo = propertyExpression.GetPropertyInfo();
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(source, value);
                }
            }
        }

        /// <summary>
        /// Invokes <paramref name="action"/> when <paramref name="source"/> is not null
        /// </summary>
        /// <typeparam name="T">Source type</typeparam>
        /// <param name="source">Object for check is is not <see langword="null"/></param>
        /// <param name="action">Action to be invoked</param>
        /// <remarks>For fluent api use <see cref="AndIfNotNull{T}(T, Action{T})"/></remarks>
        public static void IfNotNull<T>(this T source, Action<T> action)
        {
            if (source != null)
            {
                action?.Invoke(source);
            }
        }

        /// <summary>
        /// Invokes <paramref name="action"/> when <paramref name="source"/> is not null.
        /// </summary>
        /// <typeparam name="T">Source type</typeparam>
        /// <param name="source">Object for check is is not <see langword="null"/></param>
        /// <param name="action">Action to be invoked</param>
        /// <remarks>For fluent api</remarks>
        /// <returns>Returns <paramref name="source"/></returns>
        public static T AndIfNotNull<T>(this T source, Action<T> action)
        {
            if (source != null)
            {
                action?.Invoke(source);
            }

            return source;
        }

        /// <summary>
        /// Extract <see cref="PropertyInfo"/> from given <see cref="Expression{TDelegate}"/>
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <param name="propertyExpression">Expression which selects property</param>
        /// <returns></returns>
        /// <remarks>Only <see cref="MemberExpression"/> supported</remarks>
        /// <exception cref="NotSupportedException">Occurs when <see cref="Expression{TDelegate}"/> is not <see cref="MemberExpression"/></exception>
        private static PropertyInfo GetPropertyInfo<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyExpression)
        {
            PropertyInfo result;
            if (propertyExpression?.Body is MemberExpression memberExpression)
            {
                var propertyName = memberExpression.Member.Name;
                result = typeof(TSource).GetProperty(propertyName);
            }
            else
            {
                throw new NotSupportedException("Only member expression supported");
            }

            return result;
        }
    }
}
