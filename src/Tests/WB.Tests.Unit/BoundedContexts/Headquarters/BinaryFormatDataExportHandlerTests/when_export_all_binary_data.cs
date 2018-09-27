using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.BinaryFormatDataExportHandlerTests
{
    internal class when_export_all_binary_data : BinaryFormatDataExportHandlerTestContext
    {
        private static BinaryFormatDataExportHandler binaryFormatDataExportHandler;
        private static QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
        private static Guid interviewId = Guid.NewGuid();

        private static Mock<IImageFileStorage> plainInterviewFileStorageMock;
        private static Mock<IAudioFileStorage> audioFileStorage;
        private static Mock<IFileSystemAccessor> fileSystemAccessor;

        private const string audioFileName = "test.wav";
        private const string ImageFileName = "var.jpg";

        private ZipFile resultingArchive;
        private string tempFolder;
        
        [SetUp]
        public void Establish()
        {
            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.AudioQuestion(Guid.Parse("11111111111111111111111111111111"), "audio"),
                    Create.Entity.MultimediaQuestion()));

            var interviewSummary = Create.Entity.InterviewSummary(
                interviewId,
                questionnaireIdentity.QuestionnaireId,
                questionnaireIdentity.Version);

            var mockOfInterviewFactory = new Mock<IInterviewFactory>();
            var interviewSummaryStorage = new TestInMemoryWriter<InterviewSummary>();

            interviewSummaryStorage.Store(interviewSummary, interviewId.FormatGuid());
            mockOfInterviewFactory.Setup(x => x.GetMultimediaAnswersByQuestionnaire(questionnaireIdentity)).Returns(
                    new[]
                        {new InterviewStringAnswer {InterviewId = interviewId, Answer = ImageFileName}});
            mockOfInterviewFactory.Setup(x => x.GetAudioAnswersByQuestionnaire(questionnaireIdentity)).Returns(new[]
                {new InterviewStringAnswer {InterviewId = interviewId, Answer = audioFileName}});

            plainInterviewFileStorageMock = new Mock<IImageFileStorage>();
            plainInterviewFileStorageMock.Setup(x => x.GetBinaryFilesForInterview(interviewId))
                .Returns(new[] {Create.Entity.InterviewBinaryDataDescriptor()}.ToList());

            audioFileStorage = new Mock<IAudioFileStorage>();

            fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            fileSystemAccessor.Setup(x => x.GetFileName(It.IsAny<string>())).Returns<string>(Path.GetFileName);

            var ms = Create.Fake.MemoryStreamWithCallback(data =>
                resultingArchive = ZipFile.Read(new MemoryStream(data)));

            fileSystemAccessor
                .Setup(x => x.OpenOrCreateFile(It.IsAny<string>(), false))
                .Returns(ms);

            var dataExportFileAccessor = CrerateDataExportFileAccessor(fileSystemAccessor.Object);

            var manager = new Mock<IPlainTransactionManager>();
            var plainTransactionManagerProvider = new Mock<IPlainTransactionManagerProvider>();
            plainTransactionManagerProvider.Setup(t => t.GetPlainTransactionManager()).Returns(manager.Object);

            var filebasedExportedDataAccessor = new Mock<IFilebasedExportedDataAccessor>();
            filebasedExportedDataAccessor.Setup(s => s.GetArchiveFilePathForExportedData(
                    It.IsAny<QuestionnaireIdentity>(), DataExportFormat.Binary, null, null, null))
                .Returns("Name.zip");

            tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().FormatGuid());

            binaryFormatDataExportHandler =
                CreateBinaryFormatDataExportHandler(
                    interviewSummaries: interviewSummaryStorage,
                    interviewFactory: mockOfInterviewFactory.Object,
                    imageFileRepository: plainInterviewFileStorageMock.Object,
                    filebasedExportedDataAccessor: filebasedExportedDataAccessor.Object,
                    fileSystemAccessor: fileSystemAccessor.Object,
                    interviewDataExportSettings: new InterviewDataExportSettings(tempFolder, true, "http://localhost"),
                    dataExportFileAccessor: dataExportFileAccessor,
                    audioFileStorage: audioFileStorage.Object,
                    plainTransactionManagerProvider: plainTransactionManagerProvider.Object,
                    questionnaireStorage: questionnaireStorage);

            Because();
        }

        public void Because()
        {
            binaryFormatDataExportHandler.ExportData(Create.Entity.DataExportProcessDetails(questionnaireIdentity));
        }

        [Test]
        public void should_request_binary_data_for_answered_multimedia_question()
        {
            plainInterviewFileStorageMock.Verify(x => x.GetInterviewBinaryData(interviewId, ImageFileName), Times.Once);
        }
        
        [Test]
        public void should_write_answered_audio_question()
        {
            Assert.That(resultingArchive.Entries, Has.One
                .Property(nameof(ZipEntry.FileName))
                .EqualTo($"{interviewId.FormatGuid()}/{audioFileName}"));
        }

        [Test]
        public void should_write_answered_multimedia_question()
        {
            Assert.That(resultingArchive.Entries, Has.One
                .Property(nameof(ZipEntry.FileName))
                .EqualTo($"{interviewId.FormatGuid()}/{ImageFileName}"));
        }
    }
}
