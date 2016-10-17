using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_question_and_error_message_contains_substitution_to_self : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddTextQuestion(textQuestionId, chapterId, responsibleId);

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
                 textQuestionId,
                 responsibleId,
                new CommonQuestionParameters() {Title = "title", VariableName = textQuestionVariable},
                isPreFilled:false,
                validationConditions: new List<ValidationCondition>() {new ValidationCondition
                {
                    Message = $"error message with substitution %{textQuestionVariable}%"
                } },
                
                scope: QuestionScope.Interviewer,
                mask: null)));

        It should_exception_has_specified_message = () =>
            new[] { "substitution", "to", "self" }.ShouldEachConformTo(x =>
                ((QuestionnaireException) exception).Message.Contains(x));

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid textQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static string textQuestionVariable = "hhname";
        private static Exception exception;
    }
}