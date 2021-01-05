#nullable enable

using System.Linq;
using Main.Core.Entities.SubEntities;
using MigraDocCore.DocumentObjectModel;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using PdfInterviewRes = WB.Core.BoundedContexts.Headquarters.Resources.PdfInterview;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters
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
                    paragraph.AddWrapFormattedText(comment.CommentTime.ToString(DateTimeFormat.DateTimeWithTimezoneFormat), PdfStyles.CommentDateTime);
                    paragraph.AddWrapFormattedText($" {ToUiString(comment.UserRole)}: ", PdfStyles.CommentAuthor);
                    paragraph.AddWrapFormattedText(comment.Comment, PdfStyles.CommentMessage);
                }
            }
        }
        
        private string? ToUiString(UserRoles userRole) => 
            PdfInterviewRes.ResourceManager.GetString($"UserRoles_{userRole.ToString()}");
    }
}
