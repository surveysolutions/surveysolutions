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

    public class EmailContent
    {
        private const string Password = "%PASSWORD%";
        private const string SurveyLink = "%SURVEYLINK%";
        private const string SurveyName = "%SURVEYNAME%";
        private const string QuestionnaireTitle = "%QUESTIONNAIRE%";

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
            LinkText = template.LinkText
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
            PasswordDescription = template.PasswordDescription
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
        }

        public string Subject { get; }
        public string MainText { get; }
        public string PasswordDescription { get; }
        public string LinkText { get; }
    }
}


