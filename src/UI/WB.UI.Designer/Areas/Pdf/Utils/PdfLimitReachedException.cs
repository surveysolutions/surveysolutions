using System;

namespace WB.UI.Designer.Areas.Pdf.Utils
{
    public class PdfLimitReachedException : Exception
    {
        public int UserLimit { get; }
        
        public PdfLimitReachedException(int userLimit)
            : base($"User has reached the PDF generation limit of {userLimit}")
        {
            UserLimit = userLimit;
        }
    }
}
