using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Data;

namespace CodeBinding
{
    public class DelegateMultiValueConverter : IMultiValueConverter
    {
        private Delegate m_Delegate;
        private Type[] m_ParametersType;
        private object[] m_ParametersDefaultValues;

        public DelegateMultiValueConverter(Delegate converter)
        {
            m_Delegate = converter;
            m_ParametersType = m_Delegate.Method.GetParameters().Skip(1).Select(p => p.ParameterType).ToArray();
        }

        private void CreateParameterDefaultValues()
        {
            m_ParametersDefaultValues = new object[m_ParametersType.Length];
            for (int i = 0; i < m_ParametersType.Length; i++)
            {
                m_ParametersDefaultValues[i] = m_ParametersType[i].IsValueType ? Activator.CreateInstance(m_ParametersType[i]) : null;
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object result = null;
            if (targetType != m_Delegate.Method.ReturnType)
            {
                throw new ArgumentException("targetType");
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == System.Windows.DependencyProperty.UnsetValue)
                {
                    // Create default values first time they are needed
                    if (m_ParametersDefaultValues == null) CreateParameterDefaultValues();
                    values[i] = m_ParametersDefaultValues[i];
                }
            }

            try
            {
                result = m_Delegate.DynamicInvoke(values);
            }
            catch (TargetInvocationException ex)
            {
                Debug.WriteLine(ex.Message);
                result = System.Windows.DependencyProperty.UnsetValue;
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
