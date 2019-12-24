using System;

namespace Activision_Mendeleyev_table.HelperClasses
{
    public class RoundConverter : System.Windows.Data.IValueConverter
    {
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
            throw new NotImplementedException();
        }
    }
}
