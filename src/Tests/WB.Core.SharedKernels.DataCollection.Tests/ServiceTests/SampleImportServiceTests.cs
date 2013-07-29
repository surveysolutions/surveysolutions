using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Services.SampleImport.DTO;
using WB.Core.SharedKernels.DataCollection.Services.SampleImport.SampleDataReaders;
using WB.Core.SharedKernels.DataCollection.Services.SampleImport.TemporaryDataAccessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Tests.ServiceTests
{
    public class SampleImportServiceTests
    {
        [Test]
        public void GetImportStatus_When_Import_is_absent_Then_Null_is_returned()
        {
            //arrange
            SampleImportService target = CreateSampleImportService();

            //act

            var result = target.GetImportStatus(Guid.NewGuid());

            //assert
            Assert.Null(result);
        }

        [Test]
        public void GetImportStatus_When_Import_finished_with_errors_Then_Result_with_error_message_is_returned()
        {

            //arrange
            Guid tempFileId = Guid.NewGuid();
            var tempFileStorageMock = new Mock<ITemporaryDataRepositoryAccessor>();
            tempFileStorageMock.Setup(x => x.GetByName<TempFileImportData>(tempFileId.ToString()))
                               .Returns(new TempFileImportData() {ErrorMassage = "some error", PublicKey = tempFileId});

            SampleImportService target = CreateSampleImportService(tempFileStorageMock.Object);

            //act

            var result = target.GetImportStatus(tempFileId);

            //assert
            Assert.That(result.ErrorMessage, Is.EqualTo("some error"));
        }

        [Test]
        public void ImportSampleAsync_When_Template_is_absent_Then_import_result_contains_error()
        {

            //arrange
            SampleImportService target = CreateSampleImportService();

            //act

            var importId = target.ImportSampleAsync(Guid.NewGuid(),null);

            var status = WhaitForCompletedImportResult(target, importId);

            //assert
            Assert.That(status.ErrorMessage, Is.EqualTo("Template Is Absent"));
        }

        [Test]
        public void ImportSampleAsync_When_Template_Featured_question_list_dont_match_source_header_Then_import_result_contains_error()
        {

            //arrange
            var templateId = Guid.NewGuid();
            var smallTemplateStorage = new Mock<IReadSideRepositoryWriter<QuestionnaireBrowseItem>>();
            var sampleMock = new Mock<ISampleRecordsAccessor>();
            sampleMock.Setup(x => x.Records).Returns(new string[][] {new string[] {"q"}, new string[] {"v"}});
            smallTemplateStorage.Setup(x => x.GetById(templateId))
                                .Returns(new QuestionnaireBrowseItem() {QuestionnaireId = templateId, FeaturedQuestions = new FeaturedQuestionItem[0]});

            SampleImportService target = CreateSampleImportService(null, smallTemplateStorage.Object);

            //act

            var importId = target.ImportSampleAsync(templateId, sampleMock.Object);

            var status = WhaitForCompletedImportResult(target, importId);

            //assert
            Assert.That(status.ErrorMessage, Is.EqualTo("invalid header Capiton"));
        }

        private ImportResult WhaitForCompletedImportResult(SampleImportService target, Guid importId)
        {
            var status = target.GetImportStatus(importId);

            while (!status.IsCompleted)
            {
                Thread.Sleep(1000);
                status = target.GetImportStatus(importId);
            }
            return status;
        }

        private SampleImportService CreateSampleImportService(
            ITemporaryDataRepositoryAccessor tempStorage = null,
            IReadSideRepositoryWriter<QuestionnaireBrowseItem> smallTemplateRepository = null)
        {
            return new SampleImportService(new Mock<IReadSideRepositoryWriter<QuestionnaireDocument>>().Object,
                                           smallTemplateRepository ??
                                           new InMemoryReadSideRepositoryAccessor<QuestionnaireBrowseItem>(),
                                           tempStorage ?? new InMemoryTemporaryDataRepositoryAccessor());
        }
    }
}
