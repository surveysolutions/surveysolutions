namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class WebInterviewEmailTemplate
    {
        public const string Password = "%PASSWORD%";
        public const string SurveyLink = "%SURVEYLINK%";
        public const string SurveyName = "%SURVEYNAME%";

        public string Subject { get; }
        public string Message { get; }

        public WebInterviewEmailTemplate(string subject, string message)
        {
            this.Subject = subject;
            this.Message = message;
        }

        public bool HasPassword => Message.Contains(Password);
        public bool HasLink => Message.Contains(SurveyLink);
        public bool HasSurveyName => Message.Contains(SurveyName);
    }

    public class PersonalizedWebInterviewEmail
    {
        private PersonalizedWebInterviewEmail(string subject, string message, string messageWithPassword)
        {
            this.Subject = subject;
            this.Message = message;
            this.MessageWithPassword = messageWithPassword;
        }

        public string Subject { get; private set;}

        public string Message { get; private set; }
        public string MessageWithPassword { get; private set; }

        public string MessageHtml => Message;
        public string MessageWithPasswordHtml => MessageWithPassword;

        public static PersonalizedWebInterviewEmail FromTemplate(WebInterviewEmailTemplate webInterviewEmailTemplate)
        {
            var userEmail = new PersonalizedWebInterviewEmail(webInterviewEmailTemplate.Subject, webInterviewEmailTemplate.Message, webInterviewEmailTemplate.Message);
            return userEmail;
        }

        public PersonalizedWebInterviewEmail SubstitutePassword(string password)
        {
            this.Message = this.Message.Replace(WebInterviewEmailTemplate.Password, password);
            this.MessageWithPassword = this.MessageWithPassword.Replace(WebInterviewEmailTemplate.Password, password);
            return this;
        }

        public PersonalizedWebInterviewEmail SubstituteLink(string link)
        {
            this.Message = this.Message.Replace(WebInterviewEmailTemplate.SurveyLink, link);
            this.MessageWithPassword = this.MessageWithPassword.Replace(WebInterviewEmailTemplate.SurveyLink, link);
            return this;
        }

        public PersonalizedWebInterviewEmail SubstituteSurveyName(string surveyName)
        {
            this.Message = this.Message.Replace(WebInterviewEmailTemplate.SurveyName, surveyName);
            this.MessageWithPassword = this.MessageWithPassword.Replace(WebInterviewEmailTemplate.SurveyName, surveyName);
            this.Subject = this.Subject.Replace(WebInterviewEmailTemplate.SurveyName, surveyName);
            return this;
        }
    }
}
