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
            var fontResolverInfo = base.ResolveTypeface(familyName, isBold, isItalic);
            if (fontResolverInfo != null)
                return fontResolverInfo;
            
            /*var fontNames = familyName.Split(',');
            foreach (var fontName in fontNames)
            {
                var fontResolverInfo = base.ResolveTypeface(fontName.Trim(), isBold, isItalic);
                if (fontResolverInfo != null)
                    return fontResolverInfo;
            }*/

            return defaultResolver.ResolveTypeface(defaultResolver.DefaultFontName, isBold, isItalic);
        }
    }
}
