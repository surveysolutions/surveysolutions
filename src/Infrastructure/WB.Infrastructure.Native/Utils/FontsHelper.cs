using System.Linq;
using SixLabors.Fonts;

namespace WB.Infrastructure.Native.Utils;

public static class FontsHelper
{
    //first font in the environment cold be not be suitable 
    //Windows and Linux have different default fonts
    //so we need to find the first suitable font
    //looking for default font in Linux or first if it doesn't exist 
    private const string LinuxFont = "DejaVu Sans";
    
    public static string DefaultFontName => SystemFonts.Collection.TryGet(LinuxFont, out var fontFamily) ? fontFamily.Name :
        SystemFonts.Collection.Families.First().Name;
}
