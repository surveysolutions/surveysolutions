using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.Questionnaire.Synchronization.Designer;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireImportService))]
    public class QuestionnaireImportServiceTests
    {
        [Test]
        public async Task when_importing_questionnaire_and_server_return_expectation_failed_status()
        {
            var exprectedErrorMessageFromServer = "expected error message from server";

            var versionProvider = SetUp.SupportedVersionProvider(ApiVersion.MaxQuestionnaireVersion);

            var rest = new Mock<IDesignerApi>();

            rest.Setup(s => s.GetQuestionnaire(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int?>()))
            .Throws(new RestException(exprectedErrorMessageFromServer, HttpStatusCode.ExpectationFailed));

            var service = CreateIQuestionnaireImportService(
                supportedVersionProvider: versionProvider, designerApi: rest.Object);

            //Act
            var importResult = await service.Import(Guid.NewGuid(), "null", false, null, null, includePdf: false);

            //Assert
            Assert.That(importResult.ImportError, Is.EqualTo(exprectedErrorMessageFromServer));
        }

        [Test]
        public async Task when_importing_questionnaire_and_rest_service_error_occured_should_not_commit_transaction()
        {
            var versionProvider = SetUp.SupportedVersionProvider(ApiVersion.MaxQuestionnaireVersion);
            var uow = GetUnitOfWorkMock();

            var rest = new Mock<IDesignerApi>();

            rest.Setup(s => s.GetQuestionnaire(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int?>()))
                .Throws(new RestException(""));

            var service = CreateIQuestionnaireImportService(supportedVersionProvider: versionProvider, 
                designerApi: rest.Object, unitOfWork: uow.Object);

            //Act
            await service.Import(Guid.NewGuid(), "null", false, null, null, includePdf: false);

            //Assert
            uow.Verify(u => u.DiscardChanges(), Times.Once);
        }

        [Test]
        public async Task when_importing_questionnaire_and_questionnaire_already_exists_should_not_commit_transaction()
        {
            var uow = GetUnitOfWorkMock();

            var versionProvider = Mock.Of<ISupportedVersionProvider>(x => x.GetSupportedQuestionnaireVersion() == 1);

            var commandService = new Mock<ICommandService>();
            commandService
                .Setup(cs => cs.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Throws(new QuestionnaireAssemblyAlreadyExistsException("", Create.Entity.QuestionnaireIdentity()));

            var zipUtilsMock = Mock.Of<IStringCompressor>(_ => _.DecompressString<QuestionnaireDocument>(It.IsAny<string>()) == new QuestionnaireDocument(new List<IComposite>()));
            var designerApi = new Mock<IDesignerApi>();
            SetupGetQuestionnaire(designerApi);

            var service = CreateIQuestionnaireImportService(commandService: commandService.Object,
                supportedVersionProvider: versionProvider, zipUtils: zipUtilsMock, 
                designerApi: designerApi.Object, unitOfWork: uow.Object);

            // Act
            await service.Import(Guid.NewGuid(), "null", false, null, null, includePdf: false);
            
            //Assert
            uow.Verify(u => u.DiscardChanges(), Times.Once);
        }

        [Test]
        public async Task when_importing_questionnaire_from_designer_and_command_service_throws_not_a_questionnaire_exception()
        {
            var supportedVersion = 1;

            var versionProvider = Mock.Of<ISupportedVersionProvider>(x => x.GetSupportedQuestionnaireVersion() == supportedVersion);

            var commandServiceException = new Exception("meessage");

            var commandService = new Mock<ICommandService>();
            commandService
                .Setup(cs => cs.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Throws(commandServiceException);

            var zipUtilsMock = Mock.Of<IStringCompressor>(_ => _.DecompressString<QuestionnaireDocument>(It.IsAny<string>()) == new QuestionnaireDocument(new List<IComposite>()));

            var designerApi = new Mock<IDesignerApi>();
            SetupGetQuestionnaire(designerApi);
            var uow = GetUnitOfWorkMock();

            var service = CreateIQuestionnaireImportService(commandService: commandService.Object,
                supportedVersionProvider: versionProvider, zipUtils: zipUtilsMock, designerApi: designerApi.Object, unitOfWork: uow.Object);

            // Act-assert
            var importResult = await service.Import(Guid.NewGuid(), "null", false, null, null, includePdf: false);

            Assert.That(importResult.Status, Is.EqualTo(QuestionnaireImportStatus.Error));
            Assert.That(importResult.ImportError, Is.Not.Empty);
            uow.Verify(u => u.DiscardChanges(), Times.Once);
        }

        [Test]
        public async Task when_importing_questionnaire_with_attachments()
        {
            var versionProvider = SetUp.SupportedVersionProvider(1);
            List<Attachment> questionnaireAttachments =
              new List<Attachment>(new[]
              {
                    Create.Entity.Attachment("Content 1"),
                    Create.Entity.Attachment("Content 2"),
                    Create.Entity.Attachment("Content 3")
              });

            var zipUtils = SetUp.StringCompressor_Decompress(new QuestionnaireDocument()
            {
                Attachments = questionnaireAttachments
            });

            var designerApi = new Mock<IDesignerApi>();
            SetupGetQuestionnaire(designerApi);
            SetupDownloadAttachment(designerApi);
            
            SetupDownloadQuestionnaireBackup(designerApi);
            
            var mockOfAttachmentContentService = new Mock<IAttachmentContentService>();
            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[0].ContentId)).Returns(true);
            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[1].ContentId)).Returns(false);
            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[2].ContentId)).Returns(false);

            var files = new Dictionary<string, long>();
            foreach (var questionnaireAttachment in questionnaireAttachments)
            {
                files.Add(questionnaireAttachment.AttachmentId.FormatGuid() + "/content-type.txt", 3);
                files.Add(questionnaireAttachment.AttachmentId.FormatGuid() + "/att.png", 3);
            }
            
            var archiveUtils = new Mock<IArchiveUtils>();
            archiveUtils.Setup(x => x.GetArchivedFileNamesAndSize(It.IsAny<byte[]>()))
                .Returns(files);

            foreach (var questionnaireAttachment in questionnaireAttachments)
            {
                archiveUtils.Setup(x => x.GetFileFromArchive(It.IsAny<byte[]>(), questionnaireAttachment.AttachmentId.FormatGuid() + "/content-type.txt"))
                    .Returns(new ExtractedFile()
                    {
                        Bytes = Encoding.UTF8.GetBytes("image/png")

                    });

                archiveUtils.Setup(x => x.GetFileFromArchive(It.IsAny<byte[]>(), questionnaireAttachment.AttachmentId.FormatGuid() + "/att.png"))
                    .Returns(new ExtractedFile()
                    {
                        Bytes = Encoding.UTF8.GetBytes("anything")

                    });
            }

            var service = CreateIQuestionnaireImportService(attachmentContentService: mockOfAttachmentContentService.Object,
                supportedVersionProvider: versionProvider, zipUtils: zipUtils, designerApi: designerApi.Object, archiveUtils:archiveUtils.Object);

            // Act
            await service.Import(Guid.NewGuid(), "null", false, null, null, includePdf: false);

            // Assert
            mockOfAttachmentContentService.Verify(x => 
                    x.SaveAttachmentContent(questionnaireAttachments[0].ContentId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), 
                Times.Never);
            mockOfAttachmentContentService.Verify(x => 
                    x.SaveAttachmentContent(questionnaireAttachments[1].ContentId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), 
                Times.Once);
            mockOfAttachmentContentService.Verify(x => 
                    x.SaveAttachmentContent(questionnaireAttachments[2].ContentId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), 
                Times.Once);
        }

        private void SetupGetQuestionnaire(Mock<IDesignerApi> designerApi, QuestionnaireCommunicationPackage package = null)
        {
            designerApi
                .Setup(d => d.GetQuestionnaire(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(Task.FromResult(package ?? new QuestionnaireCommunicationPackage(String.Empty,string.Empty, 0)));

            designerApi
                .Setup(d => d.GetPdfStatus(It.IsAny<Guid>(), It.IsAny<Guid?>()))
                .Returns(Task.FromResult(new PdfStatus { ReadyForDownload = true }));
            
            designerApi
                .Setup(d => d.DownloadPdf(It.IsAny<Guid>(), It.IsAny<Guid?>()))
                .Returns(Task.FromResult(new RestFile(new byte[] { 1 }, "image/png", "content id", 0, "file.png", HttpStatusCode.OK)));
        }

        private void SetupDownloadAttachment(Mock<IDesignerApi> designerApi, RestFile file = null)
        {
            designerApi
                .Setup(d => d.DownloadQuestionnaireAttachment(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(file ?? 
                    new RestFile(new byte[] { 1 }, "image/png", "content id", 0, "file.png", HttpStatusCode.OK)));
        }

        private void SetupDownloadQuestionnaireBackup(Mock<IDesignerApi> designerApi, RestFile file = null)
        {
            designerApi
                .Setup(d => d.DownloadQuestionnaireBackup(It.IsAny<Guid>()))
                .Returns(Task.FromResult(file ??
                                         new RestFile(new byte[] { 1 }, "application/zip", "content id", 0, "file.zip", HttpStatusCode.OK)));
        }

        [Test]
        public async Task when_import_questionnaire_with_lookup_tables()
        {
            var versionProvider = SetUp.SupportedVersionProvider(1);

            (Guid id, string content) lookup1 = (Id.g1, "content of lookup1");
            (Guid id, string content) lookup2 = (Id.g2, "content of lookup2");

            var zipUtils = SetUp.StringCompressor_Decompress(new QuestionnaireDocument()
            {
                PublicKey = Id.gA,
                LookupTables = new Dictionary<Guid, LookupTable>
                {
                    { lookup1.id, new LookupTable()},
                    { lookup2.id, new LookupTable()}
                }
            });

            var lookupStorage = Mock.Of<IPlainKeyValueStorage<QuestionnaireLookupTable>>();

            var designerApi = new Mock<IDesignerApi>();
            SetupGetQuestionnaire(designerApi);
            SetupDownloadAttachment(designerApi);

            SetupDownloadQuestionnaireBackup(designerApi);

            void SetupLookupQuery((Guid id, string content) lookup)
            {
                designerApi
                    .Setup(d => d.GetLookupTables(It.IsAny<Guid>(), lookup.id))
                    .Returns(Task.FromResult(new QuestionnaireLookupTable
                    {
                        Content = lookup.content,
                        FileName = lookup.content
                    }));                
            }

            SetupLookupQuery(lookup1);
            SetupLookupQuery(lookup2);

            var archiveUtils = new Mock<IArchiveUtils>();
            archiveUtils.Setup(x => x.GetArchivedFileNamesAndSize(It.IsAny<byte[]>()))
                .Returns(new Dictionary<string, long>()
                {
                    {lookup1.id.FormatGuid(), 1},
                    {lookup2.id.FormatGuid(), 1},
                });

            archiveUtils.Setup(x => x.GetFileFromArchive(It.IsAny<byte[]>(), lookup1.id.FormatGuid()))
                .Returns(new ExtractedFile()
                {
                    Bytes = Encoding.UTF8.GetBytes(lookup1.content)

                });

            archiveUtils.Setup(x => x.GetFileFromArchive(It.IsAny<byte[]>(), lookup2.id.FormatGuid()))
                .Returns(new ExtractedFile()
                {
                    Bytes = Encoding.UTF8.GetBytes(lookup2.content)

                });

            var service = CreateIQuestionnaireImportService(
                supportedVersionProvider: versionProvider, zipUtils: zipUtils, designerApi: designerApi.Object, 
                lookupStorage: lookupStorage, archiveUtils: archiveUtils.Object);

            // Act
            await service.Import(Guid.NewGuid(), "null", false, null, null, includePdf: false);

            // Assert

            Mock.Get(lookupStorage).Verify(ls => 
                ls.Store(It.Is<QuestionnaireLookupTable>(lt => lt.Content == lookup1.content), It.IsAny<string>()), 
                Times.Once);
            Mock.Get(lookupStorage).Verify(ls => 
                ls.Store(It.Is<QuestionnaireLookupTable>(lt => lt.Content == lookup2.content), It.IsAny<string>()), Times.Once);

        }

        [Test]
        public async Task when_requesting_questionnaire_and_designer_service_throws_fault_exception()
        {
            var supportedVerstion = 1;
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            string someFaultReason = "some fault reason";

            var versionProvider = new Mock<ISupportedVersionProvider>();
            versionProvider.Setup(x => x.GetSupportedQuestionnaireVersion()).Returns(supportedVerstion);

            var designerApi = new Mock<IDesignerApi>();

            designerApi
                .Setup(d => d.GetQuestionnaire(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int?>()))
                .Throws(new RestException(someFaultReason, statusCode: HttpStatusCode.Unauthorized));
            
            var importService = CreateIQuestionnaireImportService(
                designerApi: designerApi.Object,
                supportedVersionProvider: versionProvider.Object);

            // Act
            var result = await importService.Import(questionnaireId, "null", false, null, null, includePdf: false);

            // Assert
            Assert.That(result.Status, Is.EqualTo(QuestionnaireImportStatus.Error));
            Assert.That(result.ImportError, Is.EqualTo(someFaultReason));
        }

        [Test]
        public async Task should_tag_imported_questionnaire_with_hq_server_data()
        {
            var supportedVerstion = 1;
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");

            var versionProvider = new Mock<ISupportedVersionProvider>();
            versionProvider.Setup(x => x.GetSupportedQuestionnaireVersion()).Returns(supportedVerstion);

            var zipUtils = SetUp.StringCompressor_Decompress(new QuestionnaireDocument()
            {
                PublicKey = Id.gA
            });

            var designerApi = new Mock<IDesignerApi>();
            SetupGetQuestionnaire(designerApi);

            var importService = CreateIQuestionnaireImportService(
                designerApi: designerApi.Object,
                zipUtils: zipUtils,
                supportedVersionProvider: versionProvider.Object,
                authorizedUser: Mock.Of<IAuthorizedUser>(u => u.UserName == "Zorge")
            );

            // act
            await importService.Import(Guid.NewGuid(), "questionnaire1", false, null, "http://fsb.ru", includePdf: false);

            designerApi.Verify(d => d.UpdateRevisionMetadata(It.IsAny<Guid>(), It.IsAny<int>(), It.Is<QuestionnaireRevisionMetadataModel>(m =>
                m.HqHost == "fsb.ru"
                && m.HqImporterLogin == "Zorge"
                && m.HqQuestionnaireVersion == 0
                )), Times.Once);
        }

        private static Mock<IUnitOfWork> GetUnitOfWorkMock()
        {
            var session = Mock.Of<NHibernate.ISession>(s => s.CreateSQLQuery(It.IsAny<string>()) == Mock.Of<ISQLQuery>());
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(m => m.Session).Returns(session);
            return mock;
        }

        protected static IQuestionnaireImportService CreateIQuestionnaireImportService(
          ICommandService commandService = null,
          IAuthorizedUser authorizedUser = null,
          IStringCompressor zipUtils = null,
          ILogger logger = null,
          IDesignerApi designerApi = null,
          ISupportedVersionProvider supportedVersionProvider = null,
          IAttachmentContentService attachmentContentService = null,
          IPlainStorageAccessor<TranslationInstance> translationInstances = null,
          IQuestionnaireVersionProvider questionnaireVersionProvider = null,
          IDesignerUserCredentials designerUserCredentials = null,
          IPlainKeyValueStorage<QuestionnaireLookupTable> lookupStorage = null,
          IUnitOfWork unitOfWork = null,
          IArchiveUtils archiveUtils = null,
          ICategoriesImporter categoriesImporter = null,
          ITranslationImporter translationImporter = null
      )
        {
            var globalInfoProvider = authorizedUser ?? new Mock<IAuthorizedUser> { DefaultValue = DefaultValue.Mock }.Object;

            if (designerUserCredentials == null)
            {
                var mockOfUserCredentials = new Mock<IDesignerUserCredentials>();
                mockOfUserCredentials.Setup(x => x.Get()).Returns(new RestCredentials());
                designerUserCredentials = mockOfUserCredentials.Object;
            }

            unitOfWork ??= GetUnitOfWorkMock().Object;

            var serviceLocatorNestedMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            serviceLocatorNestedMock.Setup(s => s.GetInstance<IDesignerApi>())
                .Returns(designerApi);

            var executor = new NoScopeInScopeExecutor(serviceLocatorNestedMock.Object);

            IQuestionnaireImportService questionnaireImportService = new QuestionnaireImportService(
                zipUtils ?? new Mock<IStringCompressor> { DefaultValue = DefaultValue.Mock }.Object,
                Mock.Of<ILogger>(),
                globalInfoProvider,
                new QuestionnaireImportStatuses(),
                Mock.Of<IAssignmentsUpgradeService>(),
                archiveUtils ?? Mock.Of<IArchiveUtils>(),
                Mock.Of<IDesignerUserCredentials>(),
                executor);

            serviceLocatorNestedMock.Setup(x => x.GetInstance<IQuestionnaireImportService>()).Returns(questionnaireImportService);
            serviceLocatorNestedMock.Setup(x => x.GetInstance<IPlainKeyValueStorage<QuestionnaireLookupTable>>())
                .Returns(lookupStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireLookupTable>>());
            serviceLocatorNestedMock.Setup(x => x.GetInstance<IUnitOfWork>())
                .Returns(unitOfWork);
            serviceLocatorNestedMock.Setup(x => x.GetInstance<IAttachmentContentService>())
                .Returns(attachmentContentService ?? Mock.Of<IAttachmentContentService>());
            serviceLocatorNestedMock.Setup(x => x.GetInstance<ISupportedVersionProvider>())
                .Returns(supportedVersionProvider ?? Mock.Of<ISupportedVersionProvider>());
            serviceLocatorNestedMock.Setup(x => x.GetInstance< IQuestionnaireVersionProvider> ())
                .Returns(questionnaireVersionProvider ?? Mock.Of<IQuestionnaireVersionProvider>());

            serviceLocatorNestedMock.Setup(x => x.GetInstance<ICommandService> ())
                .Returns(commandService ?? Mock.Of< ICommandService> ());

            serviceLocatorNestedMock.Setup(x => x.GetInstance<ICategoriesImporter>())
                .Returns(categoriesImporter ?? Mock.Of<ICategoriesImporter>());

            serviceLocatorNestedMock.Setup(x => x.GetInstance<ITranslationImporter>())
                .Returns(translationImporter ?? Mock.Of<ITranslationImporter>());

            return questionnaireImportService;
        }
    }
}
