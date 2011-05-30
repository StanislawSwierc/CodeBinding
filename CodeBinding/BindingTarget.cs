using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace CodeBinding
{
    internal class BindingTarget<T> : FrameworkElement
    {
        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(T), typeof(BindingTarget<T>), new UIPropertyMetadata(OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (BindingTarget<T>)d;
            var handler = obj.m_ValueChanged;
            if (handler != null)
            {
                handler(obj.Value);
            }
        }

        private Action<T> m_ValueChanged;

        public void AddHandler(Action<T> handler)
        {
            m_ValueChanged += handler;
        }

        public void RemoveHandler(Action<T> handler)
        {
            m_ValueChanged -= handler;
        }


    }


}
