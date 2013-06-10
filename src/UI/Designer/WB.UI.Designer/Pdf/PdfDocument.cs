using System;

namespace WB.UI.Designer.Pdf
{
    public class PdfDocument
    {
        public PdfDocument()
        {
            this.PageNumbersFormat = "Page [page] of [toPage]";
        }

        public String Url { get; set; }
        public String HeaderUrl { get; set; }
        public String FooterUrl { get; set; }
        public object State { get; set; }
        public string PageNumbersFormat { get; set; }
        public string CoverUrl { get; set; }
    }
}