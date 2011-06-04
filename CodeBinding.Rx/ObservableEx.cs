using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CodeBinding;
using System.Windows.Data;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Reactive.Disposables;

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
        // TODO: Change return type back to IObserbable<TResult>
        //       This will require client to save reference of the returned object
        public static ObservableBindingTarget<TResult> FromExpression<TResult>(Expression<Func<TResult>> expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);

            // Create target object
            ObservableBindingTarget<TResult> target = new ObservableBindingTarget<TResult>();

            // Create expression which can be used to create bingind
            // () => target.Value == <expression.Body>
            Expression<Func<bool>> expr = Expression.Lambda<Func<bool>>(
                Expression.Equal(
                    Expression.Property(Expression.Constant(target), "Value"),
                    expression.Body));
            
            // Create binding
            BindingEx.BindFromExpression(expr);

            return target;
        }
    }
}
