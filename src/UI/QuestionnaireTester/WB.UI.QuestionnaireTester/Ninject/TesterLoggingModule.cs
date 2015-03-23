using System;
using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils.Services;
using Xamarin;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class TesterLoggingModule : NinjectModule
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

        public override void Load()
        {
            this.Bind<ILogger>().To<XamarinInsightsLogger>().InSingletonScope();
        }
    }
}