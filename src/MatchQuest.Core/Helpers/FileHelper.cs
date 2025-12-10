using System;

namespace MatchQuest.Core.Helpers;

public class FileHelper
{
    public static string ImageToBase64(string imagePath)
    {
        var imageBytes = System.IO.File.ReadAllBytes(imagePath);
        return Convert.ToBase64String(imageBytes);
    }

    public static byte[]? Base64ToImageBytes(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return null;

        // Check if it's actually a Base64 string (not a file path)
        if (base64String.Length > 100 && !base64String.Contains(".png") && !base64String.Contains(".jpg"))
        {
            try
            {
                return Convert.FromBase64String(base64String);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}