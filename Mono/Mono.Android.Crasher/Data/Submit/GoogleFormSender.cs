using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Android.App;
using Android.Util;
using Mono.Android.Crasher.Attributes;
using System.Web;

namespace Mono.Android.Crasher.Data.Submit
{
    /// <summary>
    /// Submits report data to shared Google form
    /// </summary>
    public class GoogleFormSender : IReportSender
    {
        private Uri _formUrl;
        private GoogleFormReporterSettingsAttribute _config;

        public void Initialize(Application application)
        {
            _config =
                application.GetType().GetCustomAttributes(typeof(GoogleFormReporterSettingsAttribute), false).SingleOrDefault()
                as GoogleFormReporterSettingsAttribute;
            if (_config == null)
                throw new ArgumentException("Application class need to be marked with GoogleFormReporterAttribute");
            if (string.IsNullOrEmpty(_config.FormKey))
                throw new ArgumentException("FromKey can not be null or empty");
            _formUrl = new Uri("https://docs.google.com/spreadsheet/formResponse?formkey=" + _config.FormKey + "&ifq");
        }

        public void Send(ReportData errorContent)
        {
            var formParams = Remap(errorContent);
            formParams.Add("pageNumber", "0");
            formParams.Add("backupCache", "");
            formParams.Add("submit", "Submit");
            try
            {
                Log.Debug(Constants.LOG_TAG, "Sending report # " + errorContent[ReportField.ReportID]);
                Log.Debug(Constants.LOG_TAG, "Connecting to " + _formUrl.Host);
                var request = (HttpWebRequest)WebRequest.Create(_formUrl);
                request.Timeout = _config.ConnectionTimeout;
                request.Method = "POST";
                request.AllowAutoRedirect = true;
                request.ReadWriteTimeout = 10000;
                request.ContentType = "application/x-www-form-urlencoded";
                var parameters = string.Join("&", ToFormData(formParams).ToArray());

                using (var requestStream = request.GetRequestStream())
                {
                    Log.Debug(Constants.LOG_TAG, "Start sending data to " + _formUrl);
                    var bytes = Encoding.UTF8.GetBytes(parameters);
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Flush();
                    requestStream.Close();

                    using (request.GetResponse())
                    {
                    }
                }
            }
            catch (Exception e)
            {
                throw new ReportSenderException("Error while sending report to Google Form.", e);
            }
        }

        private static List<string> ToFormData(IEnumerable<KeyValuePair<string, string>> data)
        {
            var result = new List<string>();
            result.AddRange(data.Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value))));
            return result;
        }

        private static IDictionary<string, string> Remap(IEnumerable<KeyValuePair<string, string>> report)
        {
            var inputId = 0;
            return report.ToDictionary(originalKey => "entry." + inputId++ + ".single", originalKey => originalKey.Value);
        }
    }
}