#nullable enable

using MigraDocCore.DocumentObjectModel;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters
{
    public class GroupPdfWriter : IPdfWriter
    {
        private readonly InterviewTreeGroup @group;

        public GroupPdfWriter(InterviewTreeGroup @group)
        {
            this.@group = @group;
        }

        public void Write(Paragraph paragraph)
        {
            var title = @group.Title.Text.RemoveHtmlTags();

            paragraph.Style = PdfStyles.GroupHeader;
            paragraph.AddWrappedText(title);
        }
    }
}