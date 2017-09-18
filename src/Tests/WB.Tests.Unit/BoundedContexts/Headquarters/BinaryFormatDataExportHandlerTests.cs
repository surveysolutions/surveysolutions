using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.Tests.Unit.BoundedContexts.Headquarters.BinaryFormatDataExportHandlerTests;

namespace WB.Tests.Unit.BoundedContexts.Headquarters
{
    [TestOf(typeof(BinaryFormatDataExportHandler))]
    internal class BinaryDataExportHandlerTests : BinaryFormatDataExportHandlerTestContext
    {
        [Test]
        public void when_no_image_should_not_throw_ArgumentNullException()
        {
            //arrange

            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
            Guid interviewId = Guid.NewGuid();

            var questionnaireExportStructure =
                Create.Entity.QuestionnaireExportStructure(questionnaireId: questionnaireIdentity.QuestionnaireId,
                    version: questionnaireIdentity.Version);
            var headerToLevelMap = Create.Entity.HeaderStructureForLevel();

            var multiMediaQuestion = Create.Entity.ExportedQuestionHeaderItem();
            multiMediaQuestion.QuestionType = QuestionType.Multimedia;
            headerToLevelMap.HeaderItems.Add(multiMediaQuestion.PublicKey, multiMediaQuestion);

            questionnaireExportStructure.HeaderToLevelMap.Add(headerToLevelMap.LevelScopeVector, headerToLevelMap);

            var interviewSummary = Create.Entity.InterviewSummary(
                interviewId: interviewId,
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version);

            var mockOfInterviewFactory = new Mock<IInterviewFactory>();
            var interviewSummaryStorage = new TestInMemoryWriter<InterviewSummary>();

            interviewSummaryStorage.Store(interviewSummary, interviewId.FormatGuid());
            mockOfInterviewFactory.Setup(x=>x.GetInterviewData(interviewId)).Returns(
                Create.Entity.InterviewData(
                    Create.Entity.InterviewQuestion(
                        questionId: multiMediaQuestion.PublicKey,
                        answer: "var.jpg")));

            var questionnaireStorage = new Mock<IQuestionnaireExportStructureStorage>();

            questionnaireStorage.Setup(x => x.GetQuestionnaireExportStructure(Moq.It.IsAny<QuestionnaireIdentity>()))
                .Returns(questionnaireExportStructure);

            var plainInterviewFileStorageMock = new Mock<IImageFileStorage>();
            plainInterviewFileStorageMock.Setup(x => x.GetBinaryFilesForInterview(interviewId))
                .Returns(new[] {Create.Entity.InterviewBinaryDataDescriptor()}.ToList());
            plainInterviewFileStorageMock.Setup(x => x.GetInterviewBinaryData(interviewId, Moq.It.IsAny<string>()))
                .Returns((byte[])null);

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>((p1, p2) => p2);
            fileSystemAccessor.Setup(x=>x.WriteAllBytes(Moq.It.IsAny<string>(), Moq.It.IsAny<byte[]>())).Throws<ArgumentNullException>();

            var dataExportFileAccessor = CrerateDataExportFileAccessor(fileSystemAccessor.Object);

            var manager = new Mock<IPlainTransactionManager>();
            var plainTransactionManagerProvider = new Mock<IPlainTransactionManagerProvider>();
            plainTransactionManagerProvider.Setup(t => t.GetPlainTransactionManager()).Returns(manager.Object);

            var binaryFormatDataExportHandler =
                CreateBinaryFormatDataExportHandler(
                    interviewSummaries: interviewSummaryStorage,
                    interviewFactory: mockOfInterviewFactory.Object,
                    questionnaireExportStructureStorage: questionnaireStorage.Object,
                    imageFileRepository: plainInterviewFileStorageMock.Object,
                    fileSystemAccessor: fileSystemAccessor.Object,
                    dataExportFileAccessor: dataExportFileAccessor,
                    plainTransactionManagerProvider: plainTransactionManagerProvider.Object);

            //act
            //assert
            Assert.DoesNotThrow(() => binaryFormatDataExportHandler.ExportData(
                    Create.Entity.DataExportProcessDetails(questionnaireIdentity: questionnaireIdentity)));
        }
    }
}