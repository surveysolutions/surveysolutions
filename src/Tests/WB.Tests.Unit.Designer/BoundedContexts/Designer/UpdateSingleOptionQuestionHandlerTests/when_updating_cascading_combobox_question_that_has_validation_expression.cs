using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_cascading_combobox_question_that_has_validation_expression : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(Create.Event.AddGroup(chapterId));
            questionnaire.Apply(Create.Event.AddTextQuestion(parentQuestionId, chapterId));
            questionnaire.Apply(Create.Event.QuestionChanged(parentQuestionId, "cascade_parent", questionType: QuestionType.SingleOption));
            questionnaire.Apply(Create.Event.AddTextQuestion(cascadingId, chapterId));
            questionnaire.Apply(Create.Event.QuestionChanged(cascadingId, "cascade_child", questionType: QuestionType.SingleOption));
        };

        Because of = () =>
            exception = Catch.Exception(() => questionnaire.UpdateSingleOptionQuestion(
                questionId: cascadingId,
                title: "title",
                variableName: "cascade_child",
                variableLabel: null,
                isPreFilled: false,
                scope: QuestionScope.Interviewer,
                enablementCondition: null,
                hideIfDisabled: false,
                instructions: "intructions",
                responsibleId: responsibleId,
                options: null,
                linkedToEntityId: (Guid?)null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: parentQuestionId, 
                validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>
                {
                    new ValidationCondition { }
                },
                linkedFilterExpression: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__answer_title_cannot_be_empty__ = () =>
            new[] { "cascading questions can't have validation condition" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid cascadingId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

        private static Answer[] oldAnswers = new Answer[]
        {
            new Answer { AnswerText = "option1", AnswerValue = "1", ParentValue = "1"}, new Answer { AnswerText = "option2", AnswerValue = "2", ParentValue = "2" }
        };
    }
}