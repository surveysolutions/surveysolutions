using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    internal class when_creating_interview_export_view_by_interview_with_nested_roster_triggered_by_text_list_question : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionInsideRosterGroupId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("13333333333333333333333333333333");

            nestedRosterId = Guid.Parse("13333333333333333333333333333334");

            rosterSizeQuestionId = Guid.Parse("10000000000000000000000000000000");

            questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                new Group()
                {
                    VariableName = "r1",
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    FixedRosterTitles = new FixedRosterTitle[]
                    {
                        new FixedRosterTitle(1, "1"),
                        new FixedRosterTitle(2, "2"), 
                    },

                    Children = new List<IQuestionnaireEntity>
                    {
                        new TextListQuestion()
                        {
                            PublicKey = rosterSizeQuestionId,
                            QuestionType = QuestionType.TextList,
                            VariableName = "list",
                            MaxAnswerCount = maxAnswerCount
                        },
                        new Group()
                        {
                            VariableName = "nestedR",
                            PublicKey = nestedRosterId,
                            IsRoster = true,
                            RosterSizeQuestionId = rosterSizeQuestionId,
                            Children = new List<IQuestionnaireEntity>
                            {
                                new NumericQuestion()
                                {
                                    PublicKey = questionInsideRosterGroupId,
                                    QuestionType = QuestionType.Numeric,
                                    VariableName = "q1"
                                }
                            }
                        }
                    }
                });


            exporter = Create.InterviewsExporter();

            BecauseOf();
        }

        public void BecauseOf() =>
             result = exporter.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument),
                CreateInterviewDataWith3PropagatedLevels(), questionnaireDocument);
        

        [NUnit.Framework.Test] public void should_have_five_columns_for_question_on_top_level () =>
            GetLevel(result, new Guid[] { rosterId, rosterSizeQuestionId }).Records[0].ReferenceValues[0].Should().Be(someAnswer);

        private static InterviewData CreateInterviewDataWith3PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();

            var valueVector1 = new ValueVector<Guid> {rosterId};
            var vector1 = new[] { 1 };
            var vector2 = new[] { 2 };

            var newLevel1 = new InterviewLevel(valueVector1, null, vector1);

            interview.Levels.Add(InterviewLevel.GetLevelKeyName(valueVector1, vector1), newLevel1);

            if (!newLevel1.QuestionsSearchCache.ContainsKey(rosterSizeQuestionId))
                newLevel1.QuestionsSearchCache.Add(rosterSizeQuestionId,
                    Create.InterviewEntity(identity: Create.Identity(rosterSizeQuestionId), asList: new[] { new InterviewTextListAnswer(1, someAnswer) })
                );

            var newLevel2 = new InterviewLevel(valueVector1, null, vector2);
            interview.Levels.Add(InterviewLevel.GetLevelKeyName(valueVector1, vector2), newLevel2);

            var valueVector11 = new ValueVector<Guid> { rosterId , rosterSizeQuestionId };
            var vector11 = new[] { 1, 1 };
            var newLevel11 = new InterviewLevel(valueVector11, null, vector11);

            if (!newLevel11.QuestionsSearchCache.ContainsKey(questionInsideRosterGroupId))
                newLevel11.QuestionsSearchCache.Add(questionInsideRosterGroupId,
                    Create.InterviewEntity(identity: Create.Identity(questionInsideRosterGroupId), asInt:1 ));

            interview.Levels.Add(InterviewLevel.GetLevelKeyName(valueVector11, vector11), newLevel11);

            return interview;
        }

        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid nestedRosterId;
        private static Guid rosterSizeQuestionId;
        private static Guid questionInsideRosterGroupId;
        private static int levelCount;
        private static QuestionnaireDocument questionnaireDocument;
        private static string someAnswer = "some answer";
        private static int maxAnswerCount = 5;
        private static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        private InterviewsExporter exporter;
    }
}
