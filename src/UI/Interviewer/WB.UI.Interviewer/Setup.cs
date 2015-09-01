using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Content;
using Cirrious.CrossCore;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.UI.Interviewer.Activities;
using WB.UI.Interviewer.Ninject;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Ninject;

namespace WB.UI.Interviewer
{
    public class Setup : EnumeratorSetup
    {
        public Setup(Context applicationContext) : base(applicationContext){}

        protected override void InitializeViewLookup()
        {
            base.InitializeViewLookup();

            var viewModelViewLookup = new Dictionary<Type, Type>()
            {
                {typeof(LoginActivityViewModel), typeof(LoginActivity)},
                {typeof(FinishIntallationViewModel), typeof(FinishInstallationActivity)},
                {typeof(DashboardViewModel), typeof(DashboardActivity)},
                {typeof(SynchronizationViewModel), typeof(SynchronizationActivity)},
                {typeof(SettingsViewModel), typeof(SettingsActivity)},
                {typeof(InterviewerInterviewViewModel), typeof(InterviewActivity)}
            };

            var container = Mvx.Resolve<IMvxViewsContainer>();
            container.AddAll(viewModelViewLookup);
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override IMvxIoCProvider CreateIocProvider()
        {
            return new NinjectMvxIocProvider(InterviewerApplication.Kernel);
        }

        protected override Assembly[] GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies().Union(new[]
            {
                typeof(AndroidCoreRegistry).Assembly,
                typeof(LoginActivityViewModel).Assembly,

            }).ToArray();
        }
    }
}