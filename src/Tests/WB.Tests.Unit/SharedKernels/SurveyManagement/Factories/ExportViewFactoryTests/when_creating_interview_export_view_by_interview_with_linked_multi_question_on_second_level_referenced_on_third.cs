using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_interview_export_view_by_interview_with_linked_multi_question_on_second_level_referenced_on_third : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionSourceId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("13333333333333333333333333333333");
            var nestedRosterId = Guid.Parse("23333333333333333333333333333333");

            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(rosterId: rosterId, fixedTitles: new[] {"t1", "t2"},
                    children: new IComposite[]
                    {
                        new MultyOptionsQuestion()
                        {
                            PublicKey = linkedQuestionId,
                            QuestionType = QuestionType.MultyOption,
                            LinkedToQuestionId = linkedQuestionSourceId
                        },
                        Create.Entity.FixedRoster(rosterId: nestedRosterId, fixedTitles: new[] {"n1", "n2"},
                            children: new IComposite[]
                            {
                                new NumericQuestion()
                                {
                                    PublicKey = linkedQuestionSourceId,
                                    QuestionType = QuestionType.Numeric,
                                    StataExportCaption = "q1"
                                }
                            })
                    }));

            interview = CreateInterviewData();
            var rosterLevel = new InterviewLevel(new ValueVector<Guid> { rosterId }, null, new decimal[] { 0 });
            interview.Levels.Add("0", rosterLevel);

            if (!rosterLevel.QuestionsSearchCache.ContainsKey(linkedQuestionId))
                rosterLevel.QuestionsSearchCache.Add(linkedQuestionId, new InterviewQuestion(linkedQuestionId));

            var textListQuestion = rosterLevel.QuestionsSearchCache[linkedQuestionId];

            textListQuestion.Answer = new decimal[] { 0, 0 };

            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
             result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(questionnarie,1),
                interview);

        It should_linked_question_have_one_answer = () =>
           GetLevel(result, new[] { rosterId }).Records[0].GetPlainAnswers().First().Length.ShouldEqual(2);

        It should_linked_question_have_first_answer_be_equal_to_0 = () =>
           GetLevel(result, new[] { rosterId }).Records[0].GetPlainAnswers().First().First().ShouldEqual("0");

      
        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid linkedQuestionId;
        private static Guid linkedQuestionSourceId;
        private static QuestionnaireDocument questionnarie;
        private static ExportViewFactory exportViewFactory;
        private static InterviewData interview;
    }
}
