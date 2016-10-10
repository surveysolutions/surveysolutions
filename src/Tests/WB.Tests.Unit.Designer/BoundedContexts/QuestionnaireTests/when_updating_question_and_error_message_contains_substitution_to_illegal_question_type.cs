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
    internal class when_updating_question_and_error_message_contains_substitution_to_illegal_question_type : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(Create.Event.AddGroup(groupId: chapterId));
            questionnaire.AddTextQuestion(textQuestionId, chapterId, responsibleId);
            questionnaire.UpdateQuestion(Create.Event.QuestionChanged(publicKey: textQuestionId, groupPublicKey: chapterId,
                questionType: QuestionType.GpsCoordinates, stataExportCaption: textQuestionVariable));
            questionnaire.AddTextQuestion(questionWithSubstitutionId, chapterId, responsibleId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => exception =
            Catch.Exception(() => questionnaire.UpdateNumericQuestion(
                new UpdateNumericQuestion(
                    questionnaire.Id,
                    questionWithSubstitutionId,
                    responsibleId,
                    new CommonQuestionParameters() { Title = "title", VariableName = "var1" },
                    false,
                    QuestionScope.Interviewer,
                    useFormatting: false,
                    isInteger: false,
                    countOfDecimalPlaces: null,
                    validationConditions: new List<ValidationCondition>() {new ValidationCondition
                    {
                        Message = $"error message with substitution %{textQuestionVariable}%"
                    }
                    })));

        It should_exception_has_specified_message = () =>
            new[] { "substitution", "to", "illegal", "type", textQuestionVariable }.ShouldEachConformTo(x =>
                ((QuestionnaireException) exception).Message.Contains(x));

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid questionWithSubstitutionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid textQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static string textQuestionVariable = "hhname";
        private static Exception exception;
    }
}