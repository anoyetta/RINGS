using System;
using System.Globalization;
using System.Windows.Data;

namespace aframe
{
    public class FontSizeToLargeConverter :
        IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double size))
            {
                return value;
            }

            var rate = Constants.FontSizeScalingRate;
            if (parameter != null &&
                parameter is double rateParam)
            {
                rate = rateParam;
            }

            return size + (size * rate);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double size))
            {
                return value;
            }

            var rate = Constants.FontSizeScalingRate;
            if (parameter != null &&
                parameter is double rateParam)
            {
                rate = rateParam;
            }

            return size - (size * rate);
        }
    }
}
