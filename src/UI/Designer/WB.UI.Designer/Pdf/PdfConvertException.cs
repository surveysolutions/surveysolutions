using System;

namespace WB.UI.Designer.Pdf
{
    public class PdfConvertException : Exception
    {
        public PdfConvertException(String msg) : base(msg) { }
    }
}