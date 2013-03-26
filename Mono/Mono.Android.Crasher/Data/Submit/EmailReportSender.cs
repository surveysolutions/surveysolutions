using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Android.App;
using Mono.Android.Crasher.Attributes;

namespace Mono.Android.Crasher.Data.Submit
{
    [Obsolete("Not working because of some MonoDroid problems with SSL certs")]
    public class EmailReportSender : IReportSender
    {
        private EmailReporterSettingsAttribute _config;

        public void Initialize(Application application)
        {
            _config = application.GetType().GetCustomAttributes(typeof(EmailReporterSettingsAttribute), false).SingleOrDefault() as EmailReporterSettingsAttribute;
            if (_config == null) throw new InvalidOperationException("Application class need to be marked with EmailReporterAttribute");
        }

        public void Send(ReportData errorContent)
        {
            if (_config == null)
                throw new InvalidOperationException("EmailReportSender need to be initialized first");
            try
            {
                var smtpClient = new SmtpClient(_config.Host, _config.Port)
                                     {
                                         EnableSsl = _config.UseSSL,
                                         Credentials = new NetworkCredential(_config.Login, _config.Password)
                                     };
                var message = new MailMessage
                                  {
                                      From = new MailAddress(_config.From)
                                  };

                message.To.Add(_config.To);
                message.Subject = _config.Subject;
                message.Body = errorContent.ToString();
                message.IsBodyHtml = false;
                smtpClient.Send(message);
            }
            catch (Exception e)
            {
                throw new ReportSenderException("Error sending email report", e);
            }
        }
    }
}