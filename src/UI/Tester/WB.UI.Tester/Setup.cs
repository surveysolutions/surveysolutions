using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Features.ResolveAnything;
using MvvmCross.Converters;
using MvvmCross.IoC;
using MvvmCross.Plugin;
using MvvmCross.ViewModels;
using MvvmCross.Views;
using WB.Core.BoundedContexts.Tester;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Autofac.MvvmCross;
using WB.UI.Shared.Enumerator.Services.Logging;
using WB.UI.Shared.Enumerator.Utils;
using WB.UI.Tester.Activities;
using WB.UI.Tester.Converters;
using WB.UI.Tester.Infrastructure;
using WB.UI.Tester.ServiceLocation;

namespace WB.UI.Tester
{
    public class Setup : EnumeratorSetup<TesterMvxApplication>
    {
        public Setup()
        {
        }

        protected override IMvxIoCProvider CreateIocProvider()
        {
            return CreateAndInitializeIoc();
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
                ServiceLocator.Current.GetInstance<IMapInteractionService>().Init(arcgisruntimeKey);
            }

            var status = new UnderConstructionInfo();
            status.Run();

            foreach (var module in modules.OfType<IInitModule>())
            {
                module.Init(ServiceLocator.Current, status).Wait();
            }

            status.Finish();
        }

        private IModule[] modules;

        private IMvxIoCProvider CreateAndInitializeIoc()
        {
            var basePath = Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal))
               ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
               : AndroidPathUtils.GetPathToExternalDirectory();

            this.modules = new IModule[]
            {
                new NcqrsModule(),
                new InfrastructureModuleMobile(),
                new DataCollectionSharedKernelModule(),
                new TesterBoundedContextModule(),
                new TesterInfrastructureModule(basePath),
                new EnumeratorIocRegistrationModule(),
                new EnumeratorUIModule(),
                new EnumeratorSharedKernelModule(),
                new TesterUIModule(),
            };
            
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            foreach (var module in modules)
            {
                builder.RegisterModule(module.AsAutofac());
            }
            builder.RegisterModule(new EnumeratorLoggingModule());

            builder.RegisterType<NLogLogger>().As<ILogger>();

            var container = builder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));

            return new MvxIoCProviderWithParent(CreateIocOptions(),container);
        }

        protected override IMvxViewsContainer InitializeViewLookup(IDictionary<Type, Type> viewModelViewLookup, IMvxIoCProvider iocProvider)
        {
            var lookup = base.InitializeViewLookup(viewModelViewLookup, iocProvider);

            var viewModelViewLookup1 = new Dictionary<Type, Type>()
            {
                {typeof (LoginViewModel), typeof (LoginActivity)},
                {typeof (InterviewViewModel), typeof (InterviewActivity)},
                {typeof (DashboardViewModel), typeof (DashboardActivity)},
                {typeof (CompleteInterviewViewModel), typeof (CompleteInterviewFragment)},
                {typeof (PhotoViewViewModel), typeof(PhotoViewActivity) },
#if !EXCLUDEEXTENSIONS
                { typeof (WB.UI.Shared.Extensions.ViewModels.GeographyEditorViewModel), typeof (WB.UI.Shared.Extensions.Activities.GeographyEditorActivity)}
#endif
            };

            lookup.AddAll(viewModelViewLookup1);
            return lookup;
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);

            registry.AddOrOverwrite("QuestionnaireTypeToBackground", new QuestionnaireTypeToBackgroundConverter());
            registry.AddOrOverwrite("Localization", new TesterLocalizationValueConverter());
        }

        public override IEnumerable<Assembly> GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies().Union(new[]
            {
                typeof(TesterBoundedContextModule).Assembly,

#if !EXCLUDEEXTENSIONS
                typeof(WB.UI.Shared.Extensions.ViewModels.GeographyEditorViewModel).Assembly
#endif
            });
        }
    }
}
