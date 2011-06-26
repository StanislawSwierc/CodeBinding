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
    internal class ObservableValueConverter<T> : ObservableValueConverterBase<T>, IValueConverter
    {
        private IValueConverter m_Inner;

        public ObservableValueConverter(IValueConverter converter)
        {
            Contract.Requires(converter != null);

            m_Inner = converter;
        }

        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                object result = m_Inner.Convert(value, targetType, parameter, culture);
                SendNext((T)result);
            }
            catch (Exception ex)
            {
                SendError(ex);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
