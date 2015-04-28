using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using Xamarin;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Log
{
    internal class XamarinInsightsLogger : ILogger
    {
        public Task Debug(string message, Exception exception = null)
        {
            return Task.Run(() => this.Warn(message, exception));
        }

        public Task Fatal(string message, Exception exception = null)
        {
            return Task.Run(() => this.Error(message, exception));
        }

        public Task Info(string message, Exception exception = null)
        {
            return Task.Run(() => this.Warn(message, exception));
        }

        public Task Error(string message, Exception exception = null)
        {
            return Task.Run(() => Insights.Report(exception: exception, warningLevel: ReportSeverity.Error));
        }

        public Task Warn(string message, Exception exception = null)
        {
            return Task.Run(() => Insights.Report(exception: exception));
        }
    }
}