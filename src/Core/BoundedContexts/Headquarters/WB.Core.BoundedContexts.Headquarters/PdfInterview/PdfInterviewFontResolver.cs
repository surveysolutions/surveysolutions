#nullable enable

using PdfSharpCore.Fonts;
using PdfSharpCore.Utils;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview
{
    public class PdfInterviewFontResolver : FontResolver
    {
        readonly FontResolver defaultResolver = new FontResolver();
        
        public PdfInterviewFontResolver()
        {
            NullIfFontNotFound = true;
        }

        public override FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var fontNames = familyName.Split(',');
            foreach (var fontName in fontNames)
            {
                var trimFontName = fontName.Trim();
                var fontResolverInfo = base.ResolveTypeface(trimFontName, isBold, isItalic);
                if (fontResolverInfo != null)
                    return fontResolverInfo;
            }

            var defaultTypeface = defaultResolver.ResolveTypeface(defaultResolver.DefaultFontName, isBold, isItalic);
            return defaultTypeface;
        }
    }
}
