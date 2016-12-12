using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_interview_export_view_by_interview_with_linked_question_referenced_on_third_level_roster : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionSourceId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("13333333333333333333333333333333");
            var nestedRosterId = Guid.Parse("23333333333333333333333333333333");

            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new SingleQuestion()
                {
                    PublicKey = linkedQuestionId,
                    QuestionType = QuestionType.SingleOption,
                    LinkedToQuestionId = linkedQuestionSourceId
                },
                Create.Entity.FixedRoster(rosterId: rosterId, obsoleteFixedTitles: new[] {"t1", "t2"},
                    children: new IComposite[]
                    {
                        Create.Entity.FixedRoster(rosterId: nestedRosterId, obsoleteFixedTitles: new[] {"n1", "n2"},
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

            if (!interview.Levels["#"].QuestionsSearchCache.ContainsKey(linkedQuestionId))
                interview.Levels["#"].QuestionsSearchCache.Add(linkedQuestionId, new InterviewQuestion(linkedQuestionId));

            var textListQuestion = interview.Levels["#"].QuestionsSearchCache[linkedQuestionId];

            textListQuestion.Answer = new decimal[] { 0.1m , 0.4m };

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(new PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
        };

        Because of = () =>
        {
            result =
                exportViewFactory.CreateInterviewDataExportView(
                    exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaire.PublicKey, 1)), interview);
        };

        It should_linked_question_have_one_answer = () =>
           GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().Length.ShouldEqual(1);

        It should_linked_question_have_first_answer_be_equal_to_0 = () =>
           GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().First().ShouldEqual("[0.1|0.4]");

        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid linkedQuestionId;
        private static Guid linkedQuestionSourceId;
        private static QuestionnaireDocument questionnaire;
        private static InterviewData interview;
        private static ExportViewFactory exportViewFactory;
    }
}
