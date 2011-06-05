using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Windows.Data;
using System.Windows;

namespace CodeBinding
{
    internal class ClearBindingDisposable: IDisposable
    {
        DependencyObject m_Target;
        DependencyProperty m_Property;
        BindingBase m_Binding;

        public ClearBindingDisposable(DependencyObject target, DependencyProperty property, BindingBase binding)
        {
            Contract.Requires(target != null);
            Contract.Requires(property != null);
            Contract.Requires(binding != null);

            m_Target = target;
            m_Property = property;
            m_Binding = binding;
        }

        public void Dispose()
        {
            if (m_Target == null) throw new ObjectDisposedException("ClearBindingDisposable");
            if (object.ReferenceEquals(BindingOperations.GetBindingBase(m_Target, m_Property), m_Binding))
            {
                BindingOperations.ClearBinding(m_Target, m_Property);
            }
            m_Target = null;
            m_Property = null;
            m_Binding = null;
        }
    }
}
