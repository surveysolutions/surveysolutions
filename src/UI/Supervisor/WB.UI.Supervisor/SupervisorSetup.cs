using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Widget;
using Autofac;
using HockeyApp.Android;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Converters;
using MvvmCross.IoC;
using MvvmCross.Views;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Supervisor.ServiceLocation;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Converters;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Services.Logging;
using WB.UI.Supervisor.Activities;
using WB.UI.Supervisor.MvvmBindings;
using WB.UI.Supervisor.Services.Implementation;
using MvxIoCProvider = WB.UI.Shared.Enumerator.Autofac.MvxIoCProvider;

namespace WB.UI.Supervisor
{
    public class SupervisorSetup : EnumeratorSetup<SupervisorMvxApplication>
    {
        protected override void InitializeViewLookup()
        {
            base.InitializeViewLookup();

            var viewModelViewLookup = new Dictionary<Type, Type>()
            {
                {typeof(LoginViewModel), typeof(LoginActivity)},
                {typeof(FinishInstallationViewModel), typeof(FinishInstallationActivity)},
                {typeof(DiagnosticsViewModel),typeof(DiagnosticsActivity) },
#if !EXCLUDEEXTENSIONS
                {typeof (Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel), typeof (Shared.Extensions.CustomServices.AreaEditor.AreaEditorActivity)}
#endif
            };

            var container = Mvx.Resolve<IMvxViewsContainer>();
            container.AddAll(viewModelViewLookup);
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);

            registry.AddOrOverwrite("Localization", new EnumeratorLocalizationValueConverter());
            registry.AddOrOverwrite("ValidationStyleBackground", new TextEditValidationStyleBackgroundConverter());
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<ImageView>("CompanyLogo", view => new ImageCompanyLogoBinding(view));

            base.FillTargetFactories(registry);
        }

        protected override IMvxIoCProvider CreateIocProvider()
        {
            return new MvxIoCProvider(this.CreateAndInitializeIoc());
        }

        private IContainer CreateAndInitializeIoc()
        {
            var modules = new IModule[] {
                new NcqrsModule(),
                new InfrastructureModuleMobile(),
                new DataCollectionSharedKernelModule(),
                new EnumeratorUIModule(),
                new EnumeratorSharedKernelModule(),
                new SupervisorInfrastructureModule(),
                new SupervisorUIModule(),
                };

            ContainerBuilder builder = new ContainerBuilder();
            foreach (var module in modules)
            {
                builder.RegisterModule(module.AsAutofac());
            }

            builder.RegisterType<NLogLogger>().As<ILogger>();

            builder.RegisterType<SupervisorSettings>()
                .As<IEnumeratorSettings>()
                .As<IRestServiceSettings>()
                .As<IDeviceSettings>()
                .As<ISupervisorSettings>()
                .WithParameter("backupFolder", AndroidPathUtils.GetPathToSubfolderInExternalDirectory("Backup"))
                .WithParameter("restoreFolder", AndroidPathUtils.GetPathToSubfolderInExternalDirectory("Restore"));

            var container = builder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));

            var serviceLocator = ServiceLocator.Current;
            foreach (var module in modules)
            {
                module.Init(serviceLocator).Wait();
            }

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
                typeof(SupervisorSetup).Assembly
            });
        }
    }
}
