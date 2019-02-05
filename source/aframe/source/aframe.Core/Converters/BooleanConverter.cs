using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace aframe
{
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(
            T trueValue,
            T falseValue)
        {
            this.True = trueValue;
            this.False = falseValue;
        }

        public T True { get; set; }

        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool && ((bool)value) ? True : False;

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is T && EqualityComparer<T>.Default.Equals((T)value, True);
    }
}
