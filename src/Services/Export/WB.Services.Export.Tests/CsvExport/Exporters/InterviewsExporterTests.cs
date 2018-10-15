using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;

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
            interviewFactory.SetupIgnoreArgs(x => x.GetInterviewEntities(null, null))
                .Returns(Task.FromResult(new List<InterviewEntity>()));

            var exporter = Create.InterviewsExporter(csvWriter, interviewFactory.Object);

            //act
            await exporter.ExportAsync(Create.Tenant(), questionnaireExportStructure, questionnaire, interviewIdsToExport, "", 
                new Progress<int>(), CancellationToken.None);

            //assert
            Assert.That(dataInCsvFile, Has.Count.EqualTo(2));

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

        [TestCase("it is string", VariableType.String, "it is string")]
        [TestCase(789L, VariableType.LongInteger, "789")]
        [TestCase(789.56, VariableType.Double, "789.56")]
        [TestCase(true, VariableType.Boolean, "1")]
        public void when_creating_interview_export_view_by_interview_with_1_variable(object variable, VariableType variableType, string exportResult)
        {
            var variableId = Id.g1;
            var interviewData = CreateInterviewData(variableId, variable);

            var questionnaireDocument =
                Create.QuestionnaireDocument(children: Create.Variable(id: variableId, type: variableType));

            var QuestionnaireExportStructureFactory = Create.InterviewsExporter();

            var result = QuestionnaireExportStructureFactory.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument),
                interviewData, questionnaireDocument);

            result.Levels[0].Records[0].Answers.First().Should().BeEquivalentTo(new[] { exportResult });
        }

        [Test]
        public void when_creating_interview_export_view_by_interview_with_1_answered_datetime_question()
        {
            var dateTimeQuestionId = Guid.Parse("10000000000000000000000000000000");
            DateTime date = new DateTime(1984, 4, 18, 18, 4, 19);

            var interviewData =
                CreateInterviewData(Create.InterviewEntity(identity: Create.Identity(dateTimeQuestionId), asDateTime: date));

            var questionnaireDocument =
                Create.QuestionnaireDocument(children: Create.DateTimeQuestion(questionId: dateTimeQuestionId, variable: "dateTime"));

            var service = Create.InterviewsExporter();
            var result = service.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument), interviewData, questionnaireDocument);

            result.Levels[0].Records[0].Answers.First().Should().BeEquivalentTo(new[] { "1984-04-18"  });
        }

        [Test]
        [SetCulture("ru-RU")]
        [SetUICulture("ru-Ru")]
        public void when_creating_interview_export_view_by_interview_with_1_answered_double_question_in_russian_culture__should_create_record__with_one_datetime_question_which_contains_composite_answer()
        {
            var dateTimeQuestionId = Guid.Parse("10000000000000000000000000000000");

            var value = 5.55;
            var interviewData =
                CreateInterviewData(Create.InterviewEntity(identity: Create.Identity(dateTimeQuestionId),
                    asDouble: value));

            var questionnaireDocument =
                Create.QuestionnaireDocument(children: Create.NumericRealQuestion(id: dateTimeQuestionId, variable: "real"));

            var questionnaireExportStructure = Create.QuestionnaireExportStructure(questionnaireDocument);
            var service = Create.InterviewsExporter();

            var    result = service.CreateInterviewDataExportView(
                questionnaireExportStructure,
                    interviewData, questionnaireDocument);

            result.Levels[0].Records[0].Answers.First().Should()
                .BeEquivalentTo(new[] {value.ToString(CultureInfo.InvariantCulture)});
        }

        [Test]
        public void when_creating_interview_export_view_by_interview_with_1_answerd_questions_and_1_unananswered__should_answered_question_be_not_empty()
        {
            var answeredQuestionId = Guid.Parse("10000000000000000000000000000000");
            var unansweredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var variableNameAndQuestionId = new Dictionary<string, Guid>
            {
                {"q1", answeredQuestionId},
                {"q2", unansweredQuestionId}
            };
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                Create.TextQuestion(id: answeredQuestionId, variable: "q1"), 
                Create.TextQuestion(id: unansweredQuestionId, variable: "q2"));
            
            var interviewData =
                CreateInterviewData(Create.InterviewEntity(identity: Create.Identity(answeredQuestionId), asString: "answer"));

            var expoter = Create.InterviewsExporter();

            // act 
            var result = expoter.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument), interviewData, questionnaireDocument);

            // Assert
            result.Levels[0].Records.Length.Should().Be(1);
            result.Levels[0].Records[0].GetPlainAnswers().Count().Should().Be(2);
            result.Levels[0].Records[0].ParentRecordIds.Should().BeEmpty();
            result.Levels[0].Records[0].GetPlainAnswers().First().Length.Should().NotBe(0);
        }

        [Test]
        public void when_creating_interview_export_view_by_interview_with_1_answerd_gps_question__should_create_record__with_one_gps_question_which_contains_composite_answer()
        {
            var gpsQuestionId = Guid.Parse("10000000000000000000000000000000");

            var geoPosition = new GeoPosition(1, 2, 3, 4, new DateTimeOffset());
            var interviewData = CreateInterviewData(Create.InterviewEntity(identity: Create.Identity(gpsQuestionId),
                    asGps: geoPosition));

            var questionnaireDocument =
                Create.QuestionnaireDocumentWithOneChapter(Create.GpsCoordinateQuestion(questionId: gpsQuestionId, variable: "gps"));

            var exporter = Create.InterviewsExporter();
            // act
            var result = exporter.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument),
                interviewData, questionnaireDocument);
            // assert
            result.Levels[0].Records[0].GetPlainAnswers().First()
                .Should().BeEquivalentTo(new[] { "1", "2", "3", "4", geoPosition.Timestamp.DateTime.ToString(ExportFormatSettings.ExportDateTimeFormat) });
        }

        [Test]
        public void when_creating_interview_export_view_by_interview_with_1_answerd_text_question_which_contains_unreadable_symbol__should_create_record_with_one_text_question_which_answered_and_doesnt_contain_the_unreadable_symbol()
        {
            string text = "231 Pietermaritz St\u263APietermaritzburg\u263A3201";
            var textQuestionId = Guid.Parse("10000000000000000000000000000000");

            var interviewData =
                CreateInterviewData(Create.InterviewEntity(identity: Create.Identity(textQuestionId), asString: text));

            var questionnaireDocument =
                Create.QuestionnaireDocument(children: Create.DateTimeQuestion(questionId: textQuestionId, variable: "txt"));

            var exporter = Create.InterviewsExporter();

            // act
            var result = exporter.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument), interviewData, questionnaireDocument);

            // assert
            result.Levels[0].Records[0].GetPlainAnswers().First().Length.Should().Be(1);
            result.Levels[0].Records[0].GetPlainAnswers().First().Should().BeEquivalentTo(new[] { "231 Pietermaritz StPietermaritzburg3201" });
        }

        [Test]
        public void when_creating_interview_export_view_by_interview_with_1_answered_text_question__should_create_record_with_one_text_question_which_answered_and_contains_all_symbols()
        {
            string text = "231 Pietermaritz St\n\rPietermaritzburg\n\r3201";
            var textQuestionId = Guid.Parse("10000000000000000000000000000000");

            var interviewData =
                CreateInterviewData(Create.InterviewEntity(identity: Create.Identity(textQuestionId),
                    asString: text));

            var questionnaireDocument =
                Create.QuestionnaireDocument(children: Create.TextQuestion(id: textQuestionId, variable: "txt"));

            var exporter = Create.InterviewsExporter();

            // act
            var result = exporter.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument), interviewData, questionnaireDocument);

            // Assert
            result.Levels[0].Records[0].GetPlainAnswers().First().Length.Should().Be(1);
            result.Levels[0].Records[0].GetPlainAnswers().First().Should().BeEquivalentTo(new[] { text });
        }

        [Test]
        public void when_creating_interview_export_view_by_interview_with_1_answered_timestamp_question__should_create_record__with_one_timestamp_question_which_contains_composite_answer()
        {
            DateTime date = new DateTime(1984, 4, 18, 18, 4, 19);
            var dateTimeQuestionId = Guid.Parse("10000000000000000000000000000000");

            var interviewData =
                CreateInterviewData(Create.InterviewEntity(identity: Create.Identity(dateTimeQuestionId),
                    asDateTime: date));

            var questionnaireDocument =
                Create.QuestionnaireDocument(children: Create.DateTimeQuestion(questionId: dateTimeQuestionId, variable: "dateTime", isTimestamp: true));

            var exporter = Create.InterviewsExporter();

            // act
            var result = exporter.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument), interviewData, questionnaireDocument);

            // assert
            result.Levels[0].Records[0].GetPlainAnswers().First().Should().BeEquivalentTo(new[] { "1984-04-18T18:04:19"  });
        }

        [Test]
        public void when_creating_interview_export_view_by_interview_with_linked_multi_question_on_second_level_referenced_on_third__should_linked_question_have_first_answer_be_equal_to_0()
        {
            var linkedQuestionSourceId = Guid.Parse("12222222222222222222222222222222");
            var rosterId = Guid.Parse("13333333333333333333333333333333");
            var nestedRosterId = Guid.Parse("23333333333333333333333333333333");

            var linkedQuestionId = Guid.Parse("10000000000000000000000000000000");

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                Create.Roster(rosterId: rosterId, fixedTitles: new FixedRosterTitle[] { new FixedRosterTitle(1, "t1"), new FixedRosterTitle(2, "t2")},
                    children: new IQuestionnaireEntity[]
                    {
                        Create.MultyOptionsQuestion(id: linkedQuestionId, linkedToQuestionId:linkedQuestionSourceId),
                        Create.Roster(rosterId: nestedRosterId, fixedTitles: new FixedRosterTitle[] { new FixedRosterTitle(1, "n1"), new FixedRosterTitle(2, "n2")},
                            children: new IQuestionnaireEntity[]
                            {
                                Create.NumericIntegerQuestion(id: linkedQuestionSourceId, variable: "q1")
                            })
                    }));

            var interview = CreateInterviewData();
            var rosterLevel = new InterviewLevel(new ValueVector<Guid> { rosterId }, null, new int[] { 0 });
            interview.Levels.Add("0", rosterLevel);

            if (!rosterLevel.QuestionsSearchCache.ContainsKey(linkedQuestionId))
                rosterLevel.QuestionsSearchCache.Add(linkedQuestionId,
                    Create.InterviewEntity(identity: Create.Identity(linkedQuestionId), asIntArray: new int[]{0,0}));
            var exporter = Create.InterviewsExporter();

            // act
            var result = exporter.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument), interview, questionnaireDocument);

            // assert
            GetLevel(result, new[] { rosterId }).Records[0].GetPlainAnswers().First().Length.Should().Be(2);
            GetLevel(result, new[] {rosterId}).Records[0].GetPlainAnswers().First().First().Should().Be("0");
        }

        public static InterviewDataExportLevelView GetLevel(InterviewDataExportView interviewDataExportView, Guid[] levelVector)
        {
            return interviewDataExportView.Levels.FirstOrDefault(l => l.LevelVector.SequenceEqual(levelVector));
        }

        protected InterviewData CreateInterviewData(params InterviewEntity[] topLevelQuestions)
        {
            var interviewData = new InterviewData { InterviewId = Guid.NewGuid() };
            interviewData.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, new int[0]));
            foreach (var interviewQuestion in topLevelQuestions)
            {
                interviewData.Levels["#"].QuestionsSearchCache.Add(interviewQuestion.Identity.Id, interviewQuestion);
            }
            return interviewData;
        }

        protected InterviewData CreateInterviewData(Guid variableId, object topLevelVariable)
        {
            var interviewData = new InterviewData { InterviewId = Guid.NewGuid() };
            interviewData.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, new int[0]));
            interviewData.Levels["#"].Variables.Add(variableId, topLevelVariable);
            return interviewData;
        }

        protected static InterviewData CreateInterviewDataWith2PropagatedLevels(Guid rosterId, Guid nestedRosterId, Guid questionInsideRosterGroupId)
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

        protected static InterviewData CreateInterviewData()
        {
            var interviewData = new InterviewData() { InterviewId = Guid.NewGuid() };
            interviewData.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, new int[0]));
            return interviewData;
        }
    }

}
