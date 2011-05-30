using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Diagnostics.Contracts;

namespace CodeBinding.Rx
{
    internal static class BindingTargetExtensions
    {
        /// <summary>
        /// Converts BindingTarget to observable sequence
        /// </summary>
        public static IObservable<T> ToObservable<T>(this BindingTarget<T> @this)
        {
            Contract.Requires(@this != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            return Observable.Create<T>(s =>
            {
                s.OnNext(@this.Value);
                return Observable.FromEvent<T>(@this.AddHandler, @this.RemoveHandler).Subscribe(s);
            });
        }
    }
}
