using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Moq;
using MvvmCross.Core;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.PlainStorage;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Files;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Ioc;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.Implementation.Migrations;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLTests;
using WB.UI.Headquarters.Injections;
using WB.UI.Headquarters.Migrations.PlainStore;
using WB.UI.Headquarters.Migrations.ReadSide;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Versions;

namespace WB.Tests.Integration.FullStackIntegration.Hq
{
    [TestFixture]
    public class QuestionnaireBuilder : with_postgres_db
    {
        private string tempFolder;
        private string questionnaireContentFolder;
        private string exportFolder;

        [OneTimeSetUp]
        public void Setup()
        {
            tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            exportFolder = Path.Combine(tempFolder, "Export");

            Directory.CreateDirectory(tempFolder);
            var testAssembly = Assembly.GetAssembly(typeof(QuestionnaireBuilder));
            var zipPath = Path.Combine(tempFolder, "quuestionnaire.zip");

            using (var stream = testAssembly.GetManifestResourceStream("WB.Tests.Integration.FullStackIntegration.Hq.Enumerator Questions Automation.zip"))
            {
                using (var outputStream = File.Create(zipPath))
                {
                    stream.CopyTo(outputStream);
                }
            }

            ZipArchiveUtils zipUtil = new ZipArchiveUtils();
            zipUtil.Unzip(zipPath, tempFolder, true);

            questionnaireContentFolder = Path.Combine(tempFolder,
                "Enumerator Questions Automation (6158dd074d64498f8a50e5e9828fda23)");

            var kernel = new NinjectKernel();

            var pgReadSideModule = new PostgresReadSideModule(
                connectionStringBuilder.ConnectionString,
                PostgresReadSideModule.ReadSideSchemaName,
                DbUpgradeSettings.FromFirstMigration<M001_InitDb>(),
                new ReadSideCacheSettings(50, 30),
                new List<Assembly> {typeof(HeadquartersBoundedContextModule).Assembly});

            var plainStorageModule = new PostgresPlainStorageModule(new PostgresPlainStorageSettings()
            {
                ConnectionString = connectionStringBuilder.ConnectionString,
                SchemaName = "plainstore",
                DbUpgradeSettings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace),
                MappingAssemblies = new List<Assembly>
                {
                    typeof(HeadquartersBoundedContextModule).Assembly,
                    typeof(ProductVersionModule).Assembly,
                }
            });
            
            var hqBoundedContext = new HeadquartersBoundedContextModule(tempFolder, new SyncPackagesProcessorBackgroundJobSetting(false, 0, 0, 1), 
                Create.Entity.UserPreloadingSettings(), 
                new ExportSettings(1), 
                new InterviewDataExportSettings(exportFolder, false), 
                new SampleImportSettings(1),
                new SyncSettings(), 
                new TrackingSettings(TimeSpan.Zero));

            var eventStoreModule = new PostgresWriteSideModule(new PostgreConnectionSettings
                {
                    ConnectionString = connectionStringBuilder.ConnectionString,
                    SchemaName = "events"
                }, 
                new DbUpgradeSettings(typeof(M001_AddEventSequenceIndex).Assembly, typeof(M001_AddEventSequenceIndex).Namespace));

            kernel.Load(
                new NLogLoggingModule(),
                new InfrastructureModule(),
                new FileInfrastructureModule(),
                new EventSourcedInfrastructureModule(),
                new NcqrsModule(),
                new ProductVersionModule(typeof(HeadquartersUIModule).Assembly),
                new TestModule(),
                new QuartzModule(),
                eventStoreModule,
                plainStorageModule,
                new PostgresKeyValueModule(new ReadSideCacheSettings(50, 30)),
                pgReadSideModule,
                hqBoundedContext);

            ServiceLocator.SetLocatorProvider(() => new NativeNinjectServiceLocatorAdapter(kernel.Kernel));
            kernel.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);

            kernel.InitModules(new UnderConstructionInfo()).Wait();

            ServiceLocator.SetLocatorProvider(() => new NativeNinjectServiceLocatorAdapter(kernel.Kernel)); // TODO reset to previous
        }

        class TestModule : IModule
        {
            public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
            {
                return Task.CompletedTask;
            }

            public void Load(IIocRegistry registry)
            {
                registry.BindAsSingleton<IEventSourcedAggregateRootRepository, IAggregateRootCacheCleaner, EventSourcedAggregateRootRepositoryWithExtendedCache>();
                registry.BindToMethod<IWebInterviewNotificationService>(() => Mock.Of<IWebInterviewNotificationService>());


                var eventBusConfigSection = new EventBusConfigSection();
                registry.BindAsSingletonWithConstructorArgument<ILiteEventBus, NcqrCompatibleEventDispatcher>(
                    "eventBusSettings",
                     eventBusConfigSection.GetSettings());
            }
        }

        [Test]
        public async Task QuestionnairePackageComposer()
        {
            var restService = Mock.Of<IRestService>();
            var authorizedUser = Mock.Of<IAuthorizedUser>();

            var importService = new QuestionnaireImportService(
                ServiceLocator.Current.GetInstance<ISupportedVersionProvider>(),
                restService,
                ServiceLocator.Current.GetInstance<IStringCompressor>(),
                ServiceLocator.Current.GetInstance<IAttachmentContentService>(),
                ServiceLocator.Current.GetInstance<IQuestionnaireVersionProvider>(),
                ServiceLocator.Current.GetInstance<ITranslationManagementService>(),
                ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireLookupTable>>(),
                ServiceLocator.Current.GetInstance<ICommandService>(),
                Mock.Of<ILogger>(),
                ServiceLocator.Current.GetInstance<IAuditLog>(),
                authorizedUser,
                new DesignerUserCredentials()
            );

            var questionnaireId = Guid.Parse("900c45084d9d4c63a54c8ae5f60d07e3");
            await importService.Import(questionnaireId, "name", false);



            //var lookupTables = new InMemoryPlainStorageAccessor<LookupTableContent>();
            //lookupTables.

            //var lookupTableService = new LookupTableService(lookupTables, new InMemoryPlainStorageAccessor<QuestionnaireDocument>());


            //QuestionnaireExpressionProcessorGenerator expressionProcessorGenerator =
            //    new QuestionnaireExpressionProcessorGenerator(new RoslynCompiler(), IntegrationCreate.CodeGenerator(),
            //        new CodeGeneratorV2(new CodeGenerationModelsFactory(new MacrosSubstitutionService(), lookupTableService, new QuestionTypeToCSharpTypeMapper())),
            //        new DynamicCompilerSettingsProvider(compilerSettings, fileSystemAccessor));

            //NewtonJsonSerializer serializer = new NewtonJsonSerializer();
            //var version = IntegrationCreate.DesignerEngineVersionService().LatestSupportedVersion;

            //GenerationResult generationResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(
            //    questionnaire,
            //    version,
            //    out resultAssembly);
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
        }
    }
}
