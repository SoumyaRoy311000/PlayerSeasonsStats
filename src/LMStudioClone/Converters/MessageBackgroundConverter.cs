using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LMStudioClone.Converters
{
    public class MessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isUser)
            {
                return isUser 
                    ? new SolidColorBrush(Color.FromRgb(0, 120, 215)) // Blue for user messages
                    : new SolidColorBrush(Color.FromRgb(64, 64, 64));  // Dark gray for AI messages
            }
            return new SolidColorBrush(Color.FromRgb(64, 64, 64));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

