using System.IO;
using PdfSharpCore.Fonts;
using PdfSharpCore.Utils;

namespace WB.UI.Headquarters.PdfInterview
{
    public class PdfInterviewFontResolver : FontResolver
    {
        public PdfInterviewFontResolver()
        {
            NullIfFontNotFound = true;
        }

        public override FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var fontNames = familyName.Split(',');
            foreach (var fontName in fontNames)
            {
                var fontResolverInfo = base.ResolveTypeface(fontName.Trim(), isBold, isItalic);
                if (fontResolverInfo != null)
                    return fontResolverInfo;
            }

            return null;
        }
    }
}