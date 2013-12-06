using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.DenormalizerStorage;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Implementation;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.SampleImport;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests
{
    public class SampleImportServiceTests
    {
        [SetUp]
        public void Setup()
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
        }

        [Test]
        public void GetImportStatus_When_Import_is_absent_Then_Null_is_returned()
        {
            //arrange
            SampleImportService target = CreateSampleImportService(tempFileImportData: null);

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

            SampleImportService target = CreateSampleImportService(tempFileImportData:new TempFileImportData() { ErrorMassage = "some error", PublicKey = tempFileId });

            //act

            var result = target.GetImportStatus(tempFileId);

            //assert
            Assert.That(result.ErrorMessage, Is.EqualTo("some error"));
        }

        [Test]
        public void ImportSampleAsync_When_Template_is_absent_Then_import_result_contains_error()
        {
            //arrange
            SampleImportService target = CreateSampleImportService(tempFileImportData: null);

            //act

            var importId = target.ImportSampleAsync(Guid.NewGuid(),null);

            var status = this.WaitForCompletedImportResult(target, importId);

            //assert
            Assert.That(status.ErrorMessage, Is.EqualTo("Template Is Absent"));
        }

        [Test]
        public void ImportSampleAsync_When_Template_Featured_question_list_dont_match_source_header_Then_import_result_contains_error()
        {
            //arrange
            var templateId = Guid.NewGuid();

            SampleImportService target = CreateSampleImportService(questionnaireBrowseItem: new QuestionnaireBrowseItem() { QuestionnaireId = templateId, FeaturedQuestions = new FeaturedQuestionItem[0] });

            //act

            var importId = target.ImportSampleAsync(templateId, CreateSampleRecordsAccessor(new string[][] { new string[] { "q" }, new string[] { "v" } }));

            var status = this.WaitForCompletedImportResult(target, importId);

            //assert
            Assert.That(status.ErrorMessage, Is.EqualTo("invalid header Caption"));
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
            var availableFeaturedQuestions = new FeaturedQuestionItem[featuredQuestionsCount];
            var randomizer = new Random();
            for (int i = 0; i < featuredQuestionsCount; i++)
            {
                availableFeaturedQuestions[i] = new FeaturedQuestionItem(Guid.NewGuid(), randomizer.Next().ToString(),
                                                                        randomizer.Next().ToString());
            }

            var values =new string[valuesCount+1][];
            values[0] = availableFeaturedQuestions.Take(presentQuestions).Select(q => q.Caption).ToArray();
            for (int i = 0; i < valuesCount; i++)
            {
                values[i + 1] =
                    availableFeaturedQuestions.Take(presentQuestions).Select(q => randomizer.Next().ToString()).ToArray();
            }

            SampleImportService target = CreateSampleImportService(questionnaireBrowseItem: new QuestionnaireBrowseItem() { QuestionnaireId = templateId, FeaturedQuestions = availableFeaturedQuestions });

            //act

            var importId = target.ImportSampleAsync(templateId, CreateSampleRecordsAccessor(values));

            //assert
            var status = this.WaitForCompletedImportResult(target, importId);

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
            var availableFeaturedQuestions = new FeaturedQuestionItem[4];
            for (int i = 0; i < availableFeaturedQuestions.Length; i++)
            {
                availableFeaturedQuestions[i] = new FeaturedQuestionItem(Guid.NewGuid(), "title", "q" + i);
            }

            var values = new string[3][];
            values[0] =new string[]{"q1","q2"};
            for (int i = 1; i < values.Length; i++)
            {
                values[i] = new string[] { "a1", "a2" };
            }

            SampleImportService target = CreateSampleImportService(questionnaireBrowseItem: new QuestionnaireBrowseItem() { QuestionnaireId = templateId, FeaturedQuestions = availableFeaturedQuestions });

            //act

            var importId = target.ImportSampleAsync(templateId, CreateSampleRecordsAccessor(values));

            //assert
            var status = this.WaitForCompletedImportResult(target, importId);

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
            var availableFeaturedQuestions = new FeaturedQuestionItem[4];
            for (int i = 0; i < availableFeaturedQuestions.Length; i++)
            {
                availableFeaturedQuestions[i] = new FeaturedQuestionItem(Guid.NewGuid(), "title", "q" + i);
            }

            var values = new string[3][];
            values[0] = new string[] {"q1", "q2"};
            values[1] = new string[] {"a1", "a2"};
            values[2] = new string[] {"", ""};

            SampleImportService target = CreateSampleImportService(questionnaireBrowseItem: new QuestionnaireBrowseItem()
            {
                QuestionnaireId = templateId,
                FeaturedQuestions = availableFeaturedQuestions
            });

            //act

            var importId = target.ImportSampleAsync(templateId, CreateSampleRecordsAccessor(values));

            //assert
            var status = this.WaitForCompletedImportResult(target, importId);

            //assert
            Assert.True(status.IsSuccessed);
            Assert.True(status.IsCompleted);
            Assert.That(status.Values.Length, Is.EqualTo(1));
            Assert.That(status.Values[0], Is.EqualTo(new string[] {"a1", "a2"}));
        }


        [Test]
        [TestCase(QuestionType.DateTime,"ivalidDate")]
        [TestCase(QuestionType.Numeric, "ivalidNumber")]
        [TestCase(QuestionType.SingleOption, "ivalidNumber")]
        [TestCase(QuestionType.SingleOption, "10")]
        public void CreateSample_When_Batch_data_contains_invalida_values_Then_import_result_contains_error(QuestionType type, string answer)
        {
            //arrange
            var importId = Guid.NewGuid();
            var templateId = Guid.NewGuid();

            var tempFileImportData =
                new TempFileImportData()
                {
                    PublicKey = importId,
                    IsCompleted = true,
                    TemplateId = templateId,
                    Header = new string[] { "var_name" },
                    Values = new List<string[]> { new string[] { answer } }
                };

            var questionnarieMock =
                Mock.Of<IQuestionnaire>(
                    _ =>
                        _.GetQuestionByStataCaption("var_name") == CreateQuestionByType(type) &&
                        _.GetAnswerOptionsAsValues(It.IsAny<Guid>()) == new decimal[0]);

            SampleImportService target = CreateSampleImportService(tempFileImportData, new QuestionnaireDocument() { PublicKey = templateId }, questionnarieMock);

            //act

            target.CreateSample(importId, Guid.NewGuid(), Guid.NewGuid());

            var status = this.WaitForSampleCreation(target, importId);

            //assert
            Assert.False(status.IsSuccessed);
        }

        private ISampleRecordsAccessor CreateSampleRecordsAccessor(IEnumerable<string[]> values)
        {
            var sampleMock = new Mock<ISampleRecordsAccessor>();
            sampleMock.Setup(x => x.Records).Returns(values);
            return sampleMock.Object;
        }

        private IQuestion CreateQuestionByType(QuestionType type)
        {
            switch (type)
            {
                case QuestionType.DateTime:
                    return new DateTimeQuestion() { QuestionType = type };
                case QuestionType.Numeric:
                    return new NumericQuestion() { QuestionType = type };
                case QuestionType.SingleOption:
                    return new SingleQuestion() { QuestionType = type};
                default:
                    return new TextQuestion() { QuestionType = type };
            }
        }

        private ImportResult WaitForCompletedImportResult(SampleImportService target, Guid importId)
        {
            var status = target.GetImportStatus(importId);

            while (!status.IsCompleted)
            {
                Thread.Sleep(1000);
                status = target.GetImportStatus(importId);
            }
            return status;
        }

        private SampleCreationStatus WaitForSampleCreation(SampleImportService target, Guid importId)
        {
            var status = target.GetSampleCreationStatus(importId);

            while (!status.IsCompleted)
            {
                Thread.Sleep(1000);
                status = target.GetSampleCreationStatus(importId);
            }
            return status;
        }

        private SampleImportService CreateSampleImportService(
            TempFileImportData tempFileImportData = null,
            QuestionnaireDocument questionnaireDocumentVersion = null,
            IQuestionnaire questionnaire = null,
            SampleCreationStatus sampleCreationStatus = null,
            QuestionnaireBrowseItem questionnaireBrowseItem = null)
        {
            var questionnaireFactoryMock = new Mock<IQuestionnaireFactory>();
            questionnaireFactoryMock.Setup(x => x.CreateTemporaryInstance(It.IsAny<QuestionnaireDocument>()))
                .Returns(questionnaire);
            var tempStorage = new InMemoryTemporaryDataRepositoryAccessor<TempFileImportData>();
            if (tempFileImportData != null)
                tempStorage.Store(tempFileImportData, tempFileImportData.PublicKey.ToString());
            var bigTemplateRepositoryMock = new Mock<IReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>();
            bigTemplateRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>()))
                .Returns(new QuestionnaireDocumentVersioned() { Questionnaire = questionnaireDocumentVersion });

            var smallTemplateRepository = new InMemoryTemporaryDataRepositoryAccessor<SampleCreationStatus>();
            if (sampleCreationStatus != null)
                smallTemplateRepository.Store(sampleCreationStatus, sampleCreationStatus.Id.ToString());

            var questionnaireBrowseItemStore = new InMemoryReadSideRepositoryAccessor<QuestionnaireBrowseItem>();
            if (questionnaireBrowseItem != null)
                questionnaireBrowseItemStore.Store(questionnaireBrowseItem, questionnaireBrowseItem.QuestionnaireId);
            return
                new SampleImportService(
                    bigTemplateRepositoryMock.Object,
                    questionnaireBrowseItemStore,
                    tempStorage,
                    smallTemplateRepository,
                    questionnaireFactoryMock.Object);
        }
    }
}
