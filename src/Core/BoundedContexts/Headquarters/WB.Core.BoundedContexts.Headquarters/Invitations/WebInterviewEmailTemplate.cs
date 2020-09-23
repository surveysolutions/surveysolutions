using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using ZXing;
using ZXing.QrCode;

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
        public string HtmlSubject { get; private set; }
        public string HtmlMainText { get; private set; }
        public string PasswordDescription { get; }
        public string LinkText { get; }
        
        public List<EmailAttachment> Attachments { get; } = new List<EmailAttachment>();

        public void RenderInterviewData(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            HtmlSubject = ReplaceVariablesWithData(Subject, interview, questionnaire);
            HtmlMainText = ReplaceVariablesWithData(MainText, interview, questionnaire);
            Subject = ReplaceVariablesWithData(Subject, interview, questionnaire);
            MainText = ReplaceVariablesWithData(MainText, interview, questionnaire);
        }

        private static readonly Regex FindVariables = new Regex("%[A-Za-z0-9_]+(:[a-z]+)?%", RegexOptions.Compiled);
        private string ReplaceVariablesWithData(string text, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            return FindVariables.Replace(text, match =>
            {
                var variableWithMode = match.Value.Trim('%').Split(':');
                var variable = variableWithMode[0];
                var questionId = questionnaire.GetQuestionIdByVariable(variable);
                if (!questionId.HasValue)
                    return String.Empty;
                
                var answer = interview.GetAnswerAsString(new Identity(questionId.Value, RosterVector.Empty));

                if (variableWithMode.Length > 0)
                {
                    var displayMode = variableWithMode[1];
                    
                    if (displayMode == "barcode" || displayMode == "qrcode")
                    {
                        var imageStream = displayMode == "barcode"
                            ? RenderBarCode(answer)
                            : RenderQrCode(answer);

                        switch (AttachmentMode)
                        {
                            case EmailContentAttachmentMode.InlineAttachment:
                            {
                                var attachment = CreateAttachment(imageStream);
                                Attachments.Add(attachment);
                                return $"<img src='cid:{attachment.ContentId}'/>";
                            }
                            case EmailContentAttachmentMode.Base64String:
                            {
                                var base64String = Convert.ToBase64String(imageStream.ToArray());
                                return $"<img src='data:image/png;base64,{base64String}'/>";
                            }
                            default:
                                throw new ArgumentException($"Unsupported attachment mode {AttachmentMode}");
                        }
                    }
                }
                
                return answer;
            });
        }

        private EmailAttachment CreateAttachment(MemoryStream imageStream)
        {
            var id = Guid.NewGuid();
            
            return new EmailAttachment()
            {
                Base64String = Convert.ToBase64String(imageStream.ToArray()),
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

            Bitmap imageBitmap = new Bitmap(bmWidth, height, PixelFormat.Format32bppRgb);

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

        private MemoryStream RenderQrCode(string text)
        {
            var width = 250;
            var height = 250;
            
            QRCodeWriter writer = new QRCodeWriter();
            var bm = writer.encode(text, BarcodeFormat.QR_CODE, width, height);
            int bmWidth = bm.Width;
            int bmHeight = bm.Height;

            Bitmap imageBitmap = new Bitmap(bmWidth, bmHeight, PixelFormat.Format32bppRgb);
            for (int x = 0; x < bmWidth; x++) 
            {
                for (int y = 0; y < bmHeight; y++)
                {
                    var color = bm[x, y] ? Color.Black : Color.White;
                    imageBitmap.SetPixel(x, y, color);
                }
            }
            
            //imageBitmap.Save("c:\\Temp\\qrcode.jpeg", ImageFormat.Jpeg);

            var ms = new MemoryStream();
            imageBitmap.Save(ms, ImageFormat.Jpeg);
            return ms;
        }
    }
}


