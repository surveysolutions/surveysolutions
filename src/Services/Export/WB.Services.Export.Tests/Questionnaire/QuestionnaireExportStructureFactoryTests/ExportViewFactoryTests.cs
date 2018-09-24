using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Tests.Abc;

namespace WB.Services.Export.Tests.Questionnaire.QuestionnaireExportStructureFactoryTests
{
    [TestOf(typeof(QuestionnaireExportStructureFactory))]
    internal class ExportViewFactoryTests : ExportViewFactoryTestsContext
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

            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                chapterChildren: new IQuestionnaireEntity[]
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

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.SetupIgnoreArgs(x => x.GetQuestionnaireAsync(null, null))
                .ReturnsAsync(questionnaireDocument);

            var exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);

            //act
            var result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaireDocument.PublicKey, 1)),
                CreateInterviewDataWith2PropagatedLevels(rosterSizeId, nestedRosterSizeId, questionInsideRosterGroupId), questionnaire);

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
            var listRosterSizeId =     Id.g5;
            var questionInListRosterId = Id.g6;
            var questionInNumericRosterId = Id.g7;

            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(listRosterSizeId),
                Create.Entity.ListRoster(listRosterId, rosterSizeQuestionId: listRosterSizeId,
                    children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(questionInListRosterId)
                    }),
                Create.Entity.NumericIntegerQuestion(numericRosterSizeId),
                Create.Entity.NumericRoster(numericRosterId, rosterSizeQuestionId: numericRosterSizeId,
                    children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(questionInNumericRosterId)
                    }));

            var questionnaireMockStorage = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocument);

            var exportViewFactory = CreateExportViewFactory(questionnaireMockStorage);

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(questionnaireDocument.PublicKey, 1);
            var questionnaire = questionnaireMockStorage.GetQuestionnaire(questionnaireIdentity, null);
            var questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireIdentity);

            var interviewEntities = new List<InterviewEntity>
            {
                Create.Entity.InterviewEntity(asInt:2, entityType: EntityType.Question, identity: Create.Identity(numericRosterSizeId)),
                Create.Entity.InterviewEntity(asInt: 22, entityType: EntityType.Question, identity: Create.Identity(questionInNumericRosterId, 0))
            };

            var interviewFactory = Create.Service.InterviewFactory();

            var interviewData = new InterviewData
            {
                Levels = interviewFactory.GetInterviewDataLevels(questionnaire, interviewEntities),
                InterviewId =Id.gA
            };

            //act
            var result = exportViewFactory.CreateInterviewDataExportView(questionnaireExportStructure, interviewData, questionnaire);

            //assert
            Assert.That(result.Levels[2].Records[0].RecordId, Is.EqualTo("1"));
        }

        internal class Ids
        {
        }

        private static InterviewData CreateInterviewDataWith2PropagatedLevels(Guid rosterId, Guid nestedRosterId, Guid questionInsideRosterGroupId)
        {
            InterviewData interview = CreateInterviewData();
            for (int i = 0; i < 2; i++)
            {
                var vector = new decimal[1] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { rosterId }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);
                for (int j = 0; j < 2; j++)
                {
                    var nestedVector = new decimal[] { i, j };
                    var nestedLevel = new InterviewLevel(new ValueVector<Guid> { rosterId, nestedRosterId }, null, nestedVector);
                    interview.Levels.Add(string.Join(",", nestedVector), nestedLevel);

                    if (!nestedLevel.QuestionsSearchCache.ContainsKey(questionInsideRosterGroupId))
                        nestedLevel.QuestionsSearchCache.Add(questionInsideRosterGroupId, new InterviewQuestion(questionInsideRosterGroupId));

                    var question = nestedLevel.QuestionsSearchCache[questionInsideRosterGroupId];

                    question.Answer = "some answer";
                }

            }
            return interview;
        }
    }
}
