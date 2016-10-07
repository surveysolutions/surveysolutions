using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
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

            questionnaire.AddVariableAndMoveIfNeeded(new AddVariable(questionnaire.Id,  variableId, new VariableData(VariableType.String, variableName, ""), responsibleId, parentId: rosterId ));

            questionnaire.AddTextQuestion(questionWithSubstitutionId, chapterId, responsibleId);
            

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => exception =
            Catch.Exception(() => questionnaire.UpdateTextQuestion(
                new UpdateTextQuestion(
                    questionnaire.Id,
                    questionWithSubstitutionId,
                    responsibleId,
                    new CommonQuestionParameters() {Title = "title", VariableName = "var", HideIfDisabled = false},
                    validationConditions: new List<ValidationCondition>() {new ValidationCondition
                    {
                        Message = $"error message with substitution %{variableName}%"
                    }
                    },
                    
                    isPreFilled: false,
                    scope: QuestionScope.Interviewer,
                    mask: null)));

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