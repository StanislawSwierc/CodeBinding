using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace CodeBinding.Rx
{
    internal static class BindingTargetExtensions
    {
        public static IObservable<T> ToObservable<T>(this BindingTarget<T> @this)
        {
            return Observable.Create<T>(s =>
            {
                s.OnNext(@this.Value);
                return Observable.FromEvent<T>(@this.AddHandler, @this.RemoveHandler).Subscribe(s);
            });
        }
    }
}
