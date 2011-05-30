using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CodeBinding;
using System.Windows.Data;
using System.Diagnostics.Contracts;

namespace CodeBinding.Rx
{
    public static class ObservableEx
    {
        /// <summary>
        /// Creates 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IObservable<TResult> FromExpression<TResult>(Expression<Func<TResult>> expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);

            // Create target object
            BindingTarget<TResult> target = new BindingTarget<TResult>();

            // Create expression which can be used to create bingind
            // () => target.Value == <expression.Body>
            Expression<Func<bool>> expr = Expression.Lambda<Func<bool>>(
                Expression.Equal(
                    Expression.Property(Expression.Constant(target), "Value"),
                    expression.Body));
            // Create binding
            BindingBase binding = BindingEx.FromExpression(expr);

            // Crete IObservable from target
            return target.ToObservable();
        }
    }
}
