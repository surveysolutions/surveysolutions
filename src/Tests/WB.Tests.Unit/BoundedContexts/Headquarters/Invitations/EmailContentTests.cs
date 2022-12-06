using System;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestFixture]
    public class EmailContentTests
    {
        [Test]
        public void when_insert_interview_data_in_text_email()
        {
            string subject = string.Empty, password = string.Empty, link = string.Empty;
            string text = @"text
                    your answer %a% 
                    barcode %a:barcode%
                    qrcode %a:qrcode%
                    thanks" ;

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: Id.g1, variable: "a")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Create.AggregateRoot.StatefulInterview(interviewId: Id.g2, questionnaire: questionnaireDocument);
            interview.AnswerTextQuestion(Guid.NewGuid(), Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, "answer");

            var template = new WebInterviewEmailTemplate(subject, text, password, link);
            
            EmailContent emailContent = new EmailContent(template, "qtitle", "", "");
            emailContent.AttachmentMode = EmailContentAttachmentMode.Base64String;
            emailContent.TextMode = EmailContentTextMode.Text;
            
            emailContent.RenderInterviewData(interview, questionnaire);
            
            Assert.That(emailContent.MainText, Is.EqualTo(@"text
                    your answer answer 
                    barcode answer
                    qrcode answer
                    thanks"));
        }

        [Test]
        public void when_insert_interview_data_in_html_email()
        {
            string subject = string.Empty, password = string.Empty, link = string.Empty;
            string text = @"text
                    your answer %a% 
                    barcode %a:barcode%
                    qrcode %a:qrcode%
                    thanks" ;

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: Id.g1, variable: "a")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Create.AggregateRoot.StatefulInterview(interviewId: Id.g2, questionnaire: questionnaireDocument);
            interview.AnswerTextQuestion(Guid.NewGuid(), Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, "answer");

            var template = new WebInterviewEmailTemplate(subject, text, password, link);
            
            EmailContent emailContent = new EmailContent(template, "qtitle", "", "");
            emailContent.AttachmentMode = EmailContentAttachmentMode.Base64String;
            emailContent.TextMode = EmailContentTextMode.Html;
            
            emailContent.RenderInterviewData(interview, questionnaire);

            Assert.That(emailContent.MainText.Contains("your answer answer "), Is.True);
            Assert.That(emailContent.MainText.Contains("barcode <img src='data:image/jpeg;base64,/9j/"), Is.True);
            Assert.That(emailContent.MainText.Contains("qrcode <img src='data:image/jpeg;base64,/9j/"), Is.True);
        }

        [Test]
        public void when_insert_interview_data_in_html_email_with_attachment()
        {
            string subject = string.Empty, password = string.Empty, link = string.Empty;
            string text = @"text
                    your answer %a% 
                    barcode %a:barcode%
                    qrcode %a:qrcode%
                    thanks" ;

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: Id.g1, variable: "a")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Create.AggregateRoot.StatefulInterview(interviewId: Id.g2, questionnaire: questionnaireDocument);
            interview.AnswerTextQuestion(Guid.NewGuid(), Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, "answer");

            var template = new WebInterviewEmailTemplate(subject, text, password, link);
            
            EmailContent emailContent = new EmailContent(template, "qtitle", "", "");
            emailContent.AttachmentMode = EmailContentAttachmentMode.InlineAttachment;
            emailContent.TextMode = EmailContentTextMode.Html;
            
            emailContent.RenderInterviewData(interview, questionnaire);

            Assert.That(emailContent.Attachments.Count, Is.EqualTo(2));
            Assert.That(emailContent.Attachments[0].Disposition, Is.EqualTo(EmailAttachmentDisposition.Inline));
            Assert.That(emailContent.Attachments[1].Disposition, Is.EqualTo(EmailAttachmentDisposition.Inline));
            Assert.That(emailContent.MainText, Is.EqualTo(@$"text
                    your answer answer 
                    barcode <img src='cid:{emailContent.Attachments[0].ContentId}'/>
                    qrcode <img src='cid:{emailContent.Attachments[1].ContentId}'/>
                    thanks"));
        }
    }
}
