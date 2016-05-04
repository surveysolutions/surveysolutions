using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable.Services;
using Xamarin;

namespace WB.UI.Tester.Infrastructure.Internals.Log
{
    internal class XamarinInsightsLogger : ILogger
    {
        public void Debug(string message, Exception exception = null) {}

        public void Info(string message, Exception exception = null) {}

        public void Warn(string message, Exception exception = null)
        {
            Insights.Report(exception: exception, warningLevel: Insights.Severity.Warning);
        }

        public void Error(string message, Exception exception = null)
        {
            Insights.Report(exception: exception, warningLevel: Insights.Severity.Error);
        }

        public void Fatal(string message, Exception exception = null)
        {
            Insights.Report(exception: exception, warningLevel: Insights.Severity.Critical);
        }
    }
}