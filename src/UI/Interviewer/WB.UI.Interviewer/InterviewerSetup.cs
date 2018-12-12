using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Widget;
using Autofac;
using Autofac.Features.ResolveAnything;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Converters;
using MvvmCross.IoC;
using MvvmCross.Platforms.Android.Presenters;
using MvvmCross.Views;
using WB.Core.BoundedContexts.Interviewer;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Services;
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
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Services.Logging;

namespace WB.UI.Interviewer
{
    public class InterviewerSetup : EnumeratorSetup<InterviewerMvxApplication>
    {
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
                {typeof (MapsViewModel), typeof(MapsActivity) },
                {typeof (PhotoViewViewModel), typeof(PhotoViewActivity) },
                {typeof(SearchViewModel), typeof(InterviewerSearchActivity)}
#if !EXCLUDEEXTENSIONS
                ,{typeof (Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel), typeof (Shared.Extensions.CustomServices.AreaEditor.AreaEditorActivity)}
#endif
            };

            var container = Mvx.Resolve<IMvxViewsContainer>();
            container.AddAll(viewModelViewLookup);
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
            return new Shared.Enumerator.Autofac.MvxIoCProvider(this.CreateAndInitializeIoc());
        }

        private IContainer CreateAndInitializeIoc()
        {
            var modules = new IModule[] {
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
            builder.RegisterModule(new InterviewerLoggingModule());

            builder.RegisterType<NLogLogger>().As<ILogger>();

            builder.RegisterType<InterviewerSettings>()
                .As<IEnumeratorSettings>()
                .As<IRestServiceSettings>()
                .As<IInterviewerSettings>()
                .As<IDeviceSettings>()
                .WithParameter("backupFolder", AndroidPathUtils.GetPathToSubfolderInExternalDirectory("Backup"))
                .WithParameter("restoreFolder", AndroidPathUtils.GetPathToSubfolderInExternalDirectory("Restore"));

            builder.RegisterType<InterviewDashboardEventHandler>().SingleInstance();

            var container = builder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));

            var serviceLocator = ServiceLocator.Current;

            var status = new UnderConstructionInfo();
            status.Run();

            foreach (var module in modules)
            {
                module.Init(serviceLocator, status).Wait();
            }
            status.Finish();

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

        public override IEnumerable<Assembly> GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies().Union(new[]
            {
                typeof(InterviewerSetup).Assembly,
                typeof(LoginViewModel).Assembly,
#if !EXCLUDEEXTENSIONS
                typeof(Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel).Assembly
#endif
            });
        }
    }
}
