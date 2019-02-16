using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace aframe
{
    public class DayOfWeekToBrushConverter :
        IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime date))
            {
                return null;
            }

            switch (date.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    return Brushes.Blue;

                case DayOfWeek.Sunday:
                    return Brushes.Red;

                default:
                    return Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => value;
    }
}
