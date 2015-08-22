using System;

namespace WB.UI.Interviewer.Infrastructure.Internals.Crasher.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EmailReporterSettingsAttribute : Attribute
    {
        public EmailReporterSettingsAttribute(string to, string @from, string subject, string host, int port, string login, string password, bool useSSL = false)
        {
            this.To = to;
            this.From = @from;
            this.Subject = subject;
            this.Host = host;
            this.Port = port;
            this.Login = login;
            this.Password = password;
            this.UseSSL = useSSL;
        }

        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}