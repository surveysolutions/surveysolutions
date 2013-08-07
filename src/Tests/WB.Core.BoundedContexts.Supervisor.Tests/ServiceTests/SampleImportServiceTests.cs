using System;
using System.Linq;
using System.Threading;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.SampleImport;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests
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

        [Test]
        [TestCase(2,2,2)]
        [TestCase(4, 2,2)]
        [TestCase(2, 2, 12)]
        [TestCase(4, 2, 12)]
        public void ImportSampleAsync_When_Featured_questions_list_is_acceptable_Then_import_result_marked_as_successefull(
            int featuredQuestionsCount, 
            int presentQuestions,
            int valuesCount)
        {

            //arrange
            var templateId = Guid.NewGuid();
            var avalibleFeaturedQuestions = new FeaturedQuestionItem[featuredQuestionsCount];
            var randomizer = new Random();
            for (int i = 0; i < featuredQuestionsCount; i++)
            {
                avalibleFeaturedQuestions[i] = new FeaturedQuestionItem(Guid.NewGuid(), randomizer.Next().ToString(),
                                                                        randomizer.Next().ToString());
            }

            var smallTemplateStorage = new Mock<IReadSideRepositoryWriter<QuestionnaireBrowseItem>>();

            var values =new string[valuesCount+1][];
            values[0] = avalibleFeaturedQuestions.Take(presentQuestions).Select(q => q.Caption).ToArray();
            for (int i = 0; i < valuesCount; i++)
            {
                values[i + 1] =
                    avalibleFeaturedQuestions.Take(presentQuestions).Select(q => randomizer.Next().ToString()).ToArray();
            }


            var sampleMock = new Mock<ISampleRecordsAccessor>();
            sampleMock.Setup(x => x.Records)
                      .Returns(values);
            smallTemplateStorage.Setup(x => x.GetById(templateId))
                                .Returns(new QuestionnaireBrowseItem() { QuestionnaireId = templateId, FeaturedQuestions = avalibleFeaturedQuestions });

            SampleImportService target = CreateSampleImportService(null, smallTemplateStorage.Object);

            //act

            var importId = target.ImportSampleAsync(templateId, sampleMock.Object);

            //assert
            var status = WhaitForCompletedImportResult(target, importId);

            //assert
            Assert.True(status.IsSuccessed);
            Assert.True(status.IsCompleted);
            Assert.That(status.Values.Length, Is.EqualTo(valuesCount));
            Assert.That(status.Header.Length, Is.EqualTo(presentQuestions));
        }


        [Test]
        public void ImportSampleAsync_When_Featured_questions_list_is_lest_then_avalible_Then_import_result_marked_as_successefull()
        {

            //arrange
            var templateId = Guid.NewGuid();
            var avalibleFeaturedQuestions = new FeaturedQuestionItem[4];
            for (int i = 0; i < avalibleFeaturedQuestions.Length; i++)
            {
                avalibleFeaturedQuestions[i] = new FeaturedQuestionItem(Guid.NewGuid(), "title", "q" + i);
            }

            var smallTemplateStorage = new Mock<IReadSideRepositoryWriter<QuestionnaireBrowseItem>>();

            var values = new string[3][];
            values[0] =new string[]{"q1","q2"};
            for (int i = 1; i < values.Length; i++)
            {
                values[i] = new string[] { "a1", "a2" };
            }


            var sampleMock = new Mock<ISampleRecordsAccessor>();
            sampleMock.Setup(x => x.Records)
                      .Returns(values);
            smallTemplateStorage.Setup(x => x.GetById(templateId))
                                .Returns(new QuestionnaireBrowseItem() { QuestionnaireId = templateId, FeaturedQuestions = avalibleFeaturedQuestions });

            SampleImportService target = CreateSampleImportService(null, smallTemplateStorage.Object);

            //act

            var importId = target.ImportSampleAsync(templateId, sampleMock.Object);

            //assert
            var status = WhaitForCompletedImportResult(target, importId);

            //assert
            Assert.True(status.IsSuccessed);
            Assert.True(status.IsCompleted);
            Assert.That(status.Header[0], Is.EqualTo("q1"));
            Assert.That(status.Header[1], Is.EqualTo("q2"));
        }

        [Test]
        public void ImportSampleAsync_When_Featured_questions_list_contains_Empty_rows_Then_empty_Rows_are_eliminated()
        {

            //arrange
            var templateId = Guid.NewGuid();
            var avalibleFeaturedQuestions = new FeaturedQuestionItem[4];
            for (int i = 0; i < avalibleFeaturedQuestions.Length; i++)
            {
                avalibleFeaturedQuestions[i] = new FeaturedQuestionItem(Guid.NewGuid(), "title", "q" + i);
            }

            var smallTemplateStorage = new Mock<IReadSideRepositoryWriter<QuestionnaireBrowseItem>>();

            var values = new string[3][];
            values[0] = new string[] {"q1", "q2"};

            values[1] = new string[] {"a1", "a2"};
            values[2] = new string[] {"", ""};



            var sampleMock = new Mock<ISampleRecordsAccessor>();
            sampleMock.Setup(x => x.Records)
                      .Returns(values);
            smallTemplateStorage.Setup(x => x.GetById(templateId))
                                .Returns(new QuestionnaireBrowseItem()
                                    {
                                        QuestionnaireId = templateId,
                                        FeaturedQuestions = avalibleFeaturedQuestions
                                    });

            SampleImportService target = CreateSampleImportService(null, smallTemplateStorage.Object);

            //act

            var importId = target.ImportSampleAsync(templateId, sampleMock.Object);

            //assert
            var status = WhaitForCompletedImportResult(target, importId);

            //assert
            Assert.True(status.IsSuccessed);
            Assert.True(status.IsCompleted);
            Assert.That(status.Values.Length, Is.EqualTo(1));
            Assert.That(status.Values[0], Is.EqualTo(new string[] {"a1", "a2"}));
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
