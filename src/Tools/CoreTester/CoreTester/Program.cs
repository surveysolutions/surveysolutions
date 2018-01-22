using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using CommandLine;
using Main.Core.Documents;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using NConfig;
using Newtonsoft.Json;
using NLog;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.JsonConversion;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Shared.Enumerator.Services.Internals;


namespace CoreTester
{
    class Program
    {
        [Verb("run", HelpText = "Test core.")]
        protected class CoreTestOptions
        {
            [Option('a', "apppath", Required = true, HelpText = "Path to directory with web.config")]
            public string AppPath { get; set; }
        }

        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CoreTestOptions>(args).MapResult(RunCoreTestOptions, errs => 1);
        }

        private static int RunCoreTestOptions(CoreTestOptions opts)
        {
            IContainer container = CreateKernel(opts);
            CoreTestRunner coreTestRunner = GetCoreTester(container);

            return coreTestRunner.Run();
        }

        private static CoreTestRunner GetCoreTester(IContainer container)
        {
            return container.Resolve<CoreTestRunner>();
        }

        private static IContainer CreateKernel(CoreTestOptions options)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"Application started");

            var pathToWebConfig = Path.Combine(options.AppPath, @"Web.config");
            var pathToConfiguragtion = Path.Combine(options.AppPath, @"Configuration\Headquarters.Web.config");

            NConfigurator.UsingFiles(new[] { pathToConfiguragtion, pathToWebConfig }).SetAsSystemDefault();

            var connectionStringSettingsCollection = NConfigurator.Default.ConnectionStrings;
            var appSettings = NConfigurator.Default.AppSettings;
            
            ContainerBuilder builder = AutofacConfig.CreateKernel();
            IContainer container = builder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));
            return container;
        }
    }

    public class AutofacConfig
    {
        public static ContainerBuilder CreateKernel()
        {
            ContainerBuilder builder = new ContainerBuilder();
            
            builder.RegisterModule(new NcqrsModule().AsAutofac());
            builder.RegisterModule(new NLogLoggingModule().AsAutofac());
            builder.RegisterModule(new EventSourcedInfrastructureModule().AsAutofac());
            builder.RegisterModule(new InfrastructureModuleMobile().AsAutofac());
            builder.RegisterModule(new DataCollectionSharedKernelModule().AsAutofac());
            builder.RegisterModule(new CoreTesterdule().AsAutofac());
            builder.RegisterModule(new PostgresKeyValueModule(new ReadSideCacheSettings(1024, 512)).AsAutofac());

            return builder;
        }
    }

        public class CoreTesterdule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingletonWithConstructorArgument<ILiteEventBus, NcqrCompatibleEventDispatcher>("eventBusSettings", new EventBusSettings
            {
                DisabledEventHandlerTypes = Array.Empty<Type>(),
                EventHandlerTypesWithIgnoredExceptions = Array.Empty<Type>(),
                IgnoredAggregateRoots = new HashSet<string>()
            });
            registry.BindAsSingleton<IWebInterviewNotificationService, WebInterviewNotificationService>();
            registry.BindAsSingleton<IEventBus, InProcessEventBus>();

            registry.BindToMethod<IServiceLocator>(() => ServiceLocator.Current);

            registry.BindAsSingleton<IEventSourcedAggregateRootRepository, IAggregateRootCacheCleaner, EventSourcedAggregateRootRepositoryWithWebCache>();
            
            registry.Bind<IWebInterviewInterviewEntityFactory, WebInterviewInterviewEntityFactory>();

            registry.BindToConstant(() => JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new FilteredCamelCasePropertyNamesContractResolver
                {
                    AssembliesToInclude =
                    {
                        typeof(WebInterviewModule).Assembly,
                        typeof(CategoricalOption).Assembly
                    }
                }
            }));

            registry.RegisterDenormalizer<InterviewLifecycleEventHandler>();

            // TODO: Find a generic place for each of the dependencies below
            registry.Bind<IInterviewExpressionStatePrototypeProvider, InterviewExpressionStatePrototypeProvider>();
            registry.Bind<ITranslationManagementService, TranslationManagementService>();
            registry.Bind<ISubstitutionTextFactory, SubstitutionTextFactory>();
            registry.Bind<ISubstitutionService, SubstitutionService>();
            registry.Bind<IInterviewTreeBuilder, InterviewTreeBuilder>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.Bind<IQuestionOptionsRepository, QuestionnaireQuestionOptionsRepository>();
            registry.BindAsSingleton<IInterviewExpressionStateUpgrader, InterviewExpressionStateUpgrader>();
            registry.Bind<IVariableToUIStringService, VariableToUIStringService>();

            registry.BindAsSingleton<IEventStore, InMemoryEventStore>();
            registry.BindAsSingleton<ISnapshotStore, InMemoryEventStore>();
            registry.BindAsSingleton<IPlainKeyValueStorage<QuestionnaireDocument>, InMemoryKeyValueStorage<QuestionnaireDocument>>();
            registry.BindAsSingleton(typeof(IPlainStorageAccessor<>), typeof(InMemoryPlainStorageAccessor<>));

            registry.BindAsSingleton<IQuestionnaireStorage, QuestionnaireStorage>();

            registry.Bind<CoreTestRunner>();
        }
    }
}
