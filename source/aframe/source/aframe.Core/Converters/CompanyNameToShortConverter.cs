using System;
using System.Globalization;
using System.Windows.Data;

namespace aframe
{
    /// <summary>
    /// 会社名の称号を省略表記にするコンバーター
    /// </summary>
    /// <remarks>
    /// 株式会社 -> (株)
    /// 有限会社 -> (有)
    /// </remarks>
    public class CompanyNameToShortConverter :
        IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string text))
            {
                return value;
            }

            return CompanyNameConverter.ToShort(text);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value;
    }
}
