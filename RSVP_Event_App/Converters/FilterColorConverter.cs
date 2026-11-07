using System.Globalization;

namespace RSVP_Event_App.Converters;

// Highlights the selected filter button: true -> theme primary color, false -> gray
public class FilterColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var resources = Application.Current!.Resources;
        if (value is not true)
            return (Color)resources["Gray500"];

        return Application.Current.RequestedTheme == AppTheme.Dark
            ? (Color)resources["PrimaryDark"]
            : (Color)resources["Primary"];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
