using System.Globalization;

public static class LanguageExtensions
{
    public static string ToEnglish(this string language)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(language);
            string native = culture.IsNeutralCulture ? culture.EnglishName : culture.Parent.EnglishName;

            return char.ToUpper(native[0]) + native.Substring(1);
        }
        catch
        {
            return language;
        }
    }

    public static string ToNative(this string language)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(language);
            string native = culture.IsNeutralCulture ? culture.NativeName : culture.Parent.NativeName;

            if (string.IsNullOrEmpty(native))
                return language;

            return char.ToUpper(native[0]) + native.Substring(1);
        }
        catch
        {
            return language;
        }
    }
}