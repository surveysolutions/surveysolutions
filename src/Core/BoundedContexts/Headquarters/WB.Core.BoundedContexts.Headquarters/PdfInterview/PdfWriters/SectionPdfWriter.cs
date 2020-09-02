#nullable enable

using MigraDocCore.DocumentObjectModel;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters
{
    public class SectionPdfWriter : IPdfWriter 
    {
        private readonly InterviewTreeSection interviewTreeSection;

        public SectionPdfWriter(InterviewTreeSection interviewTreeSection)
        {
            this.interviewTreeSection = interviewTreeSection;
        }

        public void Write(Paragraph paragraph)
        {
            var title = interviewTreeSection.Title.Text.RemoveHtmlTags();
            paragraph.Style = PdfStyles.SectionHeader;
            paragraph.AddBookmark(title);
            paragraph.AddWrappedText(title);
        }
    }
}