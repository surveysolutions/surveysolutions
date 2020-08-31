using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.UI.Headquarters.PdfInterview.PdfWriters
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
            
            if (@group is InterviewTreeRoster roster)
            {
                paragraph.AddFormattedText(" - " + roster.RosterTitle.RemoveHtmlTags(), PdfStyles.RosterTitle);
            }
        }
    }
}