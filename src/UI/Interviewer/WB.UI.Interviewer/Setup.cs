using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Widget;
using Autofac;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Platform;
using MvvmCross.Platform.Converters;
using MvvmCross.Platform.IoC;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Interviewer.Activities;
using WB.UI.Interviewer.Activities.Dashboard;
using WB.UI.Interviewer.Converters;
using WB.UI.Interviewer.CustomBindings;
using WB.UI.Interviewer.Infrastructure;
using WB.UI.Interviewer.ServiceLocation;
using WB.UI.Interviewer.Settings;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Autofac;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using Xamarin;

namespace WB.UI.Interviewer
{
    public class Setup : EnumeratorSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
            InitializeLogger(applicationContext);
        }

        private void InitializeLogger(Context applicationContext)
        {
            Insights.HasPendingCrashReport += (sender, isStartupCrash) =>
            {
                if (isStartupCrash)
                {
                    Insights.PurgePendingCrashReports().WaitAndUnwrapException();
                }
            };

#if DEBUG
            Insights.Initialize(Insights.DebugModeKey, applicationContext);
#else
            Insights.Initialize("20fe6ac44d54f5fed5370bc877d7ba7524f15363", applicationContext);
#endif
        }

        protected override void ProcessException(Exception exception)
        {
            base.ProcessException(exception);

            var serviceSettings = Mvx.Resolve<IRestServiceSettings>();
            exception.Data["Endpoint"] = serviceSettings.Endpoint;

            var principal = Mvx.Resolve<IPrincipal>();
            exception.Data["User"] = principal.CurrentUserIdentity?.Name;
        }

        protected override void InitializeViewLookup()
        {
            base.InitializeViewLookup();

            var viewModelViewLookup = new Dictionary<Type, Type>()
            {
                {typeof(LoginViewModel), typeof(LoginActivity)},
                {typeof(FinishInstallationViewModel), typeof(FinishInstallationActivity)},
                {typeof(DashboardViewModel), typeof(DashboardActivity)},
                {typeof(DiagnosticsViewModel),typeof(DiagnosticsActivity) },
                {typeof(LoadingViewModel),typeof(LoadingActivity) },
                {typeof(InterviewViewModel), typeof(InterviewActivity)},
                {typeof(RelinkDeviceViewModel), typeof(RelinkDeviceActivity)},
                {typeof(InterviewerCompleteInterviewViewModel), typeof (CompleteInterviewFragment)},
                {typeof (PrefilledQuestionsViewModel), typeof (PrefilledQuestionsActivity)},
#if !EXCLUDEEXTENSIONS
                {typeof (Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel), typeof (Shared.Extensions.CustomServices.AreaEditor.AreaEditorActivity)}
#endif
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
            registry.AddOrOverwrite("StatusToDasboardBackground", new StatusToDasboardBackgroundConverter());
            registry.AddOrOverwrite("InterviewStatusToColor", new InterviewStatusToColorConverter());
            registry.AddOrOverwrite("SynchronizationStatusToDrawable", new SynchronizationStatusToDrawableConverter());
            registry.AddOrOverwrite("ValidationStyleBackground", new TextEditValidationStyleBackgroundConverter());
            registry.AddOrOverwrite("IsSynchronizationFailOrCanceled", new IsSynchronizationFailOrCanceledConverter());
            registry.AddOrOverwrite("SynchronizationStatusToTextColor", new SynchronizationStatusToTextColorConverter());
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<TextView>("IsCurrentDashboardTab", (view) => new TextViewIsCurrentDashboardTabBinding(view));
            registry.RegisterCustomBindingFactory<ImageView>("CompanyLogo", view => new ImageCompanyLogoBinding(view));
            registry.RegisterCustomBindingFactory<RecyclerView>("ScrollToPosition", view => new RecyclerViewScrollToPositionBinding(view));

            base.FillTargetFactories(registry);
        }
        
        protected override IMvxIoCProvider CreateIocProvider()
        {
            return new MvxIoCProvider(this.CreateAndInitializeIoc());
        }

        private IContainer CreateAndInitializeIoc()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new NcqrsModule().AsAutofac());
            builder.RegisterModule(new InfrastructureModuleMobile().AsAutofac());
            builder.RegisterModule(new DataCollectionSharedKernelModule().AsAutofac());
            builder.RegisterModule(new InterviewerInfrastructureModule().AsAutofac());
            builder.RegisterModule(new EnumeratorUIModule().AsAutofac());
            builder.RegisterModule(new EnumeratorSharedKernelModule().AsAutofac());
            builder.RegisterModule(new InterviewerUIModule().AsAutofac());
            builder.RegisterModule(new InterviewerLoggingModule());


            builder.RegisterType<InterviewerSettings>().As<IEnumeratorSettings>().As<IRestServiceSettings>().As<IInterviewerSettings>()
                .WithParameter("backupFolder", AndroidPathUtils.GetPathToSubfolderInExternalDirectory("Backup"))
                .WithParameter("restoreFolder", AndroidPathUtils.GetPathToSubfolderInExternalDirectory("Restore"));

            builder.RegisterType<InterviewerDashboardEventHandler>().SingleInstance();

            var container = builder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));

            return container;
        }

        protected override IEnumerable<Assembly> AndroidViewAssemblies
        {
            get
            {
                var toReturn = base.AndroidViewAssemblies;

                // Add assemblies with other views we use.  When the XML is inflated
                // MvvmCross knows about the types and won't complain about them.  This
                // speeds up inflation noticeably.
                return toReturn;
            }
        }

        protected override IEnumerable<Assembly> GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies().Union(new[]
            {
                typeof(Setup).Assembly,
                typeof(LoginViewModel).Assembly,
#if !EXCLUDEEXTENSIONS
                typeof(Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel).Assembly
#endif
            });
        }
    }
}