using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Files;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Ioc;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation.Migrations;
using WB.Infrastructure.Native.Threading;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLTests;
using WB.UI.Headquarters.Injections;
using WB.UI.Headquarters.Migrations.PlainStore;
using WB.UI.Headquarters.Migrations.ReadSide;
using WB.UI.Headquarters.Migrations.Users;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
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
        private Assembly testAssembly;
        private readonly Guid questionanireId = Guid.Parse("6158dd074d64498f8a50e5e9828fda23");

        [OneTimeSetUp]
        public void Setup()
        {
            tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            exportFolder = Path.Combine(tempFolder, "Export");

            Directory.CreateDirectory(tempFolder);
            testAssembly = Assembly.GetAssembly(typeof(QuestionnaireBuilder));
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
                ConnectionStringBuilder.ConnectionString,
                PostgresReadSideModule.ReadSideSchemaName,
                DbUpgradeSettings.FromFirstMigration<M001_InitDb>(),
                new ReadSideCacheSettings(50, 30),
                new List<Assembly> {typeof(HeadquartersBoundedContextModule).Assembly});

            var plainStorageModule = new PostgresPlainStorageModule(new PostgresPlainStorageSettings()
            {
                ConnectionString = ConnectionStringBuilder.ConnectionString,
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
                    ConnectionString = ConnectionStringBuilder.ConnectionString,
                    SchemaName = "events"
                }, 
                new DbUpgradeSettings(typeof(M001_AddEventSequenceIndex).Assembly, typeof(M001_AddEventSequenceIndex).Namespace));
            
            Database.SetInitializer(new FluentMigratorInitializer<HQIdentityDbContext>("users", DbUpgradeSettings.FromFirstMigration<M001_AddUsersHqIdentityModel>()));
                
            kernel.Load(
                new NLogLoggingModule(),
                new InfrastructureModule(),
                new FileInfrastructureModule(),
                new EventSourcedInfrastructureModule(),
                new NcqrsModule(),
                new ProductVersionModule(typeof(HeadquartersUIModule).Assembly),
                new TestModule(),
                eventStoreModule,
                plainStorageModule,
                new PostgresKeyValueModule(new ReadSideCacheSettings(50, 30)),
                pgReadSideModule,
                hqBoundedContext,
                new QuartzModule(),
                new FileStorageModule(tempFolder),
                new OwinSecurityModule());

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
                registry.BindToMethod(() => Mock.Of<IWebInterviewNotificationService>());

                var eventBusConfigSection = new EventBusConfigSection();
                registry.BindAsSingletonWithConstructorArgument<ILiteEventBus, NcqrCompatibleEventDispatcher>(
                    "eventBusSettings",
                     eventBusConfigSection.GetSettings());

                registry.BindToConstant<IPasswordPolicy>(() => new PasswordPolicy
                {
                    PasswordStrengthRegularExpression = ".*"
                });
            }
        }

        [Test]
        public async Task QuestionnairePackageComposer()
        {
            await ImportQuestionnaire();
            await CreateUsers();
            await RunSynchronization();
            RunExport();

        }

        private void RunExport()
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionanireId, 1);

            var tabularFormatDataExportHandler = ServiceLocator.Current.GetInstance<TabularFormatDataExportHandler>();

            var plainTransaction = ServiceLocator.Current.GetInstance<IPlainTransactionManager>();
            plainTransaction.ExecuteInPlainTransaction(() =>
                tabularFormatDataExportHandler.ExportData(new DataExportProcessDetails(DataExportFormat.Tabular,
                    questionnaireIdentity,
                    "Enumerator Questions Automation")));
        }

        private async Task RunSynchronization()
        {
            var packagesService = ServiceLocator.Current.GetInstance<IInterviewPackagesService>();

            string interview;
            using (var streamReader = new StreamReader(testAssembly.GetManifestResourceStream("WB.Tests.Integration.FullStackIntegration.Hq.syncPackage.json")))
            {
                interview = await streamReader.ReadToEndAsync();
            }

            var interviewPackage = ServiceLocator.Current.GetInstance<ISerializer>().Deserialize<InterviewPackage>(interview);

            var plainTransaction = ServiceLocator.Current.GetInstance<IPlainTransactionManager>();
            plainTransaction.ExecuteInPlainTransaction(() => { packagesService.ProcessPackage(interviewPackage); });
        }

        private async Task CreateUsers()
        {
            using (var userManager = ServiceLocator.Current.GetInstance<HqUserManager>())
            {
                var supervisorId = Guid.Parse("437af4c9-c60e-44c5-b527-ae8f39fcee1a");
                await userManager.CreateUserAsync(new HqUser
                {
                    Id = supervisorId,
                    UserName = "sup"
                }, "1", UserRoles.Supervisor);

                await userManager.CreateUserAsync(new HqUser
                {
                    Id = Guid.Parse("35e19d19-e927-484e-a950-1e9e88a64684"),
                    UserName = "int",
                    Profile = new HqUserProfile
                        {
                            SupervisorId = supervisorId
                        }
                }, "1", UserRoles.Interviewer);
            }
        }

        private async Task ImportQuestionnaire()
        {
            var restService = await SetupMockOfDesigner();

            var authorizedUser = Mock.Of<IAuthorizedUser>();

            var questionnaireId = Guid.Parse("900c45084d9d4c63a54c8ae5f60d07e3");

            ThreadMarkerManager.MarkCurrentThreadAsIsolated();

            var transactionManager = ServiceLocator.Current.GetInstance<ITransactionManager>();
            var plainTransactionManager = ServiceLocator.Current.GetInstance<IPlainTransactionManager>();

            plainTransactionManager.BeginTransaction();
            transactionManager.BeginCommandTransaction();
            var importService = new QuestionnaireImportService(
                ServiceLocator.Current.GetInstance<ISupportedVersionProvider>(),
                restService.Object,
                ServiceLocator.Current.GetInstance<IStringCompressor>(),
                ServiceLocator.Current.GetInstance<IAttachmentContentService>(),
                ServiceLocator.Current.GetInstance<IQuestionnaireVersionProvider>(),
                ServiceLocator.Current.GetInstance<ITranslationManagementService>(),
                ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireLookupTable>>(),
                ServiceLocator.Current.GetInstance<ICommandService>(),
                Mock.Of<ILogger>(),
                ServiceLocator.Current.GetInstance<IAuditLog>(),
                authorizedUser,
                Mock.Of<DesignerUserCredentials>(x => x.Get() == new RestCredentials())
            );

            await importService.Import(questionnaireId, "name", false);

            transactionManager.CommitCommandTransaction();
            plainTransactionManager.CommitTransaction();
            ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
        }

        private async Task<Mock<IRestService>> SetupMockOfDesigner()
        {
            var questionnaireDocumentSerialized =
                File.ReadAllText(Path.Combine(questionnaireContentFolder, "Enumerator Questions Automation.json"));
            var questionnaireDocument = ServiceLocator.Current.GetInstance<ISerializer>()
                .Deserialize<QuestionnaireDocument>(questionnaireDocumentSerialized);
            var compiledAssembly = await GetCompiledAssembly();

            var restService = new Mock<IRestService>();
            restService.Setup(x => x.GetAsync<QuestionnaireCommunicationPackage>(It.IsAny<string>(),
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<object>(),
                    It.IsAny<RestCredentials>(),
                    null))
                .ReturnsAsync(new QuestionnaireCommunicationPackage
                {
                    Questionnaire = ServiceLocator.Current.GetInstance<IStringCompressor>()
                        .CompressString(questionnaireDocumentSerialized),
                    QuestionnaireAssembly = compiledAssembly,
                    QuestionnaireContentVersion =
                        new DesignerEngineVersionService().GetQuestionnaireContentVersion(questionnaireDocument)
                });

            restService.Setup(x => x.DownloadFileAsync("/api/hq/attachment/B8BD54B97DADF9A7C14A7BCA3F19F0EFBB8DBE2C",
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<RestCredentials>(),
                    null,
                    null))
                .ReturnsAsync(
                    new RestFile(
                        File.ReadAllBytes(Path.Combine(questionnaireContentFolder,
                            @"Attachments\3688615c5d725099d21ae4f84cf2a7b5\simple.jpg")),
                        "image/jpeg",
                        "hashImage",
                        null,
                        "simple.jpg",
                        HttpStatusCode.OK));

            restService.Setup(x => x.GetAsync<List<TranslationDto>>(It.IsAny<string>(),
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<object>(),
                    It.IsAny<RestCredentials>(),
                    null))
                .ReturnsAsync(new List<TranslationDto>());


            restService.Setup(x => x.GetAsync<QuestionnaireLookupTable>(It.IsAny<string>(),
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<object>(),
                    It.IsAny<RestCredentials>(),
                    null))
                .ReturnsAsync(new QuestionnaireLookupTable
                {
                    Content = File.ReadAllText(Path.Combine(questionnaireContentFolder, @"Lookup Tables\c0e4d24d5e62df4d2fc92e2f78889277.txt")),
                    FileName = "c0e4d24d5e62df4d2fc92e2f78889277.txt"
                });


            return restService;
        }

        private async Task<string> GetCompiledAssembly()
        {
            string compiledAssembly;
            using (var assemblyStream =
                testAssembly.GetManifestResourceStream("WB.Tests.Integration.FullStackIntegration.Hq.assembly.dll"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await assemblyStream.CopyToAsync(memoryStream);

                    byte[] assemblyContent = memoryStream.ToArray();
                    compiledAssembly = Convert.ToBase64String(assemblyContent);
                }
            }

            return compiledAssembly;
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
