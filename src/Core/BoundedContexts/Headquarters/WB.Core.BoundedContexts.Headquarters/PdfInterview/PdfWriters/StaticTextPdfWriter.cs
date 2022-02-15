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
                var attachmentInfo = questionnaire.GetAttachmentById(attachmentId.Value);
                var attachment = attachmentContentService.GetAttachmentContent(attachmentInfo.ContentId);
                if (attachment == null)
                    throw new ArgumentException($"Unknown attachment. AttachmentId: {attachmentInfo.AttachmentId}. ContentId: {attachmentInfo.ContentId}. StaticText: {staticText.Identity}. Interview: {interview.Id}. Questionnaire: {interview.QuestionnaireIdentity}.");
                
                paragraph.AddLineBreak();

                if (attachment.IsImage())
                {
                    paragraph.Format.LineSpacingRule = LineSpacingRule.Single;
                    
                    ImageSource.IImageSource imageSource = ImageSource.FromBinary(attachment.FileName, 
                        () => attachment.Content);

                    var image = paragraph.AddImage(imageSource);
                    image.LockAspectRatio = true;
                    image.Width = Unit.FromPoint(300);
                    image.Height = Unit.FromPoint(300);
                }
                else if (attachment.IsVideo())
                {
                    paragraph.AddWrapFormattedText($"{attachment.FileName}", PdfStyles.QuestionAnswer);
                }
                else if (attachment.IsAudio())
                {
                    paragraph.AddWrapFormattedText($"{attachment.FileName}", PdfStyles.QuestionAnswer);
                }
                else if (attachment.IsPdf())
                {
                    paragraph.AddWrapFormattedText($"{attachment.FileName}", PdfStyles.QuestionAnswer);
                }
            }
        }

    }
}