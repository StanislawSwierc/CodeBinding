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
        public static IObservable<TResult> FromExpression<TResult>(Expression<Func<TResult>> expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);

            
            // Create binding
            var binding = BindingEx.FromExpression(expression);

            return Observable.Create<TResult>(observer =>
                {
                    // Create target object
                    var target = new ObservableBindingTarget<TResult>();
                    target.SetBinding(ObservableBindingTarget<TResult>.ValueProperty, binding);
                    return new CompositeDisposable(
                        target.Subscribe(observer),
                        target
                        );
                }
            );
        }
    }
}
