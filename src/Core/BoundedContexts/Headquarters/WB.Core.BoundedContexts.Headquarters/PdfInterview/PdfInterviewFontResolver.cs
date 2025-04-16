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

        public static string GetFontNameForText(string text)
        {
            var notoSansFontName = GetNotoSansFontNameForText(text);
            var fontFamily = $"{notoSansFontName}, {DefinePdfStyles.DefaultFonts}";
            return fontFamily;
        }

        private static string GetNotoSansFontNameForText(string text)
        {
            foreach (char ch in text)
            {
                int code = ch;

                if (code >= 0x0900 && code <= 0x097F) return "Noto Sans Devanagari";
                if (code >= 0x4E00 && code <= 0x9FFF) return "Noto Sans CJK SC";
                if (code >= 0x3040 && code <= 0x30FF) return "Noto Sans JP";
                if (code >= 0xAC00 && code <= 0xD7AF) return "Noto Sans KR";
                if (code >= 0x1200 && code <= 0x137F) return "Noto Sans Ethiopic";
                if (code >= 0x1780 && code <= 0x17FF) return "Noto Sans Khmer";
                if (code >= 0x0590 && code <= 0x05FF) return "Noto Sans Hebrew";
                if (code >= 0x0600 && code <= 0x06FF) return "Noto Sans Arabic";
            }

            return "Noto Sans"; // default fallback
        }
    }
}
