using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_filtered_combobox_question_that_was_cascading_combobox_and_options_are_more_then_200 : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            int incrementer = 0;
            oldAnswers = new Answer[210].Select(
                answer =>
                    new Answer
                    {
                        AnswerValue = incrementer.ToString(),
                        AnswerText = (incrementer++).ToString()
                    }).ToArray();

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded
            (
                publicKey : parentQuestionId,
                answers : new Answer[] { new Answer{ AnswerText = "option1", AnswerValue = "1"}, new Answer{AnswerText = "option2", AnswerValue = "2"}},
                groupPublicKey : chapterId,
                questionText : "Parent question",
                questionType : QuestionType.SingleOption,
                stataExportCaption : "cascade_parent",
                featured : false,
                responsibleId : responsibleId,
                linkedToQuestionId : null,
                isFilteredCombobox : false,
                cascadeFromQuestionId : null
            ));

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded
            (
                publicKey : cascadeQuestionId,
                answers : oldAnswers,
                groupPublicKey : chapterId,
                questionText : "Cascade question",
                questionType : QuestionType.SingleOption,
                stataExportCaption : "cascade",
                featured : false,
                responsibleId : responsibleId,
                linkedToQuestionId : null,
                isFilteredCombobox : false,
                cascadeFromQuestionId : parentQuestionId
            ));
            
            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.UpdateSingleOptionQuestion(
                questionId: cascadeQuestionId,
                title: "title",
                variableName: "qr_barcode_question",
                variableLabel: null,
                isPreFilled: false,
                scope: QuestionScope.Interviewer,
                enablementCondition: "some condition",
                hideIfDisabled: false,
                instructions: "intructions",
                responsibleId: responsibleId,
                options: null,
                linkedToEntityId: (Guid?)null,
                isFilteredCombobox: true,
                cascadeFromQuestionId: null, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null, properties: Create.QuestionProperties());

        private Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionChanged_event = () =>
            eventContext.ShouldContainEvent<QuestionChanged>();

        It should_raise_QuestionChanged_event_with_answer_option_that_was_presiously_saved = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Answers.Count().ShouldEqual(oldAnswers.Count());

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid cascadeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Answer[] oldAnswers;
    }
}