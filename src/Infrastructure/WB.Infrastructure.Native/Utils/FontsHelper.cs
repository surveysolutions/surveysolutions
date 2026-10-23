using System;
using System.Linq;
using SkiaSharp;

namespace WB.Infrastructure.Native.Utils;

public static class FontsHelper
{
    //first font in the environment cold be not be suitable 
    //Windows and Linux have different default fonts
    //so we need to find the first suitable font
    //looking for default font in Linux or first if it doesn't exist 
    private const string LinuxFont = "DejaVu Sans";

    private static readonly Lazy<string> defaultFontName = new(FindDefaultFontName);

    public static string DefaultFontName => defaultFontName.Value;

    private static string FindDefaultFontName()
    {
        var families = SKFontManager.Default.GetFontFamilies();

        // if there are no fonts at all, the name is still valid for the graphic engine, it will use the embedded font
        return families.FirstOrDefault(family => string.Equals(family, LinuxFont, StringComparison.OrdinalIgnoreCase))
               ?? families.FirstOrDefault()
               ?? LinuxFont;
    }
}
