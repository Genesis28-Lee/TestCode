public class TypeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            NotificationType.Info => Brushes.LightBlue,
            NotificationType.Success => Brushes.LightGreen,
            NotificationType.Warning => Brushes.Gold,
            NotificationType.Error => Brushes.IndianRed,
            _ => Brushes.LightGray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
