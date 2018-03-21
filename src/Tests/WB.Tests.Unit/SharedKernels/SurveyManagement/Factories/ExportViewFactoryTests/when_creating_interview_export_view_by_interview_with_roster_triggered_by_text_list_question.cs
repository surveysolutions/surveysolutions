using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_interview_export_view_by_interview_with_roster_triggered_by_text_list_question : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionInsideRosterGroupId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("13333333333333333333333333333333");

            levelCount = 2;

            rosterSizeQuestionId = Guid.Parse("10000000000000000000000000000000");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new TextListQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.TextList,
                    MaxAnswerCount = maxAnswerCount
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion()
                        {
                            PublicKey = questionInsideRosterGroupId,
                            QuestionType = QuestionType.Numeric,
                            StataExportCaption = "q1"
                        }
                    }.ToReadOnlyCollection()
                });

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(Create.Entity.PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
             result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaire.PublicKey, 1)),
                CreateInterviewDataWith2PropagatedLevels());

        [NUnit.Framework.Test] public void should_records_count_equals_4 () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records.Length.Should().Be(2);

        [NUnit.Framework.Test] public void should_first_record_id_equals_0 () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].RecordId.Should().Be("0");

        [NUnit.Framework.Test] public void should_first_record_has_one_question () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_first_record_has_question_with_one_answer () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().First().Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_first_record_has_question_with_answer_equal_to_some_answer () =>
         GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().First().First().Should().Be(someAnswer);

        [NUnit.Framework.Test] public void should_second_record_id_equals_1 () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].RecordId.Should().Be("1");

        [NUnit.Framework.Test] public void should_second_record_has_one_question () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_second_record_has_question_with_one_answer () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().First().Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_second_record_has_question_with_answer_equal_to_some_answer () =>
         GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().First().First().Should().Be(someAnswer);

        [NUnit.Framework.Test] public void should_have_one_question_on_top_level () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_have_five_columns_for_question_on_top_level () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().Length.Should().Be(5);

        [NUnit.Framework.Test] public void should_have_first_column_with_value_a1_for_question_on_top_level () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().First().Should().Be("a1");

        [NUnit.Framework.Test] public void should_have_second_column_with_value_a1_for_question_on_top_level () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().Second().Should().Be("a2");

        [NUnit.Framework.Test] public void should_have_all_other_coulmns_with_missing_values_for_question_on_top_level () =>
           GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().Skip(2).Any(a => a != ExportFormatSettings.MissingStringQuestionValue).Should().BeFalse();

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            if (!interview.Levels["#"].QuestionsSearchCache.ContainsKey(rosterSizeQuestionId))
                interview.Levels["#"].QuestionsSearchCache.Add(rosterSizeQuestionId, new InterviewQuestion(rosterSizeQuestionId));

            var textListQuestion = interview.Levels["#"].QuestionsSearchCache[rosterSizeQuestionId];
            textListQuestion.Answer = new[]{new InterviewTextListAnswer(1, "a1"), new InterviewTextListAnswer(2, "a2")};

            for (int i = 0; i < levelCount; i++)
            {
                var vector = new decimal[1] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { rosterSizeQuestionId }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);

                if (!newLevel.QuestionsSearchCache.ContainsKey(questionInsideRosterGroupId))
                    newLevel.QuestionsSearchCache.Add(questionInsideRosterGroupId, new InterviewQuestion(questionInsideRosterGroupId));

                var question = newLevel.QuestionsSearchCache[questionInsideRosterGroupId];

                question.Answer = new InterviewTextListAnswers(new[] { new Tuple<decimal, string>(1, someAnswer) });
            }

            return interview;
        }

        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid rosterSizeQuestionId;
        private static Guid questionInsideRosterGroupId;
        private static int levelCount;
        private static QuestionnaireDocument questionnaire;
        private static string someAnswer = "some answer";
        private static int maxAnswerCount = 5;
        private static ExportViewFactory exportViewFactory;
    }
}
