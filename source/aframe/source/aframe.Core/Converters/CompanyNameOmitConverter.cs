using System;
using System.Globalization;
using System.Windows.Data;

namespace aframe
{
    /// <summary>
    /// 会社名の称号をカットするコンバーター
    /// </summary>
    /// <remarks>
    /// 株式会社ほげ -> ほげ
    /// 有限会社ふー -> ふー
    /// (株)ほげ -> ほげ
    /// (有)ふー -> ふー
    /// </remarks>
    public class CompanyNameOmitConverter :
        IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string text))
            {
                return value;
            }

            return CompanyNameConverter.Omit(text);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value;
    }
}
