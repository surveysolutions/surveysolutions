using System.Runtime.InteropServices;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class WebInterviewEmailTemplate
    {
        public const string Password = "%PASSWORD%";
        public const string SurveyLink = "%SURVEYLINK%";
        public const string SurveyName = "%SURVEYNAME%";

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

        public bool HasPassword => string.IsNullOrWhiteSpace(PasswordDescription);
        public bool HasLink => string.IsNullOrWhiteSpace(LinkText);
        public bool HasSurveyName => MainText.Contains(SurveyName);
    }

    public class PersonalizedWebInterviewEmail
    {
        protected PersonalizedWebInterviewEmail(string subject, string message, string messageWithPassword)
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
            var subject = webInterviewEmailTemplate.Subject;
            var messageWithoutPassword = webInterviewEmailTemplate.MainText + webInterviewEmailTemplate.LinkText;
            var messageWithPassword = webInterviewEmailTemplate.MainText + webInterviewEmailTemplate.LinkText +
                          webInterviewEmailTemplate.PasswordDescription;
            var userEmail = new PersonalizedWebInterviewEmail(subject, messageWithoutPassword, messageWithPassword);
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


        private string BuidHtmlTemplate()
        {
            var subject = "Some subject";
            var address = "Some address";
            var baseUrl = "https://hqrc.mysurvey.solutions";
            var logo = $"{baseUrl}/img/logo.svg";
            var bg = $"{baseUrl}/img/logo.svg";
            var messageHeader = "Message Header";
            var messageBody = "Message Header";
            var passwordText = "Here is your password";
            var password = "password";

            var html =
                @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
                <html xmlns=""http://www.w3.org/1999/xhtml"">
                <head>
                <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
                <style type=""text/css"">
                        ul { list-style: none; padding: 0; margin: 0; }
                        ul li{ margin: 0; padding: 0; display: inline-block; }
                        ul li a:hover{ text-decoration: underline !important; }
                        table { border-spacing:0; }
                        th, td { display: block; }
                        img { border: 0; -ms-interpolation-mode: bicubic; }
                        button { font-size: 100%; margin: 0; vertical-align: baseline; vertical-align: middle; line-height: normal; } 
                        .btn-success:hover{ background: #2c7613 !important; border-color: #2c7613 !important; }
                    </style>";

            if (!string.IsNullOrWhiteSpace(subject))
            {
                html += $"<title>{subject}</title>";
            }

            html +=
                @"</head><body style=""margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; font-size: 14px; box-sizing: border-box;"">";

            html +=
                $@"<table class=""em-table"" align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""max-width: 600px; background-image: url('{bg}'); background-repeat: no-repeat;  background-size: 270px auto;  background-position: 115% 45px;"">
            <tr>
                <td style=""border: 6px solid #E5E5E5;  padding: 50px 55px 115px; box-sizing: border-box;"">
                    <table border=""0"" width=""100%"" cellpadding=""0"" cellspacing=""0"" >
                        <tr><td style=""padding-bottom: 80px;""><img src=""{logo}"" alt=""logo"" style=""display: block; max-height: 170px; width: auto"" class=""em-img""/></td></tr>
                        <tr><td style=""font-size: 24px; line-height: 30px; color: #727272; font-weight: bold; white-space: pre-wrap;"">{messageHeader}</td></tr>
                        <tr><td style=""padding: 40px 0; font-size: 16px; line-height: 20px; white-space: pre-wrap;"">{messageBody}</td></tr>";

            if (!string.IsNullOrWhiteSpace(password))
            {
                html +=
                    $@"<tr><td style=""padding: 0px 0 5px; font-size: 16px; line-height: 20px; white-space: pre-wrap;"">{passwordText}</td></tr>
                    <tr><td style=""padding: 0px 0 50px; font-size: 24px; line-height: 30px; color: #727272; font-weight: bold;"">{password}</td></tr>";
            }

            html+=@"
                        <tr><td><a href=""%SURVEYLINK%"" class=""btn-success"" style=""text-decoration: none; background: #368E19; padding: 10px 12px; text-transform: uppercase; letter-spacing: 0.1em; border-radius: 4px; border: 2px solid #368E19; color: #fff; font-family: 'Trebuchet MS', 'Lucida Sans Unicode', 'Lucida Grande', 'Lucida Sans', Arial, sans-serif; font-size: 14px; box-shadow: none;"">%START%</a></td></tr>
                    </table>
                </td></tr>
            <tr>
                <td width=""100%"" align=""center"">
                    <table  align=""center"" width=""80%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""color: #808080;"">";

            if (string.IsNullOrWhiteSpace(address))
            {
                html += $@"<tr><td align=""center"" style=""padding: 15px 0; white-space: pre-wrap;"">{address}</td></tr>";
            }

            html += @"<tr><td align=""center"" style=""padding-top: 135px;"">Powered by <a href=""https://mysurvey.solutions/"">Survey Solutions</a></td></tr>
                    </table>
                </td>
            </tr>
        </table>";

        html += @"</body></html>";



            return html;
        }
    }
}


