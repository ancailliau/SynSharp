using System.Text.RegularExpressions;

namespace Synsharp.Telepath;

internal class UrlHelper
{
    private static readonly Regex Regex = new(Pattern);
    private const string Pattern = @"^(?<front>.+?://.+?:)[^/]+?(?=@)";
    private const string Substitution = @"${front}****";
    public static string SanitizeUrl(string input)
    {
        return Regex.Replace(input, Substitution);
    }
}