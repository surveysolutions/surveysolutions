using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using Xamarin;

namespace WB.UI.Tester.Infrastructure.Internals.Log
{
    internal class XamarinInsightsLogger : ILogger
    {
        public void Error(string message, Exception exception = null)
        {
            Insights.Report(exception: exception, warningLevel: Insights.Severity.Error);
        }

        public void Warn(string message, Exception exception = null)
        {
            Insights.Report(exception: exception, warningLevel: Insights.Severity.Warning);
        }
    }
}