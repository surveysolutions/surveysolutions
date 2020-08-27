using System.IO;
using PdfSharpCore.Fonts;
using PdfSharpCore.Utils;

namespace WB.UI.Headquarters.PdfInterview
{
    public class PdfInterviewFontResolver : IFontResolver
    {
        static FontResolver fontResolver = new FontResolver()
        {
            NullIfFontNotFound = true
        };

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName == "Libre Barcode 128")
            {
                return new FontResolverInfo("LibreBarcode128-Regular.ttf", false, false);
            }
            
            var fontNames = familyName.Split(',');
            foreach (var fontName in fontNames)
            {
                var fontResolverInfo = fontResolver.ResolveTypeface(fontName.Trim(), isBold, isItalic);
                if (fontResolverInfo != null)
                    return fontResolverInfo;
            }

            return null;
        }

        public byte[] GetFont(string faceName)
        {
            if (faceName == "LibreBarcode128-Regular.ttf")
            {
                System.Reflection.Assembly a = typeof(PdfInterviewGenerator).Assembly;
                using Stream resFileStream = a.GetManifestResourceStream($"WB.UI.Headquarters.Content.fonts.LibreBarcode128-Regular.ttf");
                if (resFileStream == null) return null;
                byte[] ba = new byte[resFileStream.Length];
                resFileStream.Read(ba, 0, ba.Length);
                return ba;
            }

            return fontResolver.GetFont(faceName);
        }

        public string DefaultFontName => fontResolver.DefaultFontName;
    }
    
    /*public class PdfInterviewFontResolver : FontResolver
    {
        static PdfInterviewFontResolver()
        {
        }

        public PdfInterviewFontResolver()
        {
            NullIfFontNotFound = true;
        }

        public override FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName == "LibreBarcode128")
            {
                return new FontResolverInfo("LibreBarcode128");
            }
            
            var fontNames = familyName.Split(',');
            foreach (var fontName in fontNames)
            {
                var fontResolverInfo = base.ResolveTypeface(fontName.Trim(), isBold, isItalic);
                if (fontResolverInfo != null)
                    return fontResolverInfo;
            }

            return null;
        }
        
        override 
    }*/
}