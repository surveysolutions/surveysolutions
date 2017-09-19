using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
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
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.BinaryFormatDataExportHandlerTests
{
    internal class when_export_all_binary_data: BinaryFormatDataExportHandlerTestContext
    {
        private Establish context = () =>
        {
            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.AudioQuestion(Guid.Parse("11111111111111111111111111111111"), "audio"),
                    Create.Entity.MultimediaQuestion()));

            var interviewSummary = Create.Entity.InterviewSummary(
                interviewId: interviewId, 
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version);

            var mockOfInterviewFactory = new Mock<IInterviewFactory>();
            var interviewSummaryStorage = new TestInMemoryWriter<InterviewSummary>();

            interviewSummaryStorage.Store(interviewSummary, interviewId.FormatGuid());
            mockOfInterviewFactory.Setup(x=>x.GetMultimediaAnswers(interviewId, Moq.It.IsAny<Guid[]>())).Returns(new []{"var.jpg"});
            mockOfInterviewFactory.Setup(x => x.GetAudioAnswers(interviewId, Moq.It.IsAny<Guid[]>())).Returns(new[] { audioFileName });
            
            plainInterviewFileStorageMock = new Mock<IImageFileStorage>();
            plainInterviewFileStorageMock.Setup(x => x.GetBinaryFilesForInterview(interviewId))
                .Returns(new[] {Create.Entity.InterviewBinaryDataDescriptor()}.ToList());

            audioFileStorage = new Mock<IAudioFileStorage>();
            
            fileSystemAccessor =new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>((p1, p2) => p2);

            var dataExportFileAccessor = CrerateDataExportFileAccessor(fileSystemAccessor.Object);

            var manager = new Mock<IPlainTransactionManager>();
            var plainTransactionManagerProvider = new Mock<IPlainTransactionManagerProvider>();
            plainTransactionManagerProvider.Setup(t => t.GetPlainTransactionManager()).Returns(manager.Object);

            binaryFormatDataExportHandler =
                CreateBinaryFormatDataExportHandler(
                    interviewSummaries: interviewSummaryStorage,
                    interviewFactory: mockOfInterviewFactory.Object,
                    imageFileRepository: plainInterviewFileStorageMock.Object,
                    fileSystemAccessor: fileSystemAccessor.Object,
                    dataExportFileAccessor: dataExportFileAccessor,
                    audioFileStorage : audioFileStorage.Object,
                    plainTransactionManagerProvider: plainTransactionManagerProvider.Object,
                    questionnaireStorage: questionnaireStorage);
        };

        Because of = () => binaryFormatDataExportHandler.ExportData(Create.Entity.DataExportProcessDetails(questionnaireIdentity: questionnaireIdentity));

        It should_request_binary_data_for_answered_multimedia_question =
            () => plainInterviewFileStorageMock.Verify(x=>x.GetInterviewBinaryData(interviewId, "var.jpg"), Times.Once);

        It should_write_answered_multimedia_question =
          () => fileSystemAccessor.Verify(x => x.WriteAllBytes("var.jpg", Moq.It.IsAny<byte[]>()), Times.Once);

        It should_write_answered_audio_question =
            () => fileSystemAccessor.Verify(x => x.WriteAllBytes(audioFileName, Moq.It.IsAny<byte[]>()), Times.Once);

        private static BinaryFormatDataExportHandler binaryFormatDataExportHandler;
        private static QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
        private static Guid interviewId = Guid.NewGuid();
        private static Mock<IImageFileStorage> plainInterviewFileStorageMock;
        private static Mock<IAudioFileStorage> audioFileStorage;
        
        private static Mock<IFileSystemAccessor> fileSystemAccessor;

        private static string audioFileName = "test.wav";
    }
}