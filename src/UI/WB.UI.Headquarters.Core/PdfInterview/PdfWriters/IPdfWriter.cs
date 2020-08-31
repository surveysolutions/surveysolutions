using MigraDocCore.DocumentObjectModel;

namespace WB.UI.Headquarters.PdfInterview.PdfWriters
{
    public interface IPdfWriter
    {
        void Write(Paragraph paragraph);
    }
}