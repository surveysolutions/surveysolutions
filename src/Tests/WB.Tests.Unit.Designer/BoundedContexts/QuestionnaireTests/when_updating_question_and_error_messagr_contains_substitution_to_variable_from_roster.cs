using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_question_and_error_messagr_contains_substitution_to_variable_from_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(Create.Event.AddGroup(groupId: chapterId));
            questionnaire.AddGroup(Create.Event.AddGroup(rosterId));
            questionnaire.MarkGroupAsRoster(Create.Event.GroupBecameRoster(rosterId));
            questionnaire.AddVariable(Create.Event.VariableAdded(entityId: variableId, parentId: rosterId, variableName: variableName));
            questionnaire.AddQuestion(Create.Event.AddTextQuestion(questionId: questionWithSubstitutionId, parentId: chapterId));

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => exception =
            Catch.Exception(() => questionnaire.UpdateTextQuestion(
                questionId: questionWithSubstitutionId,
                responsibleId: responsibleId,
                title: "title",
                hideIfDisabled: false,
                variableName: "var",
                validationCoditions: new List<ValidationCondition>() {new ValidationCondition
                {
                    Message = $"error message with substitution %{variableName}%"
                } },
                variableLabel: null,
                isPreFilled: false,
                scope: QuestionScope.Interviewer,
                enablementCondition: null,
                instructions: null,
                mask: null,
                properties: null));

        It should_exception_has_specified_message = () =>
            new[] { "illegal", "substitution", "to", variableName }.ShouldEachConformTo(x =>
                ((QuestionnaireException) exception).Message.Contains(x));

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid questionWithSubstitutionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid variableId = Guid.Parse("22222222222222222222222222222222");
        private static string variableName = "hhname";
        private static Exception exception;
    }
}