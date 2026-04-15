using System;
using System.Globalization;
using System.Windows.Data;

namespace AttendanceSystem.Converters
{
    /// <summary>
    /// RadioButton の IsChecked を int プロパティとバインドするためのコンバーター
    /// </summary>
    public class RadioButtonToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Binding.DoNothing;
            if ((bool)value)
            {
                return int.Parse(parameter.ToString());
            }
            return Binding.DoNothing;
        }
    }
}
