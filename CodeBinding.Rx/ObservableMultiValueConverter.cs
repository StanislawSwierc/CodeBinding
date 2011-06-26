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
    internal class ObservableMultiValueConverter<T> : ObservableValueConverterBase<T>, IMultiValueConverter
    {
        private IMultiValueConverter m_Inner;

        public ObservableMultiValueConverter(IMultiValueConverter converter)
        {
            Contract.Requires(converter != null);

            m_Inner = converter;
        }

        #region IMultiValueConverter Implementation

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                object result = m_Inner.Convert(values, targetType, parameter, culture);
                SendNext((T)result);
            }
            catch (Exception ex)
            {
                SendError(ex);
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException();
        }

        #endregion
    }
}
