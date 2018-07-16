using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire;
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

            var versionProvider = Setup.SupportedVersionProvider(ApiVersion.MaxQuestionnaireVersion);

            var mockOfRestService = NSubstitute.Substitute.For<IRestService>();
            mockOfRestService.GetAsync<QuestionnaireCommunicationPackage>(null, null, null, null, null)
                .ThrowsForAnyArgs(new RestException(exprectedErrorMessageFromServer, HttpStatusCode.ExpectationFailed));

            var service = CreateIQuestionnaireImportService(
                supportedVersionProvider: versionProvider, restService: mockOfRestService);

            //Act
            var importResult = await service.Import(Guid.NewGuid(), "null", false);

            //Assert
            Assert.That(importResult.ImportError, Is.EqualTo(exprectedErrorMessageFromServer));
        }

        [Test]
        public void when_importing_questionnaire_from_designer_and_command_service_throws_not_a_questionnaire_exception()
        {
            var supportedVerstion = 1;

            var versionProvider = Mock.Of<ISupportedVersionProvider>(x => x.GetSupportedQuestionnaireVersion() == supportedVerstion);

            var commandServiceException = new Exception();

            var commandService = new Mock<ICommandService>();
            commandService
                .Setup(cs => cs.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Throws(commandServiceException);

            var zipUtilsMock = Mock.Of<IStringCompressor>(_ => _.DecompressString<QuestionnaireDocument>(It.IsAny<string>()) == new QuestionnaireDocument(new List<IComposite>()));

            var restServiceMock = new Mock<IRestService>();
            restServiceMock.Setup(x => x.GetAsync<QuestionnaireCommunicationPackage>(It.IsAny<string>(),
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<object>(), 
                    It.IsAny<RestCredentials>(), It.IsAny<CancellationToken?>()))
                .Returns(Task.FromResult(new QuestionnaireCommunicationPackage()));

            var service = CreateIQuestionnaireImportService(commandService: commandService.Object,
                supportedVersionProvider: versionProvider, zipUtils: zipUtilsMock, restService: restServiceMock.Object);

            // Act-assert
            var exception = Assert.ThrowsAsync<Exception>(async () => await service.Import(Guid.NewGuid(), "null", false));
            Assert.That(exception, Is.SameAs(commandServiceException));
        }

        [Test]
        public async Task when_importing_questionnaire_with_attachments()
        {
            var versionProvider = Setup.SupportedVersionProvider(1);
            List<Attachment> questionnaireAttachments =
              new List<Attachment>(new[]
              {
                    Create.Entity.Attachment("Content 1"),
                    Create.Entity.Attachment("Content 2"),
                    Create.Entity.Attachment("Content 3")
              });

            var zipUtils = Setup.StringCompressor_Decompress(new QuestionnaireDocument() { Attachments = questionnaireAttachments });

            var mockOfRestService = new Mock<IRestService>();
            mockOfRestService.Setup(x =>
                x.DownloadFileAsync(It.IsAny<string>(), null, It.IsAny<RestCredentials>(), null, null)).Returns(Task.FromResult(new RestFile(new byte[] { 1 }, "image/png", "content id", 0, "file.png", HttpStatusCode.OK)));
            mockOfRestService.Setup(x => x.GetAsync<QuestionnaireCommunicationPackage>(It.IsAny<string>(), It.IsAny<IProgress<TransferProgress>>(), It.IsAny<object>(), It.IsAny<RestCredentials>(), It.IsAny<CancellationToken?>()))
                .Returns(Task.FromResult(new QuestionnaireCommunicationPackage()));

            var mockOfAttachmentContentService = new Mock<IAttachmentContentService>();
            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[0].ContentId)).Returns(true);
            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[1].ContentId)).Returns(false);
            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[2].ContentId)).Returns(false);

            var service = CreateIQuestionnaireImportService(attachmentContentService: mockOfAttachmentContentService.Object,
                supportedVersionProvider: versionProvider, zipUtils: zipUtils, restService: mockOfRestService.Object);

            // Act
            await service.Import(Guid.NewGuid(), "null", false);

            // Assert
            mockOfRestService.Verify(x => x.DownloadFileAsync(It.IsAny<string>(), null, It.IsAny<RestCredentials>(), null, null), Times.Exactly(2));
            mockOfAttachmentContentService.Verify(x => x.SaveAttachmentContent(questionnaireAttachments[0].ContentId, It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
            mockOfAttachmentContentService.Verify(x => x.SaveAttachmentContent(questionnaireAttachments[1].ContentId, It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
            mockOfAttachmentContentService.Verify(x => x.SaveAttachmentContent(questionnaireAttachments[2].ContentId, It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        }

        [Test]
        public async Task when_import_questionnaire_with_lookup_tables()
        {
            var versionProvider = Setup.SupportedVersionProvider(1);

            (Guid id, string content) lookup1 = (Id.g1, "content of lookup1");
            (Guid id, string content) lookup2 = (Id.g2, "content of lookup2");

            var zipUtils = Setup.StringCompressor_Decompress(new QuestionnaireDocument()
            {
                PublicKey = Id.gA,
                LookupTables = new Dictionary<Guid, LookupTable>
                {
                    { lookup1.id, new LookupTable()},
                    { lookup2.id, new LookupTable()}
                }
            });

            var lookupStorage = Mock.Of<IPlainKeyValueStorage<QuestionnaireLookupTable>>();

            var mockOfRestService = new Mock<IRestService>();
            mockOfRestService.Setup(x =>
                x.DownloadFileAsync(It.IsAny<string>(), null, It.IsAny<RestCredentials>(), null, null)).Returns(Task.FromResult(new RestFile(new byte[] { 1 }, "image/png", "content id", 0, "file.png", HttpStatusCode.OK)));
            mockOfRestService.Setup(x => x.GetAsync<QuestionnaireCommunicationPackage>(It.IsAny<string>(), It.IsAny<IProgress<TransferProgress>>(), It.IsAny<object>(), It.IsAny<RestCredentials>(), It.IsAny<CancellationToken?>()))
                .Returns(Task.FromResult(new QuestionnaireCommunicationPackage()));

            void SetupLookupQuery((Guid id, string content) lookup)
            {
                mockOfRestService
                    .Setup(x => x.GetAsync<QuestionnaireLookupTable>(
                        It.Is<string>(s => s.EndsWith(lookup.id.ToString())), 
                        It.IsAny<IProgress<TransferProgress>>(), It.IsAny<object>(), It.IsAny<RestCredentials>(), It.IsAny<CancellationToken?>()))
                    .Returns(Task.FromResult(new QuestionnaireLookupTable
                    {
                        Content = lookup.content,
                        FileName = lookup.content
                    }));
            }

            SetupLookupQuery(lookup1);
            SetupLookupQuery(lookup2);
            
            var service = CreateIQuestionnaireImportService(
                supportedVersionProvider: versionProvider, zipUtils: zipUtils, restService: mockOfRestService.Object, lookupStorage: lookupStorage);

            // Act
            await service.Import(Guid.NewGuid(), "null", false);

            // Assert

            Mock.Get(lookupStorage).Verify(ls => ls.Store(It.Is<QuestionnaireLookupTable>(lt => lt.Content == lookup1.content), It.IsAny<string>()), Times.Once);
            Mock.Get(lookupStorage).Verify(ls => ls.Store(It.Is<QuestionnaireLookupTable>(lt => lt.Content == lookup2.content), It.IsAny<string>()), Times.Once);

        }

        [Test]
        public async Task when_requesting_questionnaire_and_designer_service_throws_fault_exception()
        {
            var supportedVerstion = 1;
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            string someFaultReason = "some fault reason";

            var versionProvider = new Mock<ISupportedVersionProvider>();
            versionProvider.Setup(x => x.GetSupportedQuestionnaireVersion()).Returns(supportedVerstion);

            var service = new Mock<IRestService>();

            service
                .Setup(x => x.GetAsync<QuestionnaireCommunicationPackage>(It.IsAny<string>(),
                        It.IsAny<IProgress<TransferProgress>>(),
                        It.IsAny<object>(),
                        It.IsAny<RestCredentials>(),
                        It.IsAny<CancellationToken?>()))
                .Throws(new RestException(someFaultReason, HttpStatusCode.Unauthorized));

            var importService = CreateIQuestionnaireImportService(
                restService: service.Object,
                supportedVersionProvider: versionProvider.Object);

            // Act
            var result = await importService.Import(questionnaireId, "null", false);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ImportError, Is.EqualTo(someFaultReason));
        }

        protected static IQuestionnaireImportService CreateIQuestionnaireImportService(
      ICommandService commandService = null,
      IAuthorizedUser authorizedUser = null,
      IStringCompressor zipUtils = null,
      ILogger logger = null,
      IRestService restService = null,
      ISupportedVersionProvider supportedVersionProvider = null,
      IAttachmentContentService attachmentContentService = null,
      IPlainStorageAccessor<TranslationInstance> translationInstances = null,
      IQuestionnaireVersionProvider questionnaireVersionProvider = null,
      DesignerUserCredentials designerUserCredentials = null,
      IPlainKeyValueStorage<QuestionnaireLookupTable> lookupStorage = null
      )
        {
            var service = restService ?? Mock.Of<IRestService>();
            var globalInfoProvider = authorizedUser ?? new Mock<IAuthorizedUser> { DefaultValue = DefaultValue.Mock }.Object;

            if (designerUserCredentials == null)
            {
                var mockOfUserCredentials = new Mock<DesignerUserCredentials>();
                mockOfUserCredentials.Setup(x => x.Get()).Returns(new RestCredentials());
                designerUserCredentials = mockOfUserCredentials.Object;
            }

            IQuestionnaireImportService questionnaireImportService = new QuestionnaireImportService(
                supportedVersionProvider ?? Mock.Of<ISupportedVersionProvider>(),
                service,
                zipUtils ?? new Mock<IStringCompressor> { DefaultValue = DefaultValue.Mock }.Object,
                attachmentContentService ?? Mock.Of<IAttachmentContentService>(),
                questionnaireVersionProvider ?? Mock.Of<IQuestionnaireVersionProvider>(),
                Mock.Of<ITranslationManagementService>(),
                lookupStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireLookupTable>>(),
                commandService ?? Mock.Of<ICommandService>(),
                Mock.Of<ILogger>(),
                Mock.Of<IAuditLog>(),
                globalInfoProvider,
                designerUserCredentials);
            return questionnaireImportService;
        }
    }
}
