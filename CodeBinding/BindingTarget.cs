using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace CodeBinding
{
    internal class BindingTarget : FrameworkElement
    {
        private object m_Target;
        private PropertyInfo m_Property;

        public BindingTarget(object target, PropertyInfo property)
        {
            Contract.Requires(target != null);
            Contract.Requires(property != null);

            m_Target = target;
            m_Property = property;
        }

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
           DependencyProperty.Register("Value", typeof(object), typeof(BindingTarget), new UIPropertyMetadata(OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (BindingTarget)d;
            instance.m_Property.SetValue(instance.m_Target, e.NewValue, null);
        }
    }

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
            var instance = (BindingTarget<T>)d;
            var handler = instance.m_ValueChanged;
            if (handler != null)
            {
                handler(instance.Value);
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
