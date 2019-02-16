using System;
using System.Globalization;
using System.Windows.Data;

namespace aframe
{
    public class EnumToDisplayNameConverter :
        IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Enum e))
            {
                return value;
            }

            return e.ToDisplayName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value;
    }
}
