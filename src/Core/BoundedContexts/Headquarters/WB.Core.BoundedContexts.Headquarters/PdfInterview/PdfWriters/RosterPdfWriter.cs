#nullable enable

using MigraDocCore.DocumentObjectModel;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters
{
    public class RosterPdfWriter : IPdfWriter
    {
        private readonly InterviewTreeRoster roster;
        private readonly IQuestionnaire questionnaire;

        public RosterPdfWriter(InterviewTreeRoster roster, IQuestionnaire questionnaire)
        {
            this.roster = roster;
            this.questionnaire = questionnaire;
        }

        public void Write(Paragraph paragraph)
        {
            var title = this.roster.Title.Text.RemoveHtmlTags();

            paragraph.Style = PdfStyles.GroupHeader;
            paragraph.AddWrappedText(title);
            
            if (!questionnaire.HasCustomRosterTitle(this.roster.Identity.Id))
                paragraph.AddFormattedText(" - " + roster.RosterTitle, PdfStyles.RosterTitle);
        }
    }
}