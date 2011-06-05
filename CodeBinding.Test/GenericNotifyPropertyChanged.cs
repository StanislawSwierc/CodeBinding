using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace CodeBinding.Test
{
    public class GenericNotifyPropertyChanged<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private T m_Value;
        public T Value
        {
            get { return m_Value; }
            set
            {
                if ((m_Value==null && value != null) || !m_Value.Equals(value))
                {
                    m_Value = value;
                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Value"));
                    }
                }
            }
        }
    }
}
