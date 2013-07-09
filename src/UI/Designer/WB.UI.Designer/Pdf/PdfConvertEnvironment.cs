using System;

namespace WB.UI.Designer.Pdf
{
    public class PdfConvertEnvironment
    {
        public String TempFolderPath { get; set; }
        public String WkHtmlToPdfPath { get; set; }
        public int Timeout { get; set; }
        public bool Debug { get; set; }
    }
}