using System.IO;
using cs_pdf_to_image;
using WB.Core.SharedKernels.Questionnaire.Services;

namespace WB.UI.Shared.Web.Services
{
    public class PdfConverter : IPdfConverter
    {
        public byte[] CreateThumbnail(byte[] pdfBytes)
        {
            string tempPdfFile = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.pdf");
            string tempPdfImage = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.png");

            File.WriteAllBytes(tempPdfFile, pdfBytes);

            try
            {
                var errors = Pdf2Image.Convert(tempPdfFile, tempPdfImage);
                var thumbnailBytes = File.ReadAllBytes(tempPdfImage);

                File.Delete(tempPdfImage);

                return thumbnailBytes;
            }
            catch
            {
                return null;
            }
            finally
            {
                File.Delete(tempPdfFile);
            }
        }
    }
}
