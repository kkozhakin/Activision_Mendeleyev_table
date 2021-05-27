using System;

namespace Activision_Mendeleyev_table.HelperClasses
{
    /// <summary>
    /// Класс, позволяющий округлять значения
    /// </summary>
    public class RoundConverter : System.Windows.Data.IValueConverter
    {
        /// <summary>
        /// Округляет значение до 4 знака после запятой
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>округленное значение</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var val = (double)value;
                return Math.Round(val, 4).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
