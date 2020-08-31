using System.Linq;
using MigraDocCore.DocumentObjectModel;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.UI.Headquarters.Services.Impl;
using PdfInterviewRes = WB.Core.BoundedContexts.Headquarters.Resources.PdfInterview;

namespace WB.UI.Headquarters.PdfInterview.PdfWriters
{
    public class CommentsPdfWriter : IPdfWriter
    {
        private readonly InterviewTreeQuestion question;

        public CommentsPdfWriter(InterviewTreeQuestion question)
        {
            this.question = question;
        }

        public void Write(Paragraph paragraph)
        {
            if (question.AnswerComments != null && question.AnswerComments.Any())
            {
                paragraph.Style = PdfStyles.CommentTitle;
                paragraph.AddWrapFormattedText(PdfInterviewRes.Comments.ToUpper(), PdfStyles.CommentTitle);

                foreach (var comment in question.AnswerComments)
                {
                    paragraph.AddLineBreak();
                    paragraph.AddWrapFormattedText(comment.CommentTime.ToString(PdfDateTimeFormats.DateTimeFormat), PdfStyles.CommentDateTime);
                    paragraph.AddWrapFormattedText($" {comment.UserRole.ToUiString()}: ", PdfStyles.CommentAuthor);
                    paragraph.AddWrapFormattedText(comment.Comment, PdfStyles.CommentMessage);
                }
            }

        }
    }
}