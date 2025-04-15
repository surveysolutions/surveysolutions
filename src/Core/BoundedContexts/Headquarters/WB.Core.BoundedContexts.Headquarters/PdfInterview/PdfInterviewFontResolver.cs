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
            NullIfFontNotFound = false;
        }

        public override FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var fontResolverInfo = base.ResolveTypeface(familyName, isBold, isItalic);
            if (fontResolverInfo != null)
                return fontResolverInfo;

            return defaultResolver.ResolveTypeface(defaultResolver.DefaultFontName, isBold, isItalic);
        }
    }
}
