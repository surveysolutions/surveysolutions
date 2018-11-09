using System.IO;
using System.Linq;
using cs_pdf_to_image;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.UI.Designer.Implementation.Services
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
#warning Enable 32-Bit Applications should be set to true for IIS Application Pool
                var errors = Pdf2Image.Convert(tempPdfFile, tempPdfImage);

                return errors.Any() ? null : File.ReadAllBytes(tempPdfImage);
            }
            catch
            {
                return null;
            }
            finally
            {
                File.Delete(tempPdfFile);
                File.Delete(tempPdfImage);
            }
        }
    }
}
