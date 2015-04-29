using System;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using Xamarin;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Log
{
    internal class XamarinInsightsLogger : ILogger
    {
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