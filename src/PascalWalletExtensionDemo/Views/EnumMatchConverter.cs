using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PascalWalletExtensionDemo.Views
{
    public class EnumMatchConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return false;
            }

            var checkValue = value.ToString();
            var targetValue = parameter.ToString();
            var isVisible = checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
