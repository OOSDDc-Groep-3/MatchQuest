using System.Globalization;
using Microsoft.Maui.Controls;
using MatchQuest.Core.Helpers;

namespace MatchQuest.App.Converters;

public class ProfilePictureConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string profilePicture || string.IsNullOrEmpty(profilePicture))
            return ImageSource.FromFile("showcaseprofile.png");

        // Try to convert from Base64 using FileHelper
        var imageBytes = FileHelper.Base64ToImageBytes(profilePicture);
        if (imageBytes != null)
        {
            return ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }

        // It's a file path
        return ImageSource.FromFile(profilePicture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}