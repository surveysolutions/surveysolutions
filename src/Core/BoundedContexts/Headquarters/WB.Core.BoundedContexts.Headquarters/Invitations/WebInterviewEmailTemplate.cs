using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using ZXing;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class WebInterviewEmailTemplate
    {
        public string Subject { get; }
        public string MainText { get; }
        public string PasswordDescription { get; }
        public string LinkText { get; }

        public WebInterviewEmailTemplate(string subject, string mainText, string passwordDescription, string linkText)
        {
            this.Subject = subject;
            this.MainText = mainText;
            this.PasswordDescription = passwordDescription;
            this.LinkText = linkText;
        }
    }

    public enum EmailContentAttachmentMode
    {
        Base64String,
        InlineAttachment,
    }

    public class EmailContent
    {
        private const string Password = "%PASSWORD%";
        private const string SurveyLink = "%SURVEYLINK%";
        private const string SurveyName = "%SURVEYNAME%";
        private const string QuestionnaireTitle = "%QUESTIONNAIRE%";

        public EmailContentAttachmentMode AttachmentMode { get; set; } = EmailContentAttachmentMode.Base64String;
        
        public EmailContent(WebInterviewEmailTemplate template, string questionnaireTitle, string link, string password)
        {
            Subject = template.Subject
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
            MainText = template.MainText
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle)
                .Replace(SurveyLink, link)
                .Replace(Password, password);
            LinkText = template.LinkText?
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
            PasswordDescription = template.PasswordDescription?
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
        }

        public string Subject { get; private set; }
        public string MainText { get; private set; }
        public string PasswordDescription { get; }
        public string LinkText { get; }
        
        public List<EmailAttachment> Attachments { get; } = new List<EmailAttachment>();

        public void RenderInterviewData(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            Subject = ReplaceVariablesWithData(Subject, interview, questionnaire);
            MainText = ReplaceVariablesWithData(MainText, interview, questionnaire);
        }

        private static Regex FindBarcodeVariables = new Regex("%[A-Za-z0-9_]+:barcode%", RegexOptions.Compiled);
        private static Regex FindVariables = new Regex("%[A-Za-z0-9_]+%", RegexOptions.Compiled);
        private string ReplaceVariablesWithData(string text, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            text = FindBarcodeVariables.Replace(text, match =>
            {
                var variable = match.Value.Trim('%').Replace(":barcode", "");
                var questionId = questionnaire.GetQuestionIdByVariable(variable);
                if (!questionId.HasValue)
                    return String.Empty;
                var answer = interview.GetAnswerAsString(new Identity(questionId.Value, RosterVector.Empty));

                switch (AttachmentMode)
                {
                    case EmailContentAttachmentMode.InlineAttachment:
                    {
                        var attachment = CreateBarCodeAttachment(answer);
                        Attachments.Add(attachment);

                        return $"<img src='cid:{attachment.ContentId}'/>";
                    }
                    case EmailContentAttachmentMode.Base64String:
                    {
                        var attachmentStream = RenderBarCode(answer);
                        var base64String = Convert.ToBase64String(attachmentStream.ToArray());
                        return $"<img src='data:image/png;base64,{base64String}'/>";
                    }
                    default:
                        throw new ArgumentException($"Unsupported attachment mode {AttachmentMode}");
                }
                
            });
            
            return FindVariables.Replace(text, match =>
            {
                var questionId = questionnaire.GetQuestionIdByVariable(match.Value.Trim('%'));
                if (!questionId.HasValue)
                    return String.Empty;
                var answer = interview.GetAnswerAsString(new Identity(questionId.Value, RosterVector.Empty));
                return answer;
            });
        }

        private EmailAttachment CreateBarCodeAttachment(string answer)
        {
            var barCodeStream = RenderBarCode(answer);
            var id = Guid.NewGuid();
            
            return new EmailAttachment()
            {
                Base64String = Convert.ToBase64String(barCodeStream.ToArray()),
                ContentType = "image/jpeg",
                Filename = id.ToString() + ".jpeg",
                ContentId = id.ToString(),
                Disposition = EmailAttachmentDisposition.Inline,
            };
        }

        private MemoryStream RenderBarCode(string text)
        {
            var width = 149;
            var height = 53;
            
            MultiFormatWriter writer = new MultiFormatWriter();
            var bm = writer.encode(text, BarcodeFormat.CODE_128, width, 1);
            int bmWidth = bm.Width;

            Bitmap imageBitmap = new Bitmap(bmWidth, height, PixelFormat.Format32bppArgb);

            for (int x = 0; x < bmWidth; x++) 
            {
                var color = bm[x, 0] ? Color.Black : Color.White;
                for (int y = 0; y < height; y++)
                    imageBitmap.SetPixel(x, y, color);
            }
            
            //imageBitmap.Save("c:\\Temp\\barcode.jpeg", ImageFormat.Jpeg);

            var ms = new MemoryStream();
            imageBitmap.Save(ms, ImageFormat.Jpeg);
            return ms;
        }
    }
}


