using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation;
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
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Files;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation.Migrations;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLTests;
using WB.UI.Headquarters.Injections;
using WB.UI.Headquarters.Migrations.PlainStore;
using WB.UI.Headquarters.Migrations.ReadSide;
using WB.UI.Headquarters.Migrations.Users;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Versions;

namespace WB.Tests.Integration.FullStackIntegration.Hq
{
    [TestFixture]
    public class ShouldGenerateExportAsExpected : with_postgres_db
    {
        private string tempFolder;
        private string questionnaireContentFolder;
        private string exportFolder;
        private Assembly testAssembly;
        private readonly Guid questionanireId = Guid.Parse("6158dd074d64498f8a50e5e9828fda23");
        private string actualExportFolder;
        private string expectedExportFolder;
        private IServiceLocator oldServiceLocator;

        private const string resourcesNamespace = "WB.Tests.Integration.FullStackIntegration.Hq";

        [OneTimeSetUp]
        public void Setup()
        {
            tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            exportFolder = Path.Combine(tempFolder, "Export");
            actualExportFolder = Path.Combine(tempFolder, @"Actual");
            expectedExportFolder = Path.Combine(tempFolder, @"Expected");
            Directory.CreateDirectory(actualExportFolder);
            Directory.CreateDirectory(expectedExportFolder);

            Directory.CreateDirectory(tempFolder);
            testAssembly = Assembly.GetAssembly(typeof(ShouldGenerateExportAsExpected));
            var zipPath = Path.Combine(tempFolder, "quuestionnaire.zip");

            using (var stream = testAssembly.GetManifestResourceStream($"{resourcesNamespace}.Enumerator Questions Automation.zip"))
            {
                using (var outputStream = File.Create(zipPath))
                {
                    stream.CopyTo(outputStream);
                }
            }

            ZipArchiveUtils zipUtil = new ZipArchiveUtils();
            zipUtil.Unzip(zipPath, tempFolder, true);

            questionnaireContentFolder = Path.Combine(tempFolder, "Enumerator Questions Automation (6158dd074d64498f8a50e5e9828fda23)");

            ConfigureIoc();
        }

        [Test]
        public async Task Test()
        {
            await ImportQuestionnaire();
            await CreateUsers();
            await RunSynchronization();
            RunExport();
            await PrepareActualAndExpectedExportData();
            CompareProducedExportWithExpected();
        }

        private async Task ImportQuestionnaire()
        {
            var restService = await SetupMockOfDesigner();

            var authorizedUser = Mock.Of<IAuthorizedUser>();

            using (ScopeManager.BeginScope())
            {
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

                await importService.Import(questionanireId, "name", false);
                ServiceLocator.Current.GetInstance<IUnitOfWork>().AcceptChanges();
            }
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

        private async Task RunSynchronization()
        {
            string interview;
            var manifestResourceStream = testAssembly.GetManifestResourceStream($"{resourcesNamespace}.syncPackage.json");
            using (var streamReader = new StreamReader(manifestResourceStream))
            {
                interview = await streamReader.ReadToEndAsync();
            }

            var interviewPackage = ServiceLocator.Current.GetInstance<ISerializer>().Deserialize<InterviewPackage>(interview);

            using (ScopeManager.BeginScope())
            {
                var packagesService = ServiceLocator.Current.GetInstance<IInterviewPackagesService>();
                packagesService.ProcessPackage(interviewPackage);
                ServiceLocator.Current.GetInstance<IUnitOfWork>().AcceptChanges();
            }
        }

        private void RunExport()
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionanireId, 1);

            using (ScopeManager.BeginScope())
            {
                var tabularFormatDataExportHandler =ServiceLocator.Current.GetInstance<TabularFormatDataExportHandler>();
                tabularFormatDataExportHandler.ExportData(new DataExportProcessDetails(DataExportFormat.Tabular, questionnaireIdentity, "Enumerator Questions Automation"));
            }
        }

        private async Task PrepareActualAndExpectedExportData()
        {
            var archiveUtils = ServiceLocator.Current.GetInstance<IArchiveUtils>();
            var generatedExportFilePath = Path.Combine(tempFolder, @"Export\ExportedData\auto_1_Tabular_All.zip");
            archiveUtils.Unzip(generatedExportFilePath, actualExportFolder);

            var expectedArchivePath = Path.Combine(tempFolder, @"Expected\archive.zip");
            using (var expctedExportStream = testAssembly.GetManifestResourceStream($"{resourcesNamespace}.expectedExport.zip"))
            {
                using (var expectedExportFileStream = File.Create(expectedArchivePath))
                {
                    await expctedExportStream.CopyToAsync(expectedExportFileStream);
                }
            }

            archiveUtils.Unzip(expectedArchivePath, expectedExportFolder);
            File.Delete(expectedArchivePath);
        }

        private void CompareProducedExportWithExpected()
        {
            var expectedFiles = Directory.GetFiles(expectedExportFolder);
            var actualFiles = Directory.GetFiles(actualExportFolder);

            Assert.That(expectedFiles.Select(Path.GetFileName), Is.EquivalentTo(actualFiles.Select(Path.GetFileName)),
                "Number of files and file names in export should be not changed");
            foreach (var expectedFile in expectedFiles)
            {
                var expectedFileName = Path.GetFileName(expectedFile);
                if (expectedFileName == "export__readme.txt")
                {
                    var expectedContent = File.ReadAllLines(expectedFile).Skip(1);
                    var actualContent = File.ReadAllLines(actualFiles.Single(x => Path.GetFileName(x) == expectedFileName)).Skip(1); // first line contains version info and is always changing

                    Assert.That(expectedContent, Is.EquivalentTo(actualContent), $"Mismatch found in file {expectedFileName}");
                }
                else
                {
                    var expectedContent = File.ReadAllText(expectedFile);
                    var actualContent = File.ReadAllText(actualFiles.Single(x => Path.GetFileName(x) == expectedFileName));
                    Assert.That(actualContent, Is.EqualTo(expectedContent), $"Mismatch found in file {expectedFileName}");
                }
            }
        }

        private void ConfigureIoc()
        {
            var kernel = new AutofacKernel();

            var unitOfWorkModule = new UnitOfWorkConnectionSettings();
            unitOfWorkModule.ConnectionString = ConnectionStringBuilder.ConnectionString;
            unitOfWorkModule.ReadSideMappingAssemblies = new List<Assembly> {typeof(HeadquartersBoundedContextModule).Assembly};
            unitOfWorkModule.ReadSideUpgradeSettings = DbUpgradeSettings.FromFirstMigration<M001_InitDb>();
            unitOfWorkModule.PlainMappingAssemblies = new List<Assembly>
            {
                typeof(HeadquartersBoundedContextModule).Assembly,
                typeof(ProductVersionModule).Assembly,
            };
            unitOfWorkModule.PlainStoreUpgradeSettings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace);

            var hqBoundedContext = new HeadquartersBoundedContextModule(tempFolder,
                new SyncPackagesProcessorBackgroundJobSetting(false, 0, 0, 1),
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
                new DbUpgradeSettings(typeof(M001_AddEventSequenceIndex).Assembly,
                    typeof(M001_AddEventSequenceIndex).Namespace));

            Database.SetInitializer(new FluentMigratorInitializer<HQIdentityDbContext>("users",
                DbUpgradeSettings.FromFirstMigration<M001_AddUsersHqIdentityModel>()));

            kernel.Load(
                new NLogLoggingModule(),
                new InfrastructureModule(),
                new FileInfrastructureModule(),
                new EventSourcedInfrastructureModule(),
                new NcqrsModule(),
                new ProductVersionModule(typeof(HeadquartersUIModule).Assembly, false),
                new TestModule(),
                eventStoreModule,
                new OrmModule(unitOfWorkModule),
                hqBoundedContext,
                new QuartzModule(),
                new FileStorageModule(tempFolder),
                new OwinSecurityModule());

            oldServiceLocator = ServiceLocator.Current;

            kernel.Init().Wait();
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
                testAssembly.GetManifestResourceStream($"{resourcesNamespace}.assembly.dll"))
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

        class TestModule : IModule
        {
            public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
            {
                return Task.CompletedTask;
            }

            public void Load(IIocRegistry registry)
            {
                registry.BindAsSingleton<IEventSourcedAggregateRootRepository,EventSourcedAggregateRootRepositoryWithExtendedCache>();
                registry.BindAsSingleton<IAggregateRootCacheCleaner, EventSourcedAggregateRootRepositoryWithExtendedCache>();
                registry.BindToMethod(Mock.Of<IWebInterviewNotificationService>);

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

        [OneTimeTearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            ServiceLocator.SetLocatorProvider(() => oldServiceLocator);
        }
    }
}
