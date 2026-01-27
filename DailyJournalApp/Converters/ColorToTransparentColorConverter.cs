using System.Globalization;
using Microsoft.Maui.Graphics;

namespace DailyJournalApp.Converters
{
    public class ColorToTransparentColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Colors.Transparent;

            Color color;
            if (value is Color c)
            {
                color = c;
            }
            else if (value is string s && !string.IsNullOrEmpty(s))
            {
                color = Color.FromArgb(s);
            }
            else
            {
                return Colors.Transparent;
            }

            // Return with 15% opacity
            return color.WithAlpha(0.15f);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
