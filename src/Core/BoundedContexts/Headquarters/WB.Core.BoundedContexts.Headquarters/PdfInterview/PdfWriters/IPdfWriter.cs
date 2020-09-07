#nullable enable

using MigraDocCore.DocumentObjectModel;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters
{
    public interface IPdfWriter
    {
        void Write(Paragraph paragraph);
    }
}