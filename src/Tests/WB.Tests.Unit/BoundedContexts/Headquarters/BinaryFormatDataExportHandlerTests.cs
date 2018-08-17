using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation;
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

            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.MultimediaQuestion()));

            var interviewSummary = Create.Entity.InterviewSummary(
                interviewId: interviewId,
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version);

            var mockOfInterviewFactory = new Mock<IInterviewFactory>();
            var interviewSummaryStorage = new TestInMemoryWriter<InterviewSummary>();

            interviewSummaryStorage.Store(interviewSummary, interviewId.FormatGuid());
            mockOfInterviewFactory.Setup(x => x.GetMultimediaAnswersByQuestionnaire(questionnaireIdentity))
                .Returns(new[] {new InterviewStringAnswer {InterviewId = interviewId, Answer = "var.jpg"}});

            var plainInterviewFileStorageMock = new Mock<IImageFileStorage>();
            plainInterviewFileStorageMock.Setup(x => x.GetBinaryFilesForInterview(interviewId))
                .Returns(new[] {Create.Entity.InterviewBinaryDataDescriptor()}.ToList());
            plainInterviewFileStorageMock.Setup(x => x.GetInterviewBinaryData(interviewId, Moq.It.IsAny<string>()))
                .Returns((byte[]) null);

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>((p1, p2) => p1 + p2);
            fileSystemAccessor.Setup(x => x.GetFileName(It.IsAny<string>())).Returns<string>(p => p);
            fileSystemAccessor.Setup(x => x.WriteAllBytes(Moq.It.IsAny<string>(), Moq.It.IsAny<byte[]>()))
                .Throws<ArgumentNullException>();
            fileSystemAccessor.Setup(x => x.OpenOrCreateFile(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(() => new MemoryStream());

            var dataExportFileAccessor = CrerateDataExportFileAccessor(fileSystemAccessor.Object);

            var exportedDataAccessor = new Mock<IFilebasedExportedDataAccessor>();
            exportedDataAccessor.Setup(f => f.GetArchiveFilePathForExportedData(
                    It.IsAny<QuestionnaireIdentity>(), DataExportFormat.Binary, null, null, null))
                .Returns("TestFileData");

            var binaryFormatDataExportHandler =
                CreateBinaryFormatDataExportHandler(
                    interviewSummaries: interviewSummaryStorage,
                    interviewFactory: mockOfInterviewFactory.Object,
                    imageFileRepository: plainInterviewFileStorageMock.Object,
                    filebasedExportedDataAccessor: exportedDataAccessor.Object,
                    fileSystemAccessor: fileSystemAccessor.Object,
                    dataExportFileAccessor: dataExportFileAccessor,
                    questionnaireStorage: questionnaireStorage);

            //act
            //assert
            Assert.DoesNotThrow(() => binaryFormatDataExportHandler.ExportData(
                Create.Entity.DataExportProcessDetails(questionnaireIdentity: questionnaireIdentity)));
        }
    }
}
