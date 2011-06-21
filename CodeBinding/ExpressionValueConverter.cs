using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace CodeBinding
{
    internal class ExpressionValueConverter : IValueConverter, IMultiValueConverter
    {
        #region Fields

        private Func<object> m_Func;
        private Type m_ReturnType;

        #endregion

        #region Constructors

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <remarks>
        /// Expression should be of a form convertible to parameterless 
        /// lambda body
        /// </remarks>
        /// <param name="expression">Expression</param>
        public ExpressionValueConverter(Expression expression)
        {
            Contract.Requires(expression != null);

            m_ReturnType = expression.Type;
            if (expression.Type != typeof(object))
            {
                expression = Expression.Convert(expression, typeof(object));
            }
            Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(expression);
            m_Func = lambda.Compile();
        }

        #endregion

        #region Methods

        private object Convert(Type targetType)
        {
            Contract.Requires(targetType != null);

            if (!targetType.IsAssignableFrom(m_ReturnType))
            {
                throw new ArgumentException("targetType");
            }
            return m_Func.Invoke();
        }

        #endregion

        #region ValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException();
        }

        #endregion

        #region IMultiValueConverter Implementation

        /// <summary>
        /// Invokes inner delegate and returns its result
        /// </summary>
        /// <remarks>
        /// It completely ignores following parameters: values, parameter, culture
        /// </remarks>
        /// <param name="values">This parameter is ignored</param>
        /// <param name="targetType">Target type which should be assignable from T</param>
        /// <param name="parameter">This parameter is ignored</param>
        /// <param name="culture">This parameter is ignored</param>
        /// <returns>Result of inner delegate invocation</returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(targetType);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
