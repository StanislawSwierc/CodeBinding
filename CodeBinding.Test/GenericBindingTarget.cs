using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CodeBinding.Test
{
    public class GenericBindingTarget<T> : FrameworkElement
    {
        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(T), typeof(GenericBindingTarget<T>));
    }
}