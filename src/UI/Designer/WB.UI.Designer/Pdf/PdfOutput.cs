using System;
using System.IO;

namespace WB.UI.Designer.Pdf
{
    public class PdfOutput
    {
        public String OutputFilePath { get; set; }
        public Stream OutputStream { get; set; }
        public Action<PdfDocument, byte[]> OutputCallback { get; set; }
    }
}