namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class EmailTemplate
    {
        public const string Password = "%PASSWORD%";
        public const string SurveyLink = "%SURVEYLINK%";
        public const string SurveyName = "%SURVEYNAME%";

        public string Subject { get; }
        public string Message { get; }

        public EmailTemplate(string subject, string message)
        {
            this.Subject = subject;
            this.Message = message;
        }

        public bool HasPassword => Message.Contains(Password);
        public bool HasLink => Message.Contains(SurveyLink);
        public bool HasSurveyName => Message.Contains(SurveyName);
    }

    public class UserEmail
    {
        private UserEmail(string subject, string message)
        {
            this.Subject = subject;
            this.Message = message;
        }

        public string Subject { get; private set;}
        public string Message { get; private set; }

        public static UserEmail FromTemplate(EmailTemplate emailTemplate)
        {
            var userEmail = new UserEmail(emailTemplate.Subject, emailTemplate.Message);
            return userEmail;
        }

        public UserEmail SubstitutePassword(string password)
        {
            this.Message = this.Message.Replace(EmailTemplate.Password, password);
            return this;
        }

        public UserEmail SubstituteLink(string link)
        {
            this.Message = this.Message.Replace(EmailTemplate.SurveyLink, link);
            return this;
        }

        public UserEmail SubstituteSurveyName(string surveyName)
        {
            this.Message = this.Message.Replace(EmailTemplate.SurveyName, surveyName);
            this.Subject = this.Subject.Replace(EmailTemplate.SurveyName, surveyName);
            return this;
        }
    }
}
