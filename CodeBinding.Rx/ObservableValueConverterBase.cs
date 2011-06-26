using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Reactive.Disposables;
using System.Diagnostics.Contracts;

namespace CodeBinding.Rx
{
    internal abstract class ObservableValueConverterBase<T> : IObservable<T>
    {
        private Action<T> m_OnNext;
        private Action<Exception> m_OnError;
        
        protected void SendNext(T value)
        {
            var handler = m_OnNext;
            if (handler != null)
            {
                handler(value);
            }
        }

        protected void SendError(Exception error)
        {
            Action<Exception> handler = null;
            lock (this)
            {
                // Save current subscribers
                handler = m_OnError;

                // Remove all subscribers
                m_OnError = null;
                m_OnNext = null;

            }
            if (handler != null)
            {
                handler(error);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            Contract.Requires(observer != null);

            // Lock to make sure m_OnNext and m_OnError are managed together
            lock (this)
            {
                m_OnNext += observer.OnNext;
                m_OnError += observer.OnError;
            }

            return Disposable.Create(() =>
                {
                    // Check if handlers have been cleared already
                    if (m_OnError != null)
                    {
                        lock (this)
                        {
                            m_OnNext -= observer.OnNext;
                            m_OnError -= observer.OnError;
                        }
                    }
                }
            );
        }
    }
}
