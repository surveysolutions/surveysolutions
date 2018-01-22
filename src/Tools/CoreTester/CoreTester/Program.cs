using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using CommandLine;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using NConfig;
using Newtonsoft.Json;
using Ninject;
using Ninject.Modules;
using NLog;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.JsonConversion;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Infrastructure.Native.Ioc;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;


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
            IKernel container = CreateKernel(opts);
            CoreTestRunner coreTestRunner = GetCoreTester(container);

            return coreTestRunner.Run();
        }

        private static CoreTestRunner GetCoreTester(IKernel kernel)
        {
            return kernel.Get<CoreTestRunner>();
        }

        private static IKernel CreateKernel(CoreTestOptions options)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"Application started");

            var pathToWebConfig = Path.Combine(options.AppPath, @"Web.config");
            var pathToConfiguragtion = Path.Combine(options.AppPath, @"Configuration\Headquarters.Web.config");

            NConfigurator.UsingFiles(new[] { pathToConfiguragtion, pathToWebConfig }).SetAsSystemDefault();

            var connectionStringSettingsCollection = NConfigurator.Default.ConnectionStrings;

            IKernel kernel = NinjectConfig.CreateKernel(connectionStringSettingsCollection);
           
            return kernel;
        }
    }

    public class NinjectConfig
    {
        public static IKernel CreateKernel(ConnectionStringSettingsCollection connectionStringSettingsCollection)
        {
            var dbConnectionStringName = @"Postgres";
            var cacheSettings = new ReadSideCacheSettings(1024, 512);
            var mappingAssemblies = new List<Assembly> { typeof(HeadquartersBoundedContextModule).Assembly };

            var eventStoreSettings = new PostgreConnectionSettings
            {
                ConnectionString = connectionStringSettingsCollection[dbConnectionStringName].ConnectionString,
                SchemaName = "events"
            };

            var postgresPlainStorageSettings = new PostgresPlainStorageSettings()
            {
                ConnectionString = connectionStringSettingsCollection[dbConnectionStringName].ConnectionString,
                SchemaName = "plainstore",
                DbUpgradeSettings = new DbUpgradeSettings(typeof(CoreTestRunner).Assembly, typeof(CoreTestRunner).Namespace),
                MappingAssemblies = new List<Assembly>
                {
                    typeof(HeadquartersBoundedContextModule).Assembly
                }
            };

            var kernel = new StandardKernel(
                new NinjectSettings {InjectNonPublic = true},
                new NcqrsModule().AsNinject(),
                new NLogLoggingModule().AsNinject(),
                new ServiceLocationModule(),
                new EventSourcedInfrastructureModule().AsNinject(),
                new InfrastructureModule().AsNinject(),
                new DataCollectionSharedKernelModule().AsNinject(),
                new PostgresKeyValueModule(cacheSettings).AsNinject(),
                new PostgresReadSideModule(
                    connectionStringSettingsCollection[dbConnectionStringName].ConnectionString,
                    PostgresReadSideModule.ReadSideSchemaName,
                    new DbUpgradeSettings(typeof(CoreTestRunner).Assembly, typeof(CoreTestRunner).Namespace),
                    cacheSettings,
                    mappingAssemblies).AsNinject(),
                new PostgresPlainStorageModule(postgresPlainStorageSettings).AsNinject(),
                new CoreTesterdule(eventStoreSettings).AsNinject());
            return kernel;
        }
    }

    internal class ServiceLocationModule : NinjectModule
    {
        public override void Load()
        {
            ServiceLocator.SetLocatorProvider(() => new NativeNinjectServiceLocatorAdapter(this.Kernel));

            this.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);
        }
    }

    public class CoreTesterdule : IModule
    {
        private readonly PostgreConnectionSettings eventStoreSettings;

        public CoreTesterdule(PostgreConnectionSettings eventStoreSettings)
        {
            this.eventStoreSettings = eventStoreSettings;
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindToConstant<IEventTypeResolver>(() =>
                new EventTypeResolver(
                    typeof(DataCollectionSharedKernelAssemblyMarker).Assembly,
                    typeof(HeadquartersBoundedContextModule).Assembly));

            registry.BindAsSingletonWithConstructorArgument<ILiteEventBus, NcqrCompatibleEventDispatcher>("eventBusSettings", new EventBusSettings
            {
                DisabledEventHandlerTypes = Array.Empty<Type>(),
                EventHandlerTypesWithIgnoredExceptions = Array.Empty<Type>(),
                IgnoredAggregateRoots = new HashSet<string>()
            });
            registry.BindAsSingleton<IWebInterviewNotificationService, WebInterviewNotificationService>();
            registry.BindAsSingleton<IEventBus, InProcessEventBus>();

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

            registry.BindAsSingletonWithConstructorArgument<IStreamableEventStore, PostgresEventStore>("connectionSettings", this.eventStoreSettings);
            registry.BindToMethod<IEventStore>(context => context.Get<IStreamableEventStore>());

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
            

            registry.BindToMethod<IInterviewAnswerSerializer>(() => new NewtonInterviewAnswerJsonSerializer());

            registry.Bind<IQuestionnaireBrowseViewFactory, QuestionnaireBrowseViewFactory>();

            registry.BindAsSingleton<IQuestionnaireStorage, UpdatedQuestionnaireStorage>();
            registry.Bind<CoreTestRunner>();
        }
    }

    public static class ModuleExtensions
    {
        public static NinjectModule AsNinject<TModule>(this TModule module)
            where TModule : IModule
        {
            return new NinjectModuleAdapter<TModule>(module);
        }
    }
}
