using System;

namespace MatchQuest.Core.Helpers;

public class FileHelper
{
    public static string ImageToBase64(string imagePath)
    {
        var imageBytes = System.IO.File.ReadAllBytes(imagePath);
        return Convert.ToBase64String(imageBytes);
    }
}