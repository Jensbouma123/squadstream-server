using System.Text.RegularExpressions;

namespace Web_API_new.Utilities;

public static class SanitizationUtility
{
    public static string SanitizeInput(string input)
    {
        string sanitizedInput = Regex.Replace(input, @"<[^>]+>", string.Empty); // Remove HTML tags

        sanitizedInput = Regex.Replace(sanitizedInput, @"[^\p{L}\s'\-0-9]", string.Empty); // Allow letters, spaces, single quotes, hyphens, and numbers

        return sanitizedInput;
    }

    public static string RemoveHtmlTags(string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }

}