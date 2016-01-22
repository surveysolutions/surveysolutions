using System;
using Android.Runtime;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.UI.Interviewer
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                Mvx.Resolve<ITabletDiagnosticService>().RestartTheApp();
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Mvx.Resolve<ITabletDiagnosticService>().RestartTheApp();
            };

            base.Initialize();
        }
    }
}