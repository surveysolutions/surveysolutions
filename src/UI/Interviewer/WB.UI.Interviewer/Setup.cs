using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Content;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Converters;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.UI.Interviewer.Activities;
using WB.UI.Interviewer.Converters;
using WB.UI.Interviewer.CustomBindings;
using WB.UI.Interviewer.Ninject;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.CustomBindings;
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
                {typeof(LoginViewModel), typeof(LoginActivity)},
                {typeof(FinishIntallationViewModel), typeof(FinishInstallationActivity)},
                {typeof(DashboardViewModel), typeof(DashboardActivity)},
                {typeof(SettingsViewModel), typeof(SettingsActivity)},
                {typeof(InterviewerInterviewViewModel), typeof(InterviewActivity)},
                {typeof(RelinkDeviceViewModel), typeof(RelinkDeviceActivity)}
            };

            var container = Mvx.Resolve<IMvxViewsContainer>();
            container.AddAll(viewModelViewLookup);
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);

            registry.AddOrOverwrite("Localization", new InterviewerLocalizationValueConverter());
            registry.AddOrOverwrite("InterviewStatusToColor", new InterviewStatusToColorConverter());
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<TextView>("IsCurrentDashboardTab", (view) => new TextViewIsCurrentDashboardTabBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("TextColor", (view) => new TextViewTextColorBinding(view));

            base.FillTargetFactories(registry);
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
                typeof(LoginViewModel).Assembly,

            }).ToArray();
        }
    }
}