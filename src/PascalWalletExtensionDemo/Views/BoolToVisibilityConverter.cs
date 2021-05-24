using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PascalWalletExtensionDemo.Views
{
    public class BoolToVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string)
            {
                var param = bool.Parse(parameter as string);
                var val = (bool)value;

                return val == param ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
