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
        public static IObservable<TResult> Create<TResult>(Expression<Func<TResult>> expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);

            
            // Create binding
            var binding = BindingEx.FromExpression(expression);
            IObservable<TResult> observableConverter = null;
            if (binding is Binding)
            {
                var single = (Binding)binding;
                if (single.Converter != null)
                {
                    // Create intercepting converter
                    var converter = new ObservableValueConverter<TResult>(single.Converter);
                    // Replace original converter 
                    single.Converter = converter;
                    observableConverter = converter;
                }
            }
            else if (binding is MultiBinding)
            {
                var multi = (MultiBinding)binding;
                // Create intercepting converter
                var converter = new ObservableMultiValueConverter<TResult>(multi.Converter);
                // Replace original converter 
                multi.Converter = converter;
                observableConverter = converter;
            }
            else
            {
                // Only Binding and MultiBinding classes are supported
                throw new InvalidOperationException("Invalid binding type");
            }

            return Observable.Create<TResult>(observer =>
                {
                    // Create target object
                    var target = new ObservableBindingTarget<TResult>();
                    if (observableConverter != null)
                    {
                        var subscription = observableConverter.Subscribe(observer);
                        // Binding has to be set in order to enable converter
                        target.SetBinding(ObservableBindingTarget<TResult>.ValueProperty, binding);
                        return new CompositeDisposable(
                            target,
                            subscription
                            );
                    }
                    else
                    {
                        target.SetBinding(ObservableBindingTarget<TResult>.ValueProperty, binding);
                        return new CompositeDisposable(
                            target.Subscribe(observer),
                            target
                            );
                    }
                }
            );
        }
    }
}
