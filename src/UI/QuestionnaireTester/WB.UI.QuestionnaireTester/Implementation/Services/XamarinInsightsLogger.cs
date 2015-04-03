using System;
using WB.Core.GenericSubdomains.Utils.Services;
using Xamarin;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class XamarinInsightsLogger : ILogger
    {
        public void Debug(string message, Exception exception = null)
        {
            this.Warn(message, exception);
        }

        public void Fatal(string message, Exception exception = null)
        {
            this.Error(message, exception);
        }

        public void Info(string message, Exception exception = null)
        {
            this.Warn(message, exception);
        }

        public void Error(string message, Exception exception = null)
        {
            Insights.Report(exception: exception, warningLevel: ReportSeverity.Error);
        }

        public void Warn(string message, Exception exception = null)
        {
            Insights.Report(exception: exception);
        }
    }
}