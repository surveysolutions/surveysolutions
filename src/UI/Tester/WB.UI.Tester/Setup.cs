using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac.Extras.MvvmCross;
using MvvmCross.Converters;
using MvvmCross.IoC;
using MvvmCross.Plugin;
using MvvmCross.ViewModels;
using MvvmCross.Views;
using WB.Core.BoundedContexts.Tester;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services;
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
            CrashReporting.Init("e77a488d-d76b-4300-9a37-8716f5b2faa7");
        }

        protected override IMvxIoCProvider CreateIocProvider()
        {
            return CreateAndInitializeIoc();
        }

        protected override void InitializeApp(IMvxPluginManager pluginManager, IMvxApplication app)
        {
            base.InitializeApp(pluginManager, app);

            var status = new UnderConstructionInfo();
            status.Run();

            foreach (var module in modules)
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

            AutofacKernel kernel = new AutofacKernel();

            this.modules = new IModule[]
            {
                new NcqrsModule(),
                new InfrastructureModuleMobile(),
                new DataCollectionSharedKernelModule(),
                new TesterBoundedContextModule(),
                new TesterInfrastructureModule(basePath),
                new EnumeratorUIModule(),
                new TesterUIModule(),
                new EnumeratorSharedKernelModule()
            };
            kernel.Load(this.modules);

            var container = kernel.ContainerBuilder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));

            return new AutofacMvxIocProvider(kernel.Container);
        }

        protected override IMvxViewsContainer InitializeViewLookup(IDictionary<Type, Type> viewModelViewLookup)
        {
            var result = base.InitializeViewLookup(viewModelViewLookup);

            var viewModelViewLookup1 = new Dictionary<Type, Type>()
            {
                {typeof (LoginViewModel), typeof (LoginActivity)},
                {typeof (InterviewViewModel), typeof (InterviewActivity)},
                {typeof (DashboardViewModel), typeof (DashboardActivity)},
                {typeof (CompleteInterviewViewModel), typeof (CompleteInterviewFragment)},
                {typeof (PrefilledQuestionsViewModel), typeof (PrefilledQuestionsActivity)},
                {typeof (PhotoViewViewModel), typeof(PhotoViewActivity) },
#if !EXCLUDEEXTENSIONS
                { typeof (WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel), typeof (WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorActivity)}
#endif
            };

            result.AddAll(viewModelViewLookup1);
            return result;
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);

            registry.AddOrOverwrite("QuestionnaireTypeToBackground", new QuestionnaireTypeToBackgroundConverter());
        }

        public override IEnumerable<Assembly> GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies().Union(new[]
            {
                typeof(TesterBoundedContextModule).Assembly,

#if !EXCLUDEEXTENSIONS
                typeof(WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel).Assembly
#endif
            });
        }
    }
}
