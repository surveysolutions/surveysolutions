using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Tests.Abc;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    [TestFixture]
    [TestOf(typeof(InterviewsExporter))]
    internal class InterviewsExporterTests
    {
        [SetUp]
        public void SetUp()
        {
            dataInCsvFile = new List<Create.CsvData>();

            csvWriter = Create.CsvWriter(dataInCsvFile);

            errorsExporter = new Mock<IInterviewErrorsExporter>();
            errorsExporter.SetupIgnoreArgs(x => x.Export(null, null, null, null, null))
                .Returns(() => new List<string[]>());
        }

        [Test]
        public async Task It_should_export_service_column_with_interview_key()
        {
            //arrange
            Guid interviewId = Id.g1;
            var interviewKey = "11-11-11-11";

            var questionnaire = Create.QuestionnaireDocument(
                variableName: "MyQuestionnaire"
            );

            var questionnaireExportStructure = Create.QuestionnaireExportStructure(questionnaire);
            var interviewIdsToExport = new List<InterviewToExport>
            {
                new InterviewToExport(interviewId, interviewKey, 1, InterviewStatus.Completed)
            };

            string[][] answers = { new string[1] };
            answers[0][0] = "1";

            var systemVariables = ServiceColumns.SystemVariables.Values;
            var systemVariableValues = new string[systemVariables.Count];
            systemVariableValues[ServiceColumns.SystemVariables[ServiceVariableType.InterviewRandom].Index] = "5";

            var interviewFactory = new Mock<IInterviewFactory>();
            interviewFactory.SetupIgnoreArgs(x => x.GetInterviewDataLevels(null, null))
                .Returns(new Dictionary<string, InterviewLevel>());

            var exporter = Create.InterviewsExporter(csvWriter, interviewFactory.Object);

            //act
            await exporter.ExportAsync(Create.Tenant(), questionnaireExportStructure, questionnaire, interviewIdsToExport, "", new Progress<int>(), CancellationToken.None);

            //assert
            Assert.That(dataInCsvFile[0].File, Is.EqualTo("MyQuestionnaire.tab"));

            Assert.That(dataInCsvFile[0].Data[0], Has.Length.EqualTo(dataInCsvFile[1].Data[0].Length),
                "Length of header columns should be equal to data columns length");

            Assert.That(dataInCsvFile[0].Data[0][2], Is.EqualTo(ServiceColumns.Key));
            Assert.That(dataInCsvFile[1].Data[0][2], Is.EqualTo(interviewKey));

            Assert.That(dataInCsvFile[0].Data[0][3], Is.EqualTo(ServiceColumns.HasAnyError));
            Assert.That(dataInCsvFile[1].Data[0][3], Is.EqualTo("1"));

            Assert.That(dataInCsvFile[0].Data[0][4], Is.EqualTo(ServiceColumns.InterviewStatus));
            Assert.That(dataInCsvFile[1].Data[0][4], Is.EqualTo(InterviewStatus.Completed.ToString()));
        }

        private List<Create.CsvData> dataInCsvFile;
        private ICsvWriter csvWriter;
        private Mock<IInterviewErrorsExporter> errorsExporter;
    }

    [TestOf(typeof(InterviewsExporter))]
    internal class ExportViewFactoryTests
    {
        [Test]
        public void when_creating_interview_export_view_by_interview_with_numeric_nested_roster_should_return_roster_instance_codes_by_numeric_roster_started_from_1()
        {
            //arrange
            var questionInsideRosterGroupId = Guid.Parse("12222222222222222222222222222222");
            var rosterId = Guid.Parse("11111111111111111111111111111111");
            var nestedRosterId = Guid.Parse("13333333333333333333333333333333");
            var rosterSizeId = Guid.Parse("44444444444444444444444444444444");
            var nestedRosterSizeId = Guid.Parse("55555555555555555555555555555555");

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                variable: null,
                children: new IQuestionnaireEntity[]
                {
                    Create.NumericIntegerQuestion(rosterSizeId),
                    Create.Roster(rosterId: rosterId, rosterSizeQuestionId: rosterSizeId,
                        children: new IQuestionnaireEntity[]
                        {
                            Create.NumericIntegerQuestion(nestedRosterSizeId),
                            Create.Roster(rosterId: nestedRosterId,
                                rosterSizeQuestionId: nestedRosterSizeId,
                                children: new IQuestionnaireEntity[]
                                {
                                    Create.TextQuestion(questionInsideRosterGroupId)
                                })
                        })
                });

            var exportStructure = Create.QuestionnaireExportStructure(questionnaireDocument);
            var exporter = Create.InterviewsExporter();

            //act
            var result = exporter.CreateInterviewDataExportView(exportStructure,
                CreateInterviewDataWith2PropagatedLevels(rosterSizeId, nestedRosterSizeId, questionInsideRosterGroupId), questionnaireDocument);

            //assert
            Assert.That(result.Levels[1].Records[0].RecordId, Is.EqualTo("1"));
            Assert.That(result.Levels[1].Records[1].RecordId, Is.EqualTo("2"));

            Assert.That(result.Levels[2].Records[0].RecordId, Is.EqualTo("1"));
            Assert.That(result.Levels[2].Records[0].ParentRecordIds[0], Is.EqualTo("1"));

            Assert.That(result.Levels[2].Records[1].RecordId, Is.EqualTo("2"));
            Assert.That(result.Levels[2].Records[1].ParentRecordIds[0], Is.EqualTo("1"));

            Assert.That(result.Levels[2].Records[2].RecordId, Is.EqualTo("1"));
            Assert.That(result.Levels[2].Records[2].ParentRecordIds[0], Is.EqualTo("2"));

            Assert.That(result.Levels[2].Records[3].RecordId, Is.EqualTo("2"));
            Assert.That(result.Levels[2].Records[3].ParentRecordIds[0], Is.EqualTo("2"));
        }

        [Test]
        public void when_creating_interview_export_view_by_interview_with_2_rosters_in_first_level_by_diff_roster_size_and_one_of_them_numeric_should_return_roster_instance_code_by_numeric_roster_started_from_1()
        {
            //arrange
            var numericRosterId = Id.g1;
            var listRosterId = Id.g3;
            var numericRosterSizeId = Id.g4;
            var listRosterSizeId = Id.g5;
            var questionInListRosterId = Id.g6;
            var questionInNumericRosterId = Id.g7;

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                children: new IQuestionnaireEntity[]
                {
                    Create.TextListQuestion(listRosterSizeId),
                    Create.Roster(listRosterId, rosterSizeQuestionId: listRosterSizeId,
                        children: new IQuestionnaireEntity[]
                        {
                            Create.NumericIntegerQuestion(questionInListRosterId)
                        }),
                    Create.NumericIntegerQuestion(numericRosterSizeId),
                    Create.Roster(numericRosterId, rosterSizeQuestionId: numericRosterSizeId,
                        children: new IQuestionnaireEntity[]
                        {
                            Create.NumericIntegerQuestion(questionInNumericRosterId)
                        })
                });

            var questionnaireExportStructure = Create.QuestionnaireExportStructure(questionnaireDocument);

            var interviewEntities = new List<InterviewEntity>
            {
                Create.InterviewEntity(asInt:2, entityType: EntityType.Question, identity: Create.Identity(numericRosterSizeId)),
                Create.InterviewEntity(asInt: 22, entityType: EntityType.Question, identity: Create.Identity(questionInNumericRosterId, 0))
            };

            var interviewFactory = Create.InterviewFactory();

            var interviewData = new InterviewData
            {
                Levels = interviewFactory.GetInterviewDataLevels(questionnaireDocument, interviewEntities),
                InterviewId = Id.gA
            };

            var exporter = Create.InterviewsExporter();
            //act
            var result = exporter.CreateInterviewDataExportView(questionnaireExportStructure, interviewData, questionnaireDocument);

            //assert
            Assert.That(result.Levels[2].Records[0].RecordId, Is.EqualTo("1"));
        }

        private static InterviewData CreateInterviewDataWith2PropagatedLevels(Guid rosterId, Guid nestedRosterId, Guid questionInsideRosterGroupId)
        {
            InterviewData interview = CreateInterviewData();
            for (int i = 0; i < 2; i++)
            {
                var vector = new int[1] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { rosterId }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);
                for (int j = 0; j < 2; j++)
                {
                    var nestedVector = new int[] { i, j };
                    var nestedLevel = new InterviewLevel(new ValueVector<Guid> { rosterId, nestedRosterId }, null, nestedVector);
                    interview.Levels.Add(string.Join(",", nestedVector), nestedLevel);

                    if (!nestedLevel.QuestionsSearchCache.ContainsKey(questionInsideRosterGroupId))
                        nestedLevel.QuestionsSearchCache.Add(questionInsideRosterGroupId, new InterviewEntity
                        {
                            Identity = Create.Identity(questionInsideRosterGroupId)
                        });

                    var question = nestedLevel.QuestionsSearchCache[questionInsideRosterGroupId];

                    question.AsString = "some answer";
                }

            }
            return interview;
        }

        private static InterviewData CreateInterviewData()
        {
            var interviewData = new InterviewData() { InterviewId = Guid.NewGuid() };
            interviewData.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, new int[0]));
            return interviewData;
        }
    }

}
