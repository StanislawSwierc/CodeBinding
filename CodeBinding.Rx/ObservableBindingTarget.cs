using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reactive.Linq;
using System.Diagnostics.Contracts;
using System.Reactive.Disposables;
using System.Windows.Data;

namespace CodeBinding.Rx
{
    public class ObservableBindingTarget<T> : FrameworkElement, IObservable<T>, IDisposable
    {
        private HashSet<IObserver<T>> m_Observers;

        public ObservableBindingTarget()
        {
            m_Observers = new HashSet<IObserver<T>>();
        }

        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(T), typeof(ObservableBindingTarget<T>), new UIPropertyMetadata(OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (ObservableBindingTarget<T>)d;
            lock (instance.m_Observers)
            {
                foreach (var observer in instance.m_Observers)
                {
                    observer.OnNext(instance.Value);
                }
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            // Save observer
            lock (m_Observers)
            {
                m_Observers.Add(observer);
            }

            // Send current value to observer
            observer.OnNext(Value);
            
            return Disposable.Create(() =>
            {
                lock (m_Observers)
                {
                    m_Observers.Remove(observer);
                }
            });
        }

        public void Dispose()
        {
            lock (m_Observers)
            {
                foreach (var observer in m_Observers)
                {
                    observer.OnCompleted();
                }
                m_Observers.Clear();
            }
            BindingOperations.ClearBinding(this, ObservableBindingTarget<T>.ValueProperty);
        }
    }
}
