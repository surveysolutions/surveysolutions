#nullable enable

using System;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters
{
    public class StaticTextPdfWriter : IPdfWriter
    {
        private readonly InterviewTreeStaticText staticText;
        private readonly IStatefulInterview interview;
        private readonly IQuestionnaire questionnaire;
        private readonly IAttachmentContentService attachmentContentService;

        public StaticTextPdfWriter(InterviewTreeStaticText staticText,
            IStatefulInterview interview, IQuestionnaire questionnaire,
            IAttachmentContentService attachmentContentService)
        {
            this.staticText = staticText;
            this.interview = interview;
            this.questionnaire = questionnaire;
            this.attachmentContentService = attachmentContentService;
        }

        public void Write(Paragraph paragraph)
        {
            paragraph.Style = PdfStyles.StaticTextTitle;
            paragraph.AddWrapFormattedText(staticText.Title.Text.RemoveHtmlTags(), PdfStyles.StaticTextTitle);

            var attachmentId = interview.GetAttachmentForEntity(staticText.Identity);
            if (attachmentId != null)
            {
                new AttachmentPdfWriter(attachmentId, interview, questionnaire, attachmentContentService)
                    .Write(paragraph);
                paragraph.AddLineBreak();
            }
        }
    }
}