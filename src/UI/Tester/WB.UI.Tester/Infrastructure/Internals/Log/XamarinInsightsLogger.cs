using System;
using System.Collections;
using System.Collections.Generic;
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
            Insights.Report(exception: exception, warningLevel: Insights.Severity.Warning, extraData: CollectExtraData());
        }

        public void Error(string message, Exception exception = null)
        {
            Insights.Report(exception: exception, warningLevel: Insights.Severity.Error, extraData: CollectExtraData());
        }

        public void Fatal(string message, Exception exception = null)
        {
            Insights.Report(exception: exception, warningLevel: Insights.Severity.Critical, extraData: CollectExtraData());
        }

        private IDictionary CollectExtraData()
        {
            var settings = Mvx.Resolve<IRestServiceSettings>();
            var principal = Mvx.Resolve<IPrincipal>();

            var extraData = new Dictionary<string, string>
            {
                {"Endpoint", settings.Endpoint},
                {"User",     principal.CurrentUserIdentity?.Name}
            };
            return extraData;
        }
    }
}