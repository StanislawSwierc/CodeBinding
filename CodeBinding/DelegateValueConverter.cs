using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CodeBinding
{
    public class DelegateValueConverter : IValueConverter
    {
        private Delegate m_Delegate;

        public DelegateValueConverter(Delegate converter)
        {
            m_Delegate = converter;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!targetType.IsAssignableFrom(m_Delegate.Method.ReturnType))
            {
                throw new ArgumentException("targetType");
            }
            return m_Delegate.DynamicInvoke(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
