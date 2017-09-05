﻿using System;
using System.Linq;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_interview_export_view_by_interview_with_roster_with_2_rows_with_nested_groups : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionInsideNestedGroupId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("13333333333333333333333333333333");

            levelCount = 2;

            rosterSizeQuestionId = Guid.Parse("10000000000000000000000000000000");

            var nestedGroupId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group("title")
                        {
                            PublicKey = nestedGroupId,
                            Children = new List<IComposite>
                            {
                                new NumericQuestion()
                                {
                                    PublicKey = questionInsideNestedGroupId,
                                    QuestionType = QuestionType.Numeric,
                                    StataExportCaption = "q1"
                                }
                            }.ToReadOnlyCollection()
                        }
                    }.ToReadOnlyCollection()
                });

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(Create.Entity.PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
        };

        Because of = () =>
               result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaire.PublicKey, 1)),
                CreateInterviewDataWith2PropagatedLevels());

        It should_records_count_equals_4 = () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records.Length.ShouldEqual(2);

        It should_first_record_id_equals_0 = () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].RecordId.ShouldEqual("0");

        It should_first_record_has_one_question = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().Count().ShouldEqual(1);

        It should_first_record_has_question_with_oneanswer = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().First().Length.ShouldEqual(1);

        It should_first_record_has_question_with_answer_equal_to_some_answer = () =>
         GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().First().First().ShouldEqual(someAnswer);

        It should_second_record_id_equals_1 = () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].RecordId.ShouldEqual("1");

        It should_second_record_has_one_question = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().Count().ShouldEqual(1);

        It should_second_record_has_question_with_one_answer = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().First().Length.ShouldEqual(1);

        It should_second_record_has_question_with_answer_equal_to_some_answer = () =>
         GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().First().First().ShouldEqual(someAnswer);

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            for (int i = 0; i < levelCount; i++)
            {
                var vector = new decimal[1] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { rosterSizeQuestionId }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);
                
                if (!newLevel.QuestionsSearchCache.ContainsKey(questionInsideNestedGroupId))
                    newLevel.QuestionsSearchCache.Add(questionInsideNestedGroupId, new InterviewQuestion(questionInsideNestedGroupId));

                var question = newLevel.QuestionsSearchCache[questionInsideNestedGroupId];

                question.Answer = someAnswer;
            }

            return interview;
        }

        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid rosterSizeQuestionId;
        private static Guid questionInsideNestedGroupId;
        private static int levelCount;
        private static QuestionnaireDocument questionnaire;
        private static string someAnswer = "some answer";
        private static ExportViewFactory exportViewFactory;
    }
}
