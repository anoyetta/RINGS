using System;
using System.Globalization;
using System.Windows.Data;

namespace aframe
{
    public class FontSizeToSmallConverter :
        IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double size))
            {
                return value;
            }

            return size / Constants.FontSizeScalingRate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double size))
            {
                return value;
            }

            return size * Constants.FontSizeScalingRate;
        }
    }
}
