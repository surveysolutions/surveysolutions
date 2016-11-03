using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using MvvmCross.Platform;
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
            ReportException(exception: exception, warningLevel: Insights.Severity.Warning);
        }

        public void Error(string message, Exception exception = null)
        {
            ReportException(exception: exception, warningLevel: Insights.Severity.Error);
        }

        public void Fatal(string message, Exception exception = null)
        {
            ReportException(exception: exception, warningLevel: Insights.Severity.Critical);
        }

        private void ReportException(Exception exception, Insights.Severity warningLevel)
        {
            var settings = ServiceLocator.Current.GetInstance<IRestServiceSettings>();
            var principal = ServiceLocator.Current.GetInstance<IPrincipal>();

            var extraData = new Dictionary<string, string>
            {
                {"Endpoint", settings.Endpoint},
                {"User",     principal.CurrentUserIdentity?.Name}
            };

            Insights.Report(exception: exception, warningLevel: warningLevel, extraData: extraData);
        }
    }
}