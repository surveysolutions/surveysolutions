using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class EmailContent
    {
        private const string Password = "%PASSWORD%";
        private const string SurveyLink = "%SURVEYLINK%";
        private const string SurveyName = "%SURVEYNAME%";
        private const string QuestionnaireTitle = "%QUESTIONNAIRE%";

        public EmailContentAttachmentMode AttachmentMode { get; set; } = EmailContentAttachmentMode.Base64String;
        public EmailContentTextMode TextMode { get; set; } = EmailContentTextMode.Html;
        
        public EmailContent(WebInterviewEmailTemplate template, string questionnaireTitle, string link, string password)
        {
            Subject = template.Subject
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
            MainText = HttpUtility.HtmlEncode(template.MainText
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle)
                .Replace(SurveyLink, link)
                .Replace(Password, password));
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
            MainText = ReplaceVariablesWithData(MainText, interview, questionnaire);
        }

        private static readonly Regex FindVariables = 
            new Regex("%[A-Za-z0-9_]+(:[a-z]+)?%", RegexOptions.Compiled, TimeSpan.FromMilliseconds(1000));
        private string ReplaceVariablesWithData(string text, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            return FindVariables.Replace(text, match =>
            {
                var variableWithMode = match.Value.Trim('%').Split(':');
                var variable = variableWithMode[0];
                var displayMode = variableWithMode.Length > 1 ? variableWithMode[1] : null;

                string text = null;
                
                var questionId = questionnaire.GetQuestionIdByVariable(variable);
                if (questionId.HasValue)
                {
                    if (!questionnaire.IsInsideRoster(questionId.Value))
                        text = interview.GetAnswerAsString(new Identity(questionId.Value, RosterVector.Empty));
                }
                else if (questionnaire.HasVariable(variable))
                {
                    var variableId = questionnaire.GetVariableIdByVariableName(variable);
                    if (!questionnaire.IsInsideRoster(variableId))
                    {
                        var treeVariable = interview.GetVariable(new Identity(variableId, RosterVector.Empty));
                        if (treeVariable.HasValue)
                            text = treeVariable.Value.ToString();
                    }
                }
                
                if (text == null)
                    return string.Empty;

                if (TextMode == EmailContentTextMode.Html && displayMode != null)
                {
                    if (displayMode == "barcode" || displayMode == "qrcode")
                    {
                        MemoryStream imageStream;

                        try
                        {
                            imageStream = displayMode == "barcode"
                                ? QRCodeBuilder.RenderBarCodeImage(text)
                                : QRCodeBuilder.RenderQrCodeImage(text);
                        }
                        catch
                        {
                            return text;
                        }

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
                                return $"<img src='data:image/jpeg;base64,{base64String}'/>";
                            }
                            default:
                                throw new ArgumentException($"Unsupported attachment mode {AttachmentMode}");
                        }
                    }
                }
                
                return text;
            });
        }

        private EmailAttachment CreateAttachment(MemoryStream imageStream)
        {
            var id = Guid.NewGuid();
            
            return new EmailAttachment()
            {
                Content = imageStream.ToArray(),
                ContentType = "image/jpeg",
                Filename = id + ".jpeg",
                ContentId = id.ToString(),
                Disposition = EmailAttachmentDisposition.Inline,
            };
        }
    }
}
