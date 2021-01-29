using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Widget;
using Autofac;
using Autofac.Extras.MvvmCross;
using Autofac.Features.ResolveAnything;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Converters;
using MvvmCross.IoC;
using MvvmCross.Platforms.Android.Presenters;
using MvvmCross.Plugin;
using MvvmCross.ViewModels;
using MvvmCross.Views;
using WB.Core.BoundedContexts.Interviewer;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.CreateInterview;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;
using WB.Core.SharedKernels.Enumerator.Views;
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
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Logging;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Interviewer
{
    public class Setup : EnumeratorSetup<InterviewerMvxApplication>
    {
        private IModule[] modules;

        public Setup()
        {
            
         
        }

        protected override IMvxViewsContainer InitializeViewLookup(IDictionary<Type, Type> viewModelViewLookup)
        {
            var lookup = base.InitializeViewLookup(viewModelViewLookup);
            lookup.AddAll(new Dictionary<Type, Type>()
            {
                {typeof(LoginViewModel), typeof(LoginActivity)},
                {typeof(FinishInstallationViewModel), typeof(FinishInstallationActivity)},
                {typeof(DashboardViewModel), typeof(DashboardActivity)},
                {typeof(CreateAndLoadInterviewViewModel),typeof(CreateAndLoadInterviewActivity) },
                {typeof(DiagnosticsViewModel),typeof(DiagnosticsActivity) },
                {typeof(LoadingInterviewViewModel),typeof(LoadingInterviewActivity) },
                {typeof(InterviewViewModel), typeof(InterviewActivity)},
                {typeof(RelinkDeviceViewModel), typeof(RelinkDeviceActivity)},
                {typeof(InterviewerCompleteInterviewViewModel), typeof (CompleteInterviewFragment)},
                {typeof (MapsViewModel), typeof(MapsActivity) },
                {typeof (PhotoViewViewModel), typeof(PhotoViewActivity) },
                {typeof(SearchViewModel), typeof(InterviewerSearchActivity)},
                {typeof(CalendarEventDialogViewModel), typeof(CalendarEventDialog)}
#if !EXCLUDEEXTENSIONS
                ,{typeof (Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel), typeof (Shared.Extensions.CustomServices.AreaEditor.AreaEditorActivity)}
                ,{typeof (Shared.Extensions.CustomServices.MapDashboard.MapDashboardViewModel), typeof (Shared.Extensions.CustomServices.MapDashboard.MapDashboardActivity)}
#endif
            });

            return lookup;
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<TextView>("IsCurrentDashboardTab", (view) => new TextViewIsCurrentDashboardTabBinding(view));
            registry.RegisterCustomBindingFactory<ImageView>("CompanyLogo", view => new ImageCompanyLogoBinding(view));

            base.FillTargetFactories(registry);
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);

            registry.AddOrOverwrite("Localization", new InterviewerLocalizationValueConverter());
        }

        protected override IMvxIoCProvider CreateIocProvider()
        {
            return new AutofacMvxIocProvider(this.CreateAndInitializeIoc());
        }

        protected override void InitializeApp(IMvxPluginManager pluginManager, IMvxApplication app)
        {
            base.InitializeApp(pluginManager, app);

            string appcenterKey = ApplicationContext.Resources.GetString(Resource.String.appcenter_key);
            if (!string.IsNullOrEmpty(appcenterKey))
            {
                CrashReporting.Init(appcenterKey);
            }

            string arcgisruntimeKey = ApplicationContext.Resources.GetString(Resource.String.arcgisruntime_key);
            if (!string.IsNullOrEmpty(arcgisruntimeKey))
            {
                ServiceLocator.Current.GetInstance<IMapInteractionService>().Init(appcenterKey);
            }
            
            var status = new UnderConstructionInfo();
            status.Run();

            foreach (var module in modules.OfType<IInitModule>())
            {
                module.Init(ServiceLocator.Current, status).Wait();
            }

            status.Finish();
        }

        private IContainer CreateAndInitializeIoc()
        {
            this.modules = new IModule[]
            {
                new NcqrsModule(),
                new InfrastructureModuleMobile(),
                new DataCollectionSharedKernelModule(),
                new InterviewerInfrastructureModule(),
                new EnumeratorUIModule(),
                new EnumeratorSharedKernelModule(),
                new InterviewerBoundedContextModule(),
                new InterviewerUIModule(),
            };

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            foreach (var module in modules)
            {
                builder.RegisterModule(module.AsAutofac());
            }
            builder.RegisterModule(new EnumeratorLoggingModule());

            builder.RegisterType<NLogLogger>().As<ILogger>();

            builder.RegisterType<InterviewerSettings>()
                .As<IEnumeratorSettings>()
                .As<IRestServiceSettings>()
                .As<IInterviewerSettings>()
                .As<IDeviceSettings>()
                .WithParameter("backupFolder", AndroidPathUtils.GetPathToSubfolderInExternalDirectory("Backup"))
                .WithParameter("restoreFolder", AndroidPathUtils.GetPathToSubfolderInExternalDirectory("Restore"));

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

        protected override IMvxAndroidViewPresenter CreateViewPresenter()
        {
            return new MvxAndroidViewPresenter(AndroidViewAssemblies);
        }

        public override IEnumerable<Assembly> GetViewModelAssemblies()
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
