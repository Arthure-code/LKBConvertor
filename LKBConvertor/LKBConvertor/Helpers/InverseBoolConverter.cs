using System.Globalization;

namespace LKBConvertor.Helpers
{
    public class InverseBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType,
            object? parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return value;
        }

        // ConvertBack délègue à Convert car inverser un bool s'inverse
        // symétriquement (aucune divergence de logique volontaire).
        public object? ConvertBack(object? value, Type targetType,
            object? parameter, CultureInfo culture) =>
            Convert(value, targetType, parameter, culture);
    }
}
