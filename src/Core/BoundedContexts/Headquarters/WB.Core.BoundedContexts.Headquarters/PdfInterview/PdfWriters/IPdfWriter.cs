#nullable enable

using MigraDoc.DocumentObjectModel;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters
{
    public interface IPdfWriter
    {
        void Write(Paragraph paragraph);
    }
}